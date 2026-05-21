using OxDb.MapServer.CharMail.LetterHelpers;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedGame.Characters.PlayerData;
using System.Threading.Tasks;

namespace OxDb.MapServer.CharMail.Services
{
    public class CharMailService : ICharMailService
    {
        SetupDictionaryContainer<long, ICharLetterHelper> _mailHelpers = new SetupDictionaryContainer<long, ICharLetterHelper>();

        protected IRepositoryService _repoService;

        public async Task ProcessMail(Character ch, string charLetterID)
        {
            await Task.CompletedTask;
        }
    }
}


