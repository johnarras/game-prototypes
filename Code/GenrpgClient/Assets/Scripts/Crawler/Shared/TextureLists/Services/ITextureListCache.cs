using Assets.Scripts.Assets.Services;
using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Threading;

namespace Genrpg.Shared.Crawler.TextureLists.Services
{

    public delegate void DownloadTextureListHandler(object textureList, object data);

    public interface ITextureListCache : IInitializable, IClientResetCleanup, IAssetSubsystem
    {
        void LoadTextureList(string textureName, DownloadTextureListHandler handler, object data, CancellationToken token);
    }
}
