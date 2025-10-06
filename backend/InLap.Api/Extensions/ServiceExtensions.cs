using InLap.Infrastructure.Configuration;
using InLap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InLap.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInLapServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<InfrastructureOptions>(configuration);

            var connectionString = configuration.GetConnectionString("Default")
                                   ?? configuration["ConnectionStrings__Default"]
                                   ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<InLapDbContext>(options =>
                options.UseSqlServer(connectionString, sql =>
                {
                    sql.MigrationsAssembly("InLap.Infrastructure");
                }));

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
