using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using UnityEngine;


namespace Assets.Scripts.GameSettings.Services
{
    public interface IClientGameDataService : IInitializable
    {
        Awaitable SaveSettings(IGameSettings settings);

        Awaitable LoadCachedSettings(IClientGameState gs, bool useBakedSettings);

    }
}
