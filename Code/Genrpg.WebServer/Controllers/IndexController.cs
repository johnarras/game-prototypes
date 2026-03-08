using Genrpg.RequestServer.Core;
using Genrpg.Shared.Serialization.Services;
using Genrpg.Shared.Website.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace Genrpg.WebServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get(WebRequestServer webServer)
        {
            return GetContent(webServer.GetIndexString());
        }

        private static readonly NewtonsoftTextSerializer newtonSoftSerializer = new NewtonsoftTextSerializer();

        private WebServerRequestSet ExtractRequestSet(JsonElement elem)
        {
            WebServerRequestEnvelope envelope = elem.Deserialize<WebServerRequestEnvelope>();
            return newtonSoftSerializer.Deserialize<WebServerRequestSet>(envelope.Json);
        }

        [HttpPost]
        [Route("/account-auth")]
        [AllowAnonymous]
        public async Task<IActionResult> PostAccountAuth(WebRequestServer webServer, [FromBody] JsonElement json)
        {

            return GetContent(await webServer.HandleAccountAuth(ExtractRequestSet(json)));
        }

        [HttpPost]
        [Route("/game-auth")]
        [AllowAnonymous]
        public async Task<IActionResult> PostGameAuth(WebRequestServer webServer, [FromBody] JsonElement json)
        {
            return GetContent(await webServer.HandleGameAuth(ExtractRequestSet(json)));
        }

        [HttpPost]
        [Route("/refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> PostRefreshToken(WebRequestServer webServer, [FromBody] JsonElement json)
        {
            return GetContent(await webServer.HandleRefreshToken(ExtractRequestSet(json)));
        }
        [HttpPost]
        [Route("/game-client")]
        [Authorize]
        public async Task<IActionResult> PostClient(WebRequestServer webServer, [FromBody] JsonElement json)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return GetContent(await webServer.HandleUserClient(ExtractRequestSet(json), userId));
        }

        [HttpPost]
        [Route("/nouser")]
        [AllowAnonymous]
        public async Task<IActionResult> PostNoUser(WebRequestServer webServer, [FromBody] JsonElement json)
        {
            return GetContent(await webServer.HandleNoUser(ExtractRequestSet(json)));
        }

        protected IActionResult GetContent(string data)
        {
            return Content(data, "application/json", Encoding.UTF8);
        }
    }
}


