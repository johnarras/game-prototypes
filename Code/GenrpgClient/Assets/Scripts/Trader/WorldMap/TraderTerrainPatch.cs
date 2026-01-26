using Assets.Scripts.Assets.Textures;
using Assets.Scripts.Awaitables;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.ProcGen.Settings.Textures;
using Genrpg.Shared.Trader.Maps.Settings;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Trader.WorldMap
{
    public class TraderTerrainPatch : BaseBehaviour
    {
        private IAwaitableService _awaitableService = null;
        private IClientAppService _appService = null;

        public SpriteRenderer SpriteRenderer;
        public GameObject SpriteObject;

        private int _x = -1;
        private int _y = -1;

        public int X => _x;
        public int Y => _y;

        public float StartY = -3;
        public float AnimateSeconds = 1.0f;
        private int _animateFrames = 60;

        private bool _hidingNow = false;

        private TraderTerrain _terrain = null;
        public double GetDistanceToPoint(int cx, int cy)
        {
            float dx = cx - _x;
            float dy = cy - _y;

            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public void ShowTerrain(TraderTerrain terrain, int x, int y, int cx, int cy, long biomeIndex)
        {
            _terrain = terrain;
            _x = x;
            _y = y;


            IndexedColor color = _gameData.Get<IndexedColorSettings>(_gs.ch).Get(biomeIndex);

            if (color != null)
            {
                TextureType ttype = _gameData.Get<TextureTypeSettings>(_gs.ch).Get(color.TextureTypeId);

                if (ttype != null)
                {
                    _assetService.LoadAsset<GameObject>(AssetCategoryNames.TerrainTex, ttype.Art, OnDownloadTerrainTexture, gameObject, GetToken());
                }
            }

        }

        private void OnDownloadTerrainTexture(GameObject go, object data, CancellationToken token)
        {
            if (go == null)
            {
                return;
            }

            TextureList tlist = go.GetComponent<TextureList>();

            if (tlist == null || tlist.Textures.Count < 1 || tlist.Textures[0] == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            Sprite sprite = Sprite.Create(tlist.Textures[0], new Rect(0, 0, tlist.Textures[0].width, tlist.Textures[0].height), Vector2.zero);

            SpriteRenderer.sprite = sprite;
            _animateFrames = Mathf.Max(1, (int)(AnimateSeconds * _appService.TargetFrameRate));
            _awaitableService.ForgetAwaitable(AnimateIn());

            transform.localPosition = new Vector3(_x, 0.2f, _y);
        }

        private async Awaitable AnimateIn()
        {
            _hidingNow = false;

            SpriteObject.transform.localPosition = new Vector3(0, StartY, 0);
            for (int f = 0; f < _animateFrames; f++)
            {
                if (_hidingNow)
                {
                    return;
                }
                float dist = (_animateFrames - f) * 1.0f / _animateFrames * StartY;

                dist *= dist;

                SpriteObject.transform.localPosition = new Vector3(0, -dist, 0);
                await Awaitable.NextFrameAsync();
            }
        }


        public void HideTerrain()
        {
            _hidingNow = true;

            _awaitableService.ForgetAwaitable(HideTerrainAsync());
        }

        private async Awaitable HideTerrainAsync()
        {
            float startPos = SpriteObject.transform.localPosition.y;
            for (int f = 0; f <= _animateFrames; f++)
            {
                float ypos = startPos + 1.0f * f / _animateFrames * f / _animateFrames * StartY;
                SpriteObject.transform.localPosition = new Vector3(0, ypos, 0);
                await Awaitable.NextFrameAsync();
            }

            _terrain.ReturnPatch(this);
        }
    }
}
