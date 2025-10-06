using InLap.Api.Extensions;
using System.IO;
using DotNetEnv;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicyName);

app.UseAuthorization();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

app.Run();
