using Assets.Scripts.Assets;
using Assets.Scripts.Assets.Textures;
using Assets.Scripts.GameObjects;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.TextureLists.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.TextureLists.Services
{

    public class DownloadTextureListData
    {
        public object Data;
        public DownloadTextureListHandler Handler;
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
            return _refs.Any(x => !(x is null));
        }

    }

    public class TextureListCache : ITextureListCache
    {

        private IAssetService _assetService;
        private ISingletonContainer _singletonContainer;
        private IClientEntityService _clientEntityService;

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
                _textureListParent = _singletonContainer.GetSingleton("TextureListParent");
            }
            return _textureListParent;
        }

        public void LoadTextureList(string textureName, DownloadTextureListHandler handler, object data, CancellationToken token)
        {

            DownloadTextureListData downloadData = new DownloadTextureListData()
            {
                Handler = handler,
                Data = data,
                TextureName = textureName
            };

            _assetService.LoadAssetInto(GetTextureListParent(), AssetCategoryNames.TextureLists, textureName, OnDownloadTextureList, downloadData, token);
        }

        public async Task OnClientResetCleanup(CancellationToken token)
        {
            _clientEntityService.DestroyAllChildren(GetTextureListParent());
            _textureListCache = new Dictionary<string, CachedSpriteList>();
            await Task.CompletedTask;
        }

        private void OnDownloadTextureList(object obj, object data, CancellationToken token)
        {

            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            DownloadTextureListData downloadData = data as DownloadTextureListData;

            if (downloadData == null || string.IsNullOrEmpty(downloadData.TextureName))
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
