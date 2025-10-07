using InLap.Api.Extensions;
using InLap.Api.Middleware;
using System.IO;
using DotNetEnv;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using InLap.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

try
{
    var root = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".."));
    var envPath = Path.Combine(root, ".env");
    if (File.Exists(envPath))
    {
        Env.Load(envPath);
    }
}
catch
{
    Console.WriteLine("Failed to load .env file.");
}

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInLapServices(builder.Configuration);

const string CorsPolicyName = "InLapCors";
var frontendOrigin = builder.Configuration["FRONTEND_ORIGIN"] ?? "http://localhost:4200";
builder.Services.AddInLapCors(CorsPolicyName, new[] { frontendOrigin });

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders |
                      HttpLoggingFields.ResponsePropertiesAndHeaders |
                      HttpLoggingFields.RequestBody |
                      HttpLoggingFields.ResponseBody;
    o.RequestBodyLogLimit = 4096;
    o.ResponseBodyLogLimit = 4096;
});

builder.WebHost.ConfigureKestrel(k =>
{
    k.Limits.MaxRequestBodySize = 1_000_000;
});
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 1_000_000;
});

var app = builder.Build();

app.UseMiddleware<ErrorMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InLapDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicyName);

app.UseAuthorization();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

app.Run();
