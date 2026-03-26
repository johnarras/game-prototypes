using Genrpg.RequestServer.Core;
using Genrpg.Shared.Config.Constants;
using Genrpg.Shared.Serialization.Services;
using Genrpg.Shared.Website.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Genrpg.WebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication("DefaultBearer")
            .AddScheme<CustomSessionOptions, CustomSessionHandler>("DefaultBearer", options =>
            {
                options.TokenSecret = System.Configuration.ConfigurationManager.AppSettings[AppConfigKeys.TokenSecret];
            });
            builder.Services.AddAuthorization();

            builder.Services.AddControllers();
            builder.Services.Add(new ServiceDescriptor(typeof(WebRequestServer), new WebRequestServer()));
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

            WebApplication app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.MapGet("/Index", async (WebRequestServer webServer) =>
            {
                return GetContent(webServer.GetIndexString());
            }).AllowAnonymous();

            app.MapPost("/account-auth", async (WebRequestServer webServer, [FromBody] JsonElement json) =>
            {

                return GetContent(await webServer.HandleAccountAuth(ExtractRequestSet(json)));
            }).AllowAnonymous();

            app.MapPost("/game-auth", async (WebRequestServer webServer, [FromBody] JsonElement json) =>
            {
                return GetContent(await webServer.HandleGameAuth(ExtractRequestSet(json)));
            }).AllowAnonymous();

            app.MapPost("/refresh-token", async (WebRequestServer webServer, [FromBody] JsonElement json) =>
            {
                return GetContent(await webServer.HandleRefreshToken(ExtractRequestSet(json)));
            }).AllowAnonymous();


            app.MapPost("/nouser", async (WebRequestServer webServer, [FromBody] JsonElement json) =>
            {
                return GetContent(await webServer.HandleNoUser(ExtractRequestSet(json)));
            }).AllowAnonymous();


            app.MapPost("/game-client", async (WebRequestServer webServer, ClaimsPrincipal user, [FromBody] JsonElement json) =>
            {
                string userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return GetContent(await webServer.HandleUserClient(ExtractRequestSet(json), userId));
            }).RequireAuthorization();

            app.Run();

        }

        private static readonly NewtonsoftTextSerializer newtonSoftSerializer = new NewtonsoftTextSerializer();

        protected static IResult GetContent(string data)
        {
            return Results.Content(data, "application/json", Encoding.UTF8);
        }
        private static WebServerRequestSet ExtractRequestSet(JsonElement elem)
        {
            WebServerRequestEnvelope envelope = elem.Deserialize<WebServerRequestEnvelope>();
            return newtonSoftSerializer.Deserialize<WebServerRequestSet>(envelope.Json);
        }
    }
}



