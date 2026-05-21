using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.UserMail.PlayerData;

namespace OxDb.RequestServer.UserMail.LetterHelpers
{
    public interface IUserLetterHelper : ISetupDictionaryItem<long>
    {
        Task ProcessLetter(WebContext context, UserLetter letter);
    }
}


