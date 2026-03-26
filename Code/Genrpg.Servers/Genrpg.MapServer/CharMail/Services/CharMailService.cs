using Genrpg.MapServer.CharMail.LetterHelpers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.HelperClasses;
using System.Threading.Tasks;

namespace Genrpg.MapServer.CharMail.Services
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


