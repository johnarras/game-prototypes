using Genrpg.RequestServer.Core;
using Genrpg.Shared.Config.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

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
            app.Run();
        }
    }
}



