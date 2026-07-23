using System.Threading;
using UnityEngine;

namespace OxDb.Client.Assets.Services
{
    public interface IAssetSubsystem
    {
        Awaitable UpdateAssets(CancellationToken token);
    }
}


