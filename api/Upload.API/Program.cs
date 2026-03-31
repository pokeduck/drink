using System.Text.Json;
using Drink.Application.Conventions;
using Drink.Application.Extensions;
using Drink.Infrastructure.Extensions;
using Drink.Upload.API.Middleware;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.AddSerilog();

// kebab-case routes + snake_case query binding + JSON snake_case
builder.Services.AddControllers(options =>
    {
      options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
      options.ValueProviderFactories.Insert(0, new SnakeCaseQueryValueProviderFactory());
    })
    .AddJsonOptions(options =>
    {
      options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

// File Upload (UploadSettings + IFileStorageService)
builder.Services.AddFileUpload(builder.Configuration);

// Application Services (auto-scan all BaseService subclasses)
builder.Services.AddApplicationServices();

// AutoMapper (auto-scan all Profiles)
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));

// HttpContextAccessor (required by BaseService)
builder.Services.AddHttpContextAccessor();

// CORS (for static file reads from frontend)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(policy =>
  {
    policy.WithOrigins(allowedOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod();
  });
});

// Swagger (dev only)
builder.Services.AddSwagger("Drink Upload API");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// Static files: /assets → Uploads/
app.UseAssetFileServer(builder.Configuration);

app.UseSerilogLogging();

app.UseCors();

// API Key authentication for upload endpoints (skips /assets)
app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();
