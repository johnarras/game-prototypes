using Genrpg.WebServer.Handlers;
using Genrpg.WebServer.Sessions;
using MessagePack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OxDb.RequestServer.Core;
using OxDb.ServerCore.Logalytics.Utils;
using OxDb.ServerCore.Setup;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Services;
using OxDb.SharedCore.Website.Constants;
using OxDb.SharedCore.Website.Requests.Core;
using OxDb.SharedCore.Website.Requests.Interfaces;
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

                builder.Services.AddAuthentication(options =>
                {
                    // Change defaults to use your custom handler string identifier
                    options.DefaultAuthenticateScheme = "CustomSession";
                    options.DefaultChallengeScheme = "CustomSession";
                    options.DefaultScheme = "CustomSession";
                })
                .AddScheme<CustomSessionOptions, CustomSessionHandler>("CustomSession", options =>
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

                app.MapPost(CoreEndpoints.AccountAuth, async (WebRequestServer webServer, [FromBody] JsonElement json) =>
                {
                    return await HandleRequest(webServer, json, CoreEndpoints.AccountAuth, async (wrs) => { return await webServer.HandleAccountAuth(wrs); });

                }).AllowAnonymous();

                app.MapPost(CoreEndpoints.GameAuth, async (WebRequestServer webServer, [FromBody] JsonElement json) =>
                {
                    return await HandleRequest(webServer, json, CoreEndpoints.GameAuth, async (wrs) => { return await webServer.HandleGameAuth(wrs); });
                }).AllowAnonymous();

                app.MapPost(CoreEndpoints.RefreshToken, async (WebRequestServer webServer, [FromBody] JsonElement json) =>
                {
                    return await HandleRequest(webServer, json, CoreEndpoints.RefreshToken, async (wrs) => { return await webServer.HandleRefreshToken(wrs); });
                }).AllowAnonymous();

                app.MapPost(CoreEndpoints.GameClient, async (IServiceProvider sp, WebRequestServer webServer, ClaimsPrincipal user, [FromBody] JsonElement json) =>
                {
                    UserWebRequestClaimData claimData = new UserWebRequestClaimData()
                    {
                        UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        ExistingData = user.FindFirst(CustomClaimTypes.ExistingData)?.Value,
                        GameSessionId = user.FindFirst(CustomClaimTypes.GameSessionId)?.Value,
                    };

                    return await HandleRequest(webServer, json, CoreEndpoints.GameClient, async (wrs) => { return await webServer.HandleUserClient(wrs, claimData); });
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

                ActivityUtils.SafeAddTag(activity, LogalyticsKeys.GameUserId, requestSet.GameUserId);
                ActivityUtils.SafeAddTag(activity, LogalyticsKeys.ClientVersion, requestSet.ClientVersion);
                ActivityUtils.SafeAddTag(activity, LogalyticsKeys.ClientPlatform, requestSet.ClientPlatform);
                ActivityUtils.SafeAddTag(activity, LogalyticsKeys.RequestId, requestSet.RequestId);
                ActivityUtils.SafeAddTag(activity, LogalyticsKeys.ClientSessionId, requestSet.ClientSessionId);
                ActivityUtils.SafeAddTag(activity, LogalyticsKeys.ClientEnv, requestSet.ClientEnv);

                try
                {
                    string resultString = await func(requestSet);
                    return CreateContentFromString(resultString);
                }
                catch (Exception ex)
                {
                    
                    logService.Exception(ex, "ProcessRequests: " + requestSet.ShowRequestNames());

                    
            
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



