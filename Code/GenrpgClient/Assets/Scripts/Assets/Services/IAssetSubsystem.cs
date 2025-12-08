using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Assets.Services
{
    public interface IAssetSubsystem
    {
        Awaitable UpdateAssets(CancellationToken token);
    }
}
