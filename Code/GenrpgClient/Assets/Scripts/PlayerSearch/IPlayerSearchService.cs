using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Interfaces;
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
