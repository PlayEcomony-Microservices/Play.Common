#nullable disable

using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using GreenPipes.Configurators;
using MassTransit.Definition;
using GreenPipes;

namespace Play.Common.MassTransit
{
    public static class Extensions
    {
        private const string RabbitMq = "RABBITMQ";
        private const string ServiceBus = "SERVICEBUS";
        public static IServiceCollection AddMassTransitWithMessageBroker(this IServiceCollection services,
        IConfiguration configuration,
        Action<IRetryConfigurator> configureRetries = null)
        {
            var serviceSettings = configuration?.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

            switch (serviceSettings.MessageBroker?.ToUpper())
            {
                case ServiceBus:
                    services.AddMassTransitWithServiceBus(configureRetries);
                    break;
                case RabbitMq:
                default:
                    services.AddMassTransitWithRabbitMq(configureRetries);
                    break;
            }

            return services;
        }
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, Action<IRetryConfigurator> configureRetries = null)
        {

            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.UsingPlayEconomyRabbitMq(configureRetries);
            });

            services.AddMassTransitHostedService();

            return services;
        }
        public static IServiceCollection AddMassTransitWithServiceBus(this IServiceCollection services, Action<IRetryConfigurator> configureRetries = null)
        {

            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.UsingPlayEconomyAzureServiceBus(configureRetries);
            });

            services.AddMassTransitHostedService();

            return services;
        }

        public static void UsingPlayEconomyMessageBroker(this IServiceCollectionBusConfigurator configure,
        IConfiguration configuration,
        Action<IRetryConfigurator> configureRetries = null)
        {
            var serviceSettings = configuration?.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

            switch (serviceSettings.MessageBroker?.ToUpper())
            {
                case ServiceBus:
                    configure.UsingPlayEconomyAzureServiceBus(configureRetries);
                    break;
                case RabbitMq:
                default:
                    configure.UsingPlayEconomyRabbitMq(configureRetries);
                    break;
            }            
        }
        public static void UsingPlayEconomyRabbitMq(this IServiceCollectionBusConfigurator configure, Action<IRetryConfigurator> configureRetries = null)
        {
            configure.UsingRabbitMq((context, configurator) =>
            {
                var configuration = context.GetService<IConfiguration>();
                var serviceSettings = configuration?.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var rabbitMQSettings = configuration?.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                configurator.Host(rabbitMQSettings?.Host);
                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings?.ServiceName, false));

                if (configureRetries is null)
                {
                    configureRetries = (retryConfigurator) =>
                        retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                }
                //enable message retry
                configurator.UseMessageRetry(configureRetries);
            });
        }
        public static void UsingPlayEconomyAzureServiceBus(this IServiceCollectionBusConfigurator configure, Action<IRetryConfigurator> configureRetries = null)
        {
            configure.UsingAzureServiceBus((context, configurator) =>
            {
                var configuration = context.GetService<IConfiguration>();
                var serviceSettings = configuration?.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var rabbitMQSettings = configuration?.GetSection(nameof(ServiceBusSettings)).Get<ServiceBusSettings>();
                configurator.Host(rabbitMQSettings?.ConnectionString);
                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings?.ServiceName, false));

                if (configureRetries is null)
                {
                    configureRetries = (retryConfigurator) =>
                        retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                }
                //enable message retry
                configurator.UseMessageRetry(configureRetries);
            });
        }
    }
}