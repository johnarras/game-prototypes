
using Assets.Scripts.PlayerSearch;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedPlatform.Accounts.PublicData;
using System;
using System.Threading;

namespace Assets.Scripts.PlayerSearches
{
    public class PlayerSearchService : IPlayerSearchService
    {

        private IFileDownloadService _downloadService = null;
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


