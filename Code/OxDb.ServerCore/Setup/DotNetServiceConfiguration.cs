using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OxDb.ServerCore.Config;
using OxDb.ServerCore.Logalytics.Entities;
using OxDb.ServerCore.Logalytics.Services;
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
            ServerConfigUtils.AddHardCodedValueToDictionary(serverDict, LogalyticsKeys.ServerProductName, AppConfigKeys.ProductName, null);
            ServerConfigUtils.AddHardCodedValueToDictionary(serverDict, LogalyticsKeys.ServerEnv, AppConfigKeys.DefaultEnv, null);
            serverDict[LogalyticsKeys.GameComponent] = gameComponent;

            LogalyticsEnricher enricher = new LogalyticsEnricher(serverDict);

            // 2. Configure the Resource (Static Server Attributes)
            ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault();
            resourceBuilder.AddAttributes(serverDict);

            builder.Services.AddOpenTelemetry()
            .UseAzureMonitor(options =>
            {
                options.ConnectionString = ServerConfigUtils.GetHardcodedConfigValue(AppConfigKeys.AppInsightsConnectionString);
            })
            .WithTracing(tracing =>
            {
                // Explicitly subscribe to your custom activity source
                tracing.AddSource(Source.Name);
            })
            .WithLogging(logging =>
            {
                logging.AddProcessor(enricher);
            })
            ;
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

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

            _logService = new ServerLogService(factory.CreateLogger(gameComponent));
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
