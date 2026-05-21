using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;

namespace OxDb.RequestServer.UserMail.Services
{
    public interface IUserMailService : IInjectable
    {
        Task ProcessMail(WebContext context);
    }
}


