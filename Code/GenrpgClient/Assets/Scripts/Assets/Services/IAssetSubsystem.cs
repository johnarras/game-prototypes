using Genrpg.Shared.Interfaces;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Assets.Services
{
    public interface IAssetSubsystem : IInjectable
    {
        Awaitable UpdateAssets(CancellationToken token);
    }
}
