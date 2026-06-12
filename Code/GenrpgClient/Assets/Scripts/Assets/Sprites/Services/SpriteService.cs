using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Assets.Entities;
using Assets.Scripts.Assets.Services;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.GameObjects;
using OxDb.SharedCore.Entities.Assets;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

namespace Assets.Scripts.Assets.Sprites.Services
{

    public interface ISpriteService : IInitializable, IClientResetCleanup, IAssetSubsystem
    {
        void SetEntityIcon(long entityTypeId, long entityId, GImage parentImage, CancellationToken token, string forcedIconName = null);
        void SetAtlasSpriteInto(string atlasName, string spriteName, GImage image, CancellationToken token, AssetDownloadHandler<object> handler = null);
        void LoadAtlas(string atlasName, CancellationToken token, AssetDownloadHandler<object> handler = null);
    }

    public class SpriteService : ISpriteService
    {
        protected IAssetService _assetService = null;
        protected IClientEntityService _clientEntityService = null;
        protected ILogService _logService = null;
        protected IClientGameState _gs = null;
        protected IEntityService _entityService = null;
        protected ISingletonContainer _singletonContainer = null;

        protected GameObject _assetParent = null;

        protected Dictionary<string, SpriteAtlasContainer> _atlasCache = new Dictionary<string, SpriteAtlasContainer>();

        private List<string> _missingAssets = new List<string>();

        public async Task Initialize(CancellationToken token)
        {
            _assetParent = _singletonContainer.GetAssetParent<SpriteAtlas>();
            await Task.CompletedTask;
        }

        public async Task OnReset(CancellationToken token)
        {
            foreach (SpriteAtlasContainer container in _atlasCache.Values)
            {
                _clientEntityService.Destroy(container.gameObject);
            }
            _atlasCache.Clear();
            await Task.CompletedTask;
        }

        public void LoadAtlas(string atlasName, CancellationToken token, AssetDownloadHandler<object> handler = null)
        {
            SetAtlasSpriteInto(atlasName, null, null, token, handler);
        }

        public void SetAtlasSpriteInto(string atlasName, string spriteName, GImage parentSprite, CancellationToken token, AssetDownloadHandler<object> handler = null)
        {
            GImage image = parentSprite as GImage;

            if (string.IsNullOrEmpty(atlasName))
            {
                if (handler != null)
                {
                    handler(null, parentSprite, token);
                }
                return;
            }

            if (_atlasCache.TryGetValue(atlasName, out SpriteAtlasContainer cont))
            {
                GetAtlasSprite(cont, image, spriteName, handler, token);
                return;
            }

            AtlasSpriteDownload atlasDownload = new AtlasSpriteDownload()
            {
                AtlasName = atlasName,
                SpriteName = spriteName,
                FinalHandler = handler,
                TargetImage = image,
            };

            _assetService.LoadAssetInto(_assetParent, AssetCategoryNames.Atlas, atlasName, OnDownloadAtlas, token, atlasDownload);
        }

        private void OnDownloadAtlas(GameObject go, AtlasSpriteDownload atlasSpriteDownload, CancellationToken token)
        {
            if (go == null)
            {
                if (atlasSpriteDownload != null && atlasSpriteDownload.FinalHandler != null)
                {
                    atlasSpriteDownload.FinalHandler(null, atlasSpriteDownload.TargetImage, token);
                }

                return;
            }

            if (atlasSpriteDownload == null)
            {
                _clientEntityService.Destroy(go);

                if (atlasSpriteDownload != null && atlasSpriteDownload.FinalHandler != null)
                {
                    atlasSpriteDownload.FinalHandler(null, atlasSpriteDownload.TargetImage, token);
                }
                return;
            }

            SpriteAtlasContainer atlasCont = go.GetComponent<SpriteAtlasContainer>();
            if (atlasCont == null || atlasCont.Atlas == null)
            {
                if (atlasSpriteDownload.FinalHandler != null)
                {
                    atlasSpriteDownload.FinalHandler(null, atlasSpriteDownload.TargetImage, token);
                    _clientEntityService.Destroy(go);
                    return;
                }
            }

            if (!_atlasCache.TryGetValue(atlasSpriteDownload.AtlasName, out SpriteAtlasContainer currAtlasCont))
            {
                _atlasCache[atlasSpriteDownload.AtlasName] = atlasCont;
                atlasCont.UpdateUnloadTime();

            }
            else
            {
                atlasCont = currAtlasCont;
                _clientEntityService.Destroy(go);
            }

            GetAtlasSprite(atlasCont, atlasSpriteDownload.TargetImage, atlasSpriteDownload.SpriteName, atlasSpriteDownload.FinalHandler, token);

        }

        private void GetAtlasSprite(SpriteAtlasContainer cont, GImage image, string spriteName, AssetDownloadHandler<object> handler, CancellationToken token)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                return;
            }
            if (cont.Atlas == null)
            {
                if (!_missingAssets.Contains(cont.name))
                {
                    _logService.Warning($"Missing Atlas in container {cont.name}");
                    _missingAssets.Add(cont.name);
                }
                if (handler != null)
                {
                    handler(null, image, token);
                }
                return;
            }

            Sprite spr = cont.Atlas.GetSprite(spriteName);

            if (spr == null)
            {
                if (!_missingAssets.Contains(cont.name + "." + spriteName))
                {
                    _logService.Warning($"Missing sprite {spriteName} in Atlas {cont.name}");
                    _missingAssets.Add(cont.name + "." + spriteName);
                }
                if (handler != null)
                {
                    handler(null, image, token);
                }
                return;
            }

            if (image == null)
            {
                if (handler != null)
                {
                    handler(null, image, token);
                }
                return;
            }

            image.SetAtlasSprite(cont, spr);

        }

        public void SetEntityIcon(long entityTypeId, long entityId, GImage parentImage, CancellationToken token,
            string forcedIconName = null)
        {
            EntityAtlasIcon icon = _entityService.TryGetEntityIcon(_gs.ch, entityTypeId, entityId, forcedIconName);

            if (icon != null && icon.IsValid())
            {
                string finalIconName = forcedIconName ?? icon.IconName;
                SetAtlasSpriteInto(icon.AtlasName, finalIconName, parentImage, token);
            }
            else
            {
                if (!_missingAssets.Contains(entityTypeId + "." + entityId))
                {
                    //_logService.Info("Missing icon for " + entityTypeId + " " + entityId);
                    _missingAssets.Add(entityTypeId + "." + entityId);
                }
            }
        }

        public async Awaitable UpdateAssets(CancellationToken token)
        {
            List<SpriteAtlasContainer> emptyContainers = _atlasCache.Values.Where(x => x.CanUnload()).ToList();

            foreach (SpriteAtlasContainer emptyCont in emptyContainers)
            {
                _atlasCache.Remove(emptyCont.name);
                _clientEntityService.Destroy(emptyCont.gameObject);
            }
            await Task.CompletedTask;
        }
    }
}


