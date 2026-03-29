using System.Text.Json;
using Drink.Application.Middleware;
using Drink.Application.Extensions;
using Drink.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.AddSerilog();

// JSON snake_case
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
      options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

// Infrastructure (DbContext, Repository, JWT settings, HttpContextAccessor)
builder.Services.AddInfrastructure(builder.Configuration);

// Application Services (auto-scan all BaseService subclasses, exclude Upload-only services)
builder.Services.AddApplicationServices(typeof(Drink.Application.Services.FileUploadService));

// AutoMapper (auto-scan all Profiles)
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Authorization
builder.Services.AddAuthorization();

// Upload API proxy
builder.Services.Configure<Drink.Infrastructure.Settings.UploadApiSettings>(
    builder.Configuration.GetSection("UploadApi"));
builder.Services.AddHttpClient();

// CORS
builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(policy =>
  {
    policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
  });
});

// Swagger (dev only)
builder.Services.AddSwagger("Drink User API");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseSerilogLogging();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();