using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Drink.Application.Extensions;

public static class SwaggerExtensions
{
  public static IServiceCollection AddSwagger(this IServiceCollection services, string title)
  {
    services.AddSwaggerGen(options =>
    {
      options.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });

      // XML comments
      var entryAssembly = Assembly.GetEntryAssembly();
      if (entryAssembly is not null)
      {
        var xmlFile = $"{entryAssembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
          options.IncludeXmlComments(xmlPath);
      }

      // JWT Bearer
      options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "輸入 JWT Token"
      });

      options.AddSecurityRequirement(doc =>
      {
        var requirement = new OpenApiSecurityRequirement();
        var schemeRef = new OpenApiSecuritySchemeReference("Bearer", doc);
        requirement.Add(schemeRef, []);
        return requirement;
      });

      // snake_case property names
      options.SchemaFilter<SnakeCaseSchemaFilter>();
    });

    return services;
  }
}

internal class SnakeCaseSchemaFilter : ISchemaFilter
{
  public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
  {
    if (schema.Properties is null || schema.Properties.Count == 0)
      return;

    var original = schema.Properties.ToList();
    schema.Properties.Clear();
    foreach (var (key, value) in original)
    {
      var snakeKey = JsonNamingPolicy.SnakeCaseLower.ConvertName(key);
      schema.Properties[snakeKey] = value;
    }
  }
}
