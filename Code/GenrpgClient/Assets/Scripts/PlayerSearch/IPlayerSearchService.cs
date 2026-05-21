
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedPlatform.Accounts.PublicData;
using System;
using System.Threading;

namespace Assets.Scripts.PlayerSearch
{
    public interface IPlayerSearchService : IInjectable
    {
        void AccountSearch(string accountId, Action<PublicAccount> handler, CancellationToken token);
        void UserSearch(string userId, Action<PublicUser> handler, CancellationToken token);
        void CharacterSearch(string charId, Action<PublicCharacter> handler, CancellationToken token);
    }
}


