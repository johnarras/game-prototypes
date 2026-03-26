using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.UserMail.Services
{
    public interface IUserMailService : IInjectable
    {
        Task ProcessMail(WebContext context);
    }
}


