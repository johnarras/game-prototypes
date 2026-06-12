using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OxDb.ServerCore.AzureImpl.Logalytics;
using OxDb.ServerCore.Config;
using OxDb.ServerCore.Logalytics.Entities;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using System.Diagnostics;

namespace OxDb.ServerCore.Setup
{
    public class DotNetServiceConfiguration
    {
        public const string DefaultClientName = "Default";

        public const string DefaultActivityName = "DefaultActivity";

        public static string ActivityName { get; private set; } = DefaultActivityName;


        private static IHttpClientFactory _httpClientFactory = null;

        public static readonly ActivitySource Source = new ActivitySource(DefaultActivityName);

        private static ILogService _logService = null;
        public static void ConfigureCoreServices(IHostApplicationBuilder builder, string gameComponent)
        {

            Dictionary<string, object?> serverDict = new Dictionary<string, object?>();

            ServerConfigUtils.AddHardCodedValueToDictionary(serverDict, LogalyticsKeys.ServerVersion, AppConfigKeys.ServerVersion, null);
            ServerConfigUtils.AddHardCodedValueToDictionary(serverDict, LogalyticsKeys.ProductName, AppConfigKeys.ProductName, null);
            ServerConfigUtils.AddHardCodedValueToDictionary(serverDict, LogalyticsKeys.ServerEnv, AppConfigKeys.DefaultEnv, null);
            serverDict[LogalyticsKeys.GameComponent] = gameComponent;

            LogRecordEnricher logEnricher = new LogRecordEnricher(serverDict);

            GlobalRequestActivityEnricher activityEnricher = new GlobalRequestActivityEnricher(serverDict);

            List<string> bannedTracingDomains = new List<string>()
            {
                "core.windows.net",
                "cosmos.azure.com",
            };

            List<string> warningOnlyAssemblies = new List<string>()
            {
                "Microsoft",
                "System",
                "Microsoft.AspNetCore.Hosting.Diagnostics",
                "Microsoft.AspNetCore.Routing.EndpointMiddleware",
                "Microsoft.AspNetCore.Mvc.Infrastructure",
            };

            List<string> dropPaths = new List<string>()
            {
                "/healthz",
                "/livez",
                "/readyz",
            };

            string connectionString = ServerConfigUtils.GetHardcodedConfigValue(AppConfigKeys.AppInsightsConnectionString);

            // 2. Configure the Resource (Static Server Attributes)
            ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault();
            resourceBuilder.AddAttributes(serverDict);

            builder.Services.AddOpenTelemetry()
           .UseAzureMonitor(opts =>
           {
               opts.ConnectionString = connectionString;
           })
           .WithMetrics(metrics =>
           {
               // 2. Pass the static Drop property to silence the metric extractor logs
               metrics.AddView(
               instrumentName: "http.server.request.duration",
               metricStreamConfiguration: MetricStreamConfiguration.Drop);
           })
            .WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(resourceBuilder);
                // Explicitly subscribe to your custom activity source
                tracing.AddSource(Source.Name);
                tracing.AddProcessor(new StorageDependencyFilterProcessor());
                tracing.AddProcessor(activityEnricher);
                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = (httpContext) =>
                    {
                        string? path = httpContext.Request.Path.Value;
                        if (path != null)
                        {

                            foreach (string dropPath in dropPaths)
                            {
                                if (path.Contains(dropPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    return false;
                                }
                            }
                        }
                        return true;
                    };
                });
                tracing.AddHttpClientInstrumentation(options =>
                 {
                     options.FilterHttpRequestMessage = (req) =>
                     {
                         string host = req.RequestUri.Host;

                         return !bannedTracingDomains.Any(x => x.Contains(host));
                     };
                 });

                tracing.AddProcessor(new AzureMetricExtractorSanitizer(dropPaths));
            })
            .WithLogging(logging =>
            {
                // 1. Set the resource builder so logs get the serverDict attributes automatically
                logging.SetResourceBuilder(resourceBuilder);

                // 2. Explicitly append your custom LogRecordEnricher if it handles dynamic processing
                logging.AddProcessor(logEnricher);
            }, options =>
            {
                // 3. Configure the underlying OpenTelemetryLoggerOptions correctly within the DI lifecycle
                options.IncludeScopes = true;
                options.IncludeFormattedMessage = true;
            })
            ;


            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            foreach (string assemblyName in warningOnlyAssemblies)
            {
                builder.Logging.AddFilter(assemblyName, LogLevel.Warning);
            }

            builder.Services.AddHttpClient(DefaultClientName).SetHandlerLifetime(TimeSpan.FromMinutes(5));
        }

        public static List<IInjectable> SetupServiceInstances(IHost existingHost, string gameComponent)
        {
            if (existingHost == null)
            {
                HostApplicationBuilder builder = Host.CreateApplicationBuilder();
                ConfigureCoreServices(builder, gameComponent);
                existingHost = builder.Build();
            }
            if (_httpClientFactory == null)
            {
                _httpClientFactory = existingHost.Services.GetRequiredService<IHttpClientFactory>();
            }
            ILoggerFactory factory = existingHost.Services.GetRequiredService<ILoggerFactory>();

            _logService = new AzureAppInsightsLogService(factory.CreateLogger(gameComponent));
            List<IInjectable> retval = new List<IInjectable>();
            retval.Add(_logService);
            return retval;
        }

        public static IHttpClientFactory GetHttpClientFactory()
        {
            return _httpClientFactory;
        }
    }
}
