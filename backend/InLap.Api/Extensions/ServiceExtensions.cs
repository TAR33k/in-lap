using InLap.App.Interfaces;
using InLap.Infrastructure.Configuration;
using InLap.Infrastructure.Persistence;
using InLap.Infrastructure.FileStorage;
using InLap.App.Parsing;
using InLap.App.Summary;
using InLap.Infrastructure.LLM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using InLap.Infrastructure.Persistence;
using InLap.App.UseCases;

namespace InLap.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInLapServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<InfrastructureOptions>(configuration);

            services.PostConfigure<InfrastructureOptions>(opts =>
            {
                var filesBase = configuration["FILES_BASE_PATH"];
                if (!string.IsNullOrWhiteSpace(filesBase))
                {
                    opts.FilesBasePath = filesBase;
                }
                var maxBytesStr = configuration["MAX_UPLOAD_BYTES"];
                if (long.TryParse(maxBytesStr, out var maxBytes) && maxBytes > 0)
                {
                    opts.MaxUploadBytes = maxBytes;
                }
            });

            var connectionString = configuration.GetConnectionString("Default")
                                   ?? configuration["ConnectionStrings__Default"]
                                   ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<InLapDbContext>(options =>
                options.UseSqlServer(connectionString, sql =>
                {
                    sql.MigrationsAssembly("InLap.Infrastructure");
                }));

            services.AddScoped<IFileStore, LocalFileStore>();
            services.AddScoped<IFileParsingService, FileParsingService>();
            services.AddScoped<ISummaryService, SummaryComposer>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddSingleton<ResponseCleaner>();
            services.AddScoped<ProcessUploadUseCase>();

            services.AddHttpClient<ILLMClient, OpenAIHttpClient>((sp, http) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var baseUrl = cfg["OPENAI_BASE_URL"] ?? "https://api.openai.com/v1";
                var apiKey = cfg["OPENAI_API_KEY"];
                http.BaseAddress = new Uri(baseUrl);
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                }
                http.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }

        public static IServiceCollection AddInLapCors(this IServiceCollection services, string policyName, string[] allowedOrigins)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
