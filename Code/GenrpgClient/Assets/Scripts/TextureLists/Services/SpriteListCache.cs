using OxDb.Client.Assets.Constants;
using OxDb.Client.Assets.Services;
using OxDb.Client.Assets.Textures;
using OxDb.Client.Core.Interfaces;
using OxDb.Client.GameObjects;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.TextureLists.Services
{

    public delegate void DownloadSpriteListHandler(object spriteList, object data);

    public interface ISpriteListCache : IInitializable, IClientResetCleanup, IAssetSubsystem
    {
        void LoadSpriteList(string textureName, DownloadSpriteListHandler handler, object data, CancellationToken token);
    }
    public class DownloadSpriteListArgs
    {
        public object Data;
        public DownloadSpriteListHandler Handler;
        public string TextureName;
        public CachedSpriteList TextureList;
    }

    public class CachedSpriteList
    {
        public string Name;
        public SpriteList SpriteList;

        private List<AnimatedSprite> _refs = new List<AnimatedSprite>();

        public void AddRef(AnimatedSprite sprite)
        {
            if (sprite != null && !_refs.Contains(sprite))
            {
                _refs.Add(sprite);
            }
        }


        public void RemoveRef(AnimatedSprite sprite)
        {
            _refs.Remove(sprite);
        }

        public bool HasReferences()
        {
            return _refs.FastAny(x => !(x is null));
        }
    }

    public class SpriteListCache : ISpriteListCache
    {

        private IAssetService _assetService = null;
        private ISingletonContainer _singletonContainer;
        private IClientEntityService _clientEntityService = null;

        private GameObject _textureListParent;

        private Dictionary<string, CachedSpriteList> _textureListCache = new Dictionary<string, CachedSpriteList>();

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private GameObject GetTextureListParent()
        {
            if (_textureListParent == null)
            {
                _textureListParent = _singletonContainer.GetAssetParent<TextureList>();
            }
            return _textureListParent;
        }

        public void LoadSpriteList(string textureName, DownloadSpriteListHandler handler, object data, CancellationToken token)
        {

            DownloadSpriteListArgs downloadData = new DownloadSpriteListArgs()
            {
                Handler = handler,
                Data = data,
                TextureName = textureName
            };

            string assetCategoryNames = AssetCategoryNames.SpriteLists;

            if (textureName.ToLower().IndexOf("portrait") == 0)
            {
                assetCategoryNames = AssetCategoryNames.Portraits;
            }

            _assetService.LoadAssetInto(GetTextureListParent(), assetCategoryNames, textureName, OnDownloadTextureList, token, downloadData);
        }

        public async Task OnReset(CancellationToken token)
        {
            _clientEntityService.DestroyAllChildren(GetTextureListParent());
            _textureListCache = new Dictionary<string, CachedSpriteList>();
            await Task.CompletedTask;
        }

        private void OnDownloadTextureList(GameObject go, DownloadSpriteListArgs downloadData, CancellationToken token)
        {

            if (downloadData == null || string.IsNullOrEmpty(downloadData.TextureName) || go == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            if (_textureListCache.TryGetValue(downloadData.TextureName, out CachedSpriteList cachedSpriteList))
            {
                _clientEntityService.Destroy(go);
            }
            else
            {
                SpriteList spriteList = go.GetComponent<SpriteList>();
                if (spriteList == null)
                {
                    _clientEntityService.Destroy(go);
                    return;
                }
                _clientEntityService.SetActive(go, false);

                cachedSpriteList = new CachedSpriteList()
                {
                    Name = downloadData.TextureName,
                    SpriteList = spriteList,
                };

                _textureListCache[downloadData.TextureName] = cachedSpriteList;
            }

            downloadData.TextureList = cachedSpriteList;
            if (downloadData.Handler != null)
            {
                downloadData.Handler(cachedSpriteList, downloadData);
            }

        }

        public async Awaitable UpdateAssets(CancellationToken token)
        {
            List<string> emptyLists = _textureListCache.Values.Where(x => !x.HasReferences()).Select(x => x.Name).ToList();

            foreach (string spriteName in emptyLists)
            {
                CachedSpriteList spriteList = _textureListCache[spriteName];

                _clientEntityService.Destroy(spriteList.SpriteList.gameObject);
                _textureListCache.Remove(spriteName);
            }

            await Task.CompletedTask;
        }
    }
}


