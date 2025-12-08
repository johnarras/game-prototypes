using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Assets.Entities
{
    public delegate void AssetDownloadHandler<T>(GameObject obj, T data, CancellationToken token);

    public delegate void FileDownloadHandler(object obj, object data, CancellationToken token);

    public delegate void SpriteListDelegate(object[] sprites);

}
