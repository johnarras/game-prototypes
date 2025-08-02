using Genrpg.RequestServer.Core;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.WebServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Index" };
        }

        [HttpPost]
        [Route("/account-auth")]
        public async Task<string> PostAccountAuth(WebRequestServer webServer, [FromForm] string Data)
        {
            return await webServer.HandleAccountAuth(Data);
        }

        [HttpPost]
        [Route("/game-auth")]
        public async Task<string> PostGameAuth(WebRequestServer webServer, [FromForm] string Data)
        {
            return await webServer.HandleGameAuth(Data);
        }

        [HttpPost]
        [Route("/game-client")]
        public async Task<string> PostClient(WebRequestServer webServer, [FromForm] string Data)
        {
            return await webServer.HandleUserClient(Data);
        }

        [HttpPost]
        [Route("/nouser")]
        public async Task<string> PostNoUser(WebRequestServer webServer, [FromForm] string Data)
        {
            return await webServer.HandleNoUser(Data);
        }

        [HttpGet]
        [Route("/txlist")]
        public async Task<string> PostTxList(WebRequestServer webServer, string address)
        {
            return await webServer.HandleTxList(address);
        }
    }
}
