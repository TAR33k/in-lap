using InLap.App.Interfaces;
using InLap.Infrastructure.Configuration;
using InLap.Infrastructure.Persistence;
using InLap.Infrastructure.FileStorage;
using InLap.App.Parsing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
