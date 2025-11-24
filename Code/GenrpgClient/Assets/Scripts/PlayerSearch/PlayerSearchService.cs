using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.DataGroups;
using System;
using System.Threading;

namespace Assets.Scripts.PlayerSearch
{
    public class PlayerSearchService : IPlayerSearchService
    {

        private IFileDownloadService _downloadService;
        public void AccountSearch(string accountId, Action<PublicAccount> handler, CancellationToken token)
        {
            PlayerSearch(accountId, handler, EDataCategories.Accounts, token);
        }

        public void CharacterSearch(string charId, Action<PublicCharacter> handler, CancellationToken token)
        {
            PlayerSearch(charId, handler, EDataCategories.Players, token);
        }

        public void UserSearch(string userId, Action<PublicUser> handler, CancellationToken token)
        {
            PlayerSearch(userId, handler, EDataCategories.Players, token);
        }


        void PlayerSearch<T>(string Id, Action<T> handler, EDataCategories category, CancellationToken token) where T : class
        {
            _downloadService.DownloadTypedFile<T>(Id, handler, category, token);

        }
    }
}
