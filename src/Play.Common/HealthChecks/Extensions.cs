using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Play.Common.Settings;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Play.Common.HealthChecks
{
    public static class Extensions
    {
        private const string Name = "mongodb";
        private const string ReadyTagName = "ready";
        private const string LiveTagName = "live";
        private const string HealthEndpoint = "health";
        private const int DefaultSeconds = 3;

        public static IHealthChecksBuilder AddMongoDb(this IHealthChecksBuilder builder, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                        Name,
                        serviceProvider =>
                        {
                            var configuration = serviceProvider.GetService<IConfiguration>();
                            var mongoDbSettings = configuration?.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                            var mongoClient = new MongoClient(mongoDbSettings?.ConnectionString);
                            return new MongoDbHealthCheck(mongoClient);
                        },
                        HealthStatus.Unhealthy,
                        new[] { ReadyTagName },
                        TimeSpan.FromSeconds(DefaultSeconds)));
        }


        public static  void MapPlayEconomyHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks($"/{HealthEndpoint}/{ReadyTagName}", new HealthCheckOptions()
            {
                Predicate = check => check.Tags.Contains("ready")
            });
            endpoints.MapHealthChecks($"/{HealthEndpoint}/{LiveTagName}", new HealthCheckOptions()
            {
                Predicate = check => false
            });
        }
    }
}