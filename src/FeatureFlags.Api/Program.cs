using FeatureFlags.Api.Infrastructure;
using FeatureFlags.Core;
using FeatureFlags.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddSingleton<IFeatureFlagRepository>(services =>
{
    var definitions = services.GetRequiredService<IConfiguration>()
        .GetSection(FeatureFlagDefinition.SectionName)
        .Get<FeatureFlagDefinition[]>() ?? [];
    return new InMemoryFeatureFlagRepository(definitions.Select(definition => definition.ToFeatureFlag()));
});
builder.Services.AddSingleton<FeatureFlagService>();

var app = builder.Build();

app.Services.GetRequiredService<IFeatureFlagRepository>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    var contractPath = Path.Combine(app.Environment.ContentRootPath, "openapi.yaml");
    app.MapGet("/openapi/v1.yaml", () => Results.File(contractPath, "application/yaml"));
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.yaml", "Feature Flag Service API v1");
        options.DocumentTitle = "Feature Flag Service API";
    });
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.MapControllers();
app.Run();

public partial class Program;