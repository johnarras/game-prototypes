using Genrpg.RequestServer.Core;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.WebServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return GetContent("[Index]");
        }

        [HttpPost]
        [Route("/account-auth")]
        public async Task<IActionResult> PostAccountAuth(WebRequestServer webServer, [FromForm] string Data)
        {
            return GetContent(await webServer.HandleAccountAuth(Data));
        }

        [HttpPost]
        [Route("/game-auth")]
        public async Task<IActionResult> PostGameAuth(WebRequestServer webServer, [FromForm] string Data)
        {
            return GetContent(await webServer.HandleGameAuth(Data));
        }

        [HttpPost]
        [Route("/game-client")]
        public async Task<IActionResult> PostClient(WebRequestServer webServer, [FromForm] string Data)
        {
            return GetContent(await webServer.HandleUserClient(Data));
        }

        [HttpPost]
        [Route("/nouser")]
        public async Task<IActionResult> PostNoUser(WebRequestServer webServer, [FromForm] string Data)
        {
            return GetContent(await webServer.HandleNoUser(Data));
        }

        protected IActionResult GetContent(string data)
        {
            return Content(data, "application/json", Encoding.UTF8);
        }
    }
}


