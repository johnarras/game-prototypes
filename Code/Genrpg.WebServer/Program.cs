using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OxDb.RequestServer.Core;
using OxDb.ServerCore.Setup;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Services;
using OxDb.SharedCore.Website.Requests.Core;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedCore.Website.Responses.Errors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static OxDb.ServerCore.Setup.DotNetServiceConfiguration;

namespace Genrpg.WebServer
{
    public class Program
    {

        private static readonly NewtonsoftTextSerializer newtonSoftSerializer = new NewtonsoftTextSerializer();

        public static void Main(string[] args)
        {
            try
            {
                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

                builder.Services.AddHttpClient(DefaultClientName).SetHandlerLifetime(TimeSpan.FromMinutes(5));

                builder.Services.AddAuthentication("DefaultBearer")
                .AddScheme<CustomSessionOptions, CustomSessionHandler>("DefaultBearer", options =>
                {
                    options.TokenSecret = System.Configuration.ConfigurationManager.AppSettings[AppConfigKeys.TokenSecret];
                });
                builder.Services.AddAuthorization();
                builder.Services.AddControllers();

                WebRequestServer webServer = new WebRequestServer();
                builder.Services.Add(new ServiceDescriptor(typeof(WebRequestServer), webServer));
                builder.Services.Configure<FormOptions>(options =>
                {
                    options.KeyLengthLimit = int.MaxValue;
                    options.ValueCountLimit = int.MaxValue;
                    options.ValueLengthLimit = int.MaxValue;
                    options.MultipartHeadersLengthLimit = int.MaxValue;
                });

                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = int.MaxValue;
                });

                DotNetServiceConfiguration.ConfigureCoreServices(builder, GameComponentNames.WebServer);

                // This ordering is needed to allow IHttpClientFactory to exist during the normal setup process for the app.
                WebApplication app = builder.Build();

                List<IInjectable> serviceList = DotNetServiceConfiguration.SetupServiceInstances(app, GameComponentNames.WebServer);

                webServer.Init(serviceList);

                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();

                app.MapGet("/Index", async (WebRequestServer webServer) =>
                {
                    return CreateContentFromString(webServer.GetIndexString());
                }).AllowAnonymous();

                app.MapPost("/account-auth", async (WebRequestServer webServer, [FromBody] JsonElement json) =>
                {

                    return await HandleRequest(webServer, json, "AccountAuth", async (wrs) => { return await webServer.HandleAccountAuth(wrs); });

                }).AllowAnonymous();

                app.MapPost("/game-auth", async (WebRequestServer webServer, [FromBody] JsonElement json) =>
                {
                    return await HandleRequest(webServer, json, "GameAuth", async (wrs) => { return await webServer.HandleGameAuth(wrs); });
                }).AllowAnonymous();

                app.MapPost("/refresh-token", async (WebRequestServer webServer, [FromBody] JsonElement json) =>
                {
                    return await HandleRequest(webServer, json, "RefreshToken", async (wrs) => { return await webServer.HandleRefreshToken(wrs); });
                }).AllowAnonymous();

                app.MapPost("/game-client", async (IServiceProvider sp, WebRequestServer webServer, ClaimsPrincipal user, [FromBody] JsonElement json) =>
                {

                    string userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    string existingData = user.FindFirst("ExistingData")?.Value;

                    return await HandleRequest(webServer, json, "GameClient", async (wrs) => { return await webServer.HandleUserClient(wrs, userId, existingData); });
                }).RequireAuthorization();

                app.Run();
            }
            catch (Exception ee)
            {
                Trace.TraceError("Toplevel exception: " + ee.Message + " " + ee.StackTrace);
            }

        }

        private static async Task<IResult> HandleRequest(
          WebRequestServer webServer,
          [FromBody] JsonElement json,
         string activityName,
          Func<WebServerRequestSet, Task<string>> func)
        {
            using Activity? activity = DotNetServiceConfiguration.Source.StartActivity(activityName);
            {
                WebServerRequestSet requestSet = ExtractRequestSetFromJson(json);
                if (requestSet == null)
                {

                    return ShowError("Improper Request Format");
                }
                ILogService logService = webServer.GetLogService();

                activity?.SetTag(LogalyticsKeys.GameUserId, requestSet.GameUserId);
                activity?.SetTag(LogalyticsKeys.ClientVersion, requestSet.ClientVersion);
                activity?.SetTag(LogalyticsKeys.ClientPlatform, requestSet.ClientPlatform);
                activity?.SetTag(LogalyticsKeys.RequestId, requestSet.RequestId);
                activity?.SetTag(LogalyticsKeys.SessionId, requestSet.SessionId);
                activity?.SetTag(LogalyticsKeys.ClientEnv, requestSet.ClientEnv);

                try
                {
                    string resultString = await func(requestSet);
                    return CreateContentFromString(resultString);
                }
                catch (Exception ex)
                {
                    // Rely on structured logging instead of raw string concatenation.
                    // This ensures the stack trace and message are accurately parsed by Azure Monitor.
                    logService.Exception(ex, "An unhandled exception occurred during request execution.");

                    return ShowError(ex.Message + " -- " + ex.StackTrace);
                }
            }
        }

        protected static IResult ShowError(string error)
        {
            WebServerResponseSet responseSet = new WebServerResponseSet();

            responseSet.Responses.Add(new ErrorResponse() { Error = error });

            return CreateContentFromString(newtonSoftSerializer.SerializeToString(responseSet));
        }

        protected static IResult CreateContentFromString(string data)
        {
            return Results.Content(data, "application/json", Encoding.UTF8);
        }
        private static WebServerRequestSet ExtractRequestSetFromJson(JsonElement elem)
        {
            try
            {
                WebServerRequestEnvelope envelope = elem.Deserialize<WebServerRequestEnvelope>();
                return newtonSoftSerializer.Deserialize<WebServerRequestSet>(envelope.Json);
            }
            catch (Exception ee)
            {
                Trace.TraceError("Failed to Deserialize Request Payload " + ee.Message + " " + ee.StackTrace);
            }
            return null;
        }
    }

}



