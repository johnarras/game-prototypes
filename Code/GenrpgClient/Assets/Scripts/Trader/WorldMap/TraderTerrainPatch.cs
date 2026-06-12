using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Assets.Textures;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Dungeons;
using OxDb.SharedGame.ProcGen.Settings.Textures;
using OxDb.SharedGame.Trader.Maps.Settings;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Trader.WorldMap
{
    public class TraderTerrainPatch : BaseBehaviour
    {
        private IAwaitableService _awaitableService = null;
        private IClientAppService _appService = null;


        public DungeonAsset GroundAsset;
        public GameObject GroundAssetObject;

        private int _x = -1;
        private int _y = -1;

        public int X => _x;
        public int Y => _y;

        public float StartY = -3;
        public float AnimateSeconds = 10.0f;
        private int _animateFrames = 60;

        private bool _hidingNow = false;

        private TraderTerrain _terrain = null;

        public TextureList _currentTextures = null;

        public double GetDistanceToPoint(int cx, int cy)
        {
            float dx = cx - _x;
            float dy = cy - _y;

            return Mathf.Sqrt(dx * dx + dy * dy);
        }
        public void ShowTerrain(TraderTerrain terrain, TerrainPatchArgs args)
        {
            _terrain = terrain;
            _x = args.X;
            _y = args.Y;
            long biomeIndex = args.BiomeIndex;

            IndexedColor color = _gameData.Get<IndexedColorSettings>(_gs.ch).Get(biomeIndex);

            if (color != null)
            {
                TextureType ttype = _gameData.Get<TextureTypeSettings>(_gs.ch).Get(color.TextureTypeId);

                if (ttype != null)
                {
                    if (_currentTextures == null || _currentTextures.name != ttype.Art)
                    {
                        SetMainTexture(null);

                        if (_currentTextures != null)
                        {
                            _clientEntityService.Destroy(_currentTextures.gameObject);
                        }
                        _currentTextures = null;
                        _assetService.LoadAsset(AssetCategoryNames.TerrainTex, ttype.Art, OnDownloadTerrainTexture, gameObject, GetToken(), ttype);
                    }
                    else
                    {
                        OnDownloadTerrainTexture(_currentTextures.gameObject, ttype, GetToken());
                    }
                }
            }
        }

        private MaterialPropertyBlock _block = null;

        private void SetMainTexture(Texture2D tex)
        {
            if (GroundAsset != null)
            {
                if (_block == null)
                {
                    _block = new MaterialPropertyBlock();
                }
                foreach (Renderer renderer in GroundAsset.StoneRenderers)
                {
                    renderer.material.mainTexture = tex;

                    //renderer.GetPropertyBlock(_block);
                    //_block.SetTexture(MaterialUtils.MainTexturePropertyName, tex);
                    //renderer.SetPropertyBlock(_block);
                }
            }
        }

        private void OnDownloadTerrainTexture(GameObject go, TextureType ttype, CancellationToken token)
        {
            if (go == null)
            {
                return;
            }

            TextureList tlist = go.GetComponent<TextureList>();

            if (tlist == null || tlist.Textures.Count < 1 || tlist.Textures[0] == null)
            {
                _clientEntityService.Destroy(go);
                _clientEntityService.Destroy(_currentTextures);
                _currentTextures = null;
                return;
            }
            gameObject.name = "LoadedTerrain" + ttype.Name;

            SetMainTexture(tlist.Textures[0]);

            _animateFrames = Mathf.Max(1, (int)(AnimateSeconds * _appService.TargetFrameRate));
            _awaitableService.ForgetAwaitable(AnimateIn());

            transform.localPosition = new Vector3(_x, 0.2f, _y);

            _currentTextures = tlist;
        }

        private async Awaitable AnimateIn()
        {
            float s = 1.70f;
            _hidingNow = false;

            GroundAssetObject.transform.localPosition = new Vector3(0, StartY, 0);
            for (int f = 0; f < _animateFrames; f++)
            {
                if (_hidingNow)
                {
                    return;
                }
                float timePct = 1.0f * f / _animateFrames;

                float tp = timePct - 1;

                float dist = ((s + 1) * tp * tp * tp - s * tp * tp) * StartY;

                //dist *= dist;

                GroundAssetObject.transform.localPosition = new Vector3(0, -dist, 0);
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
            float startPos = GroundAssetObject.transform.localPosition.y;
            for (int f = 0; f <= _animateFrames; f++)
            {
                float ypos = startPos + 1.0f * f / _animateFrames * f / _animateFrames * StartY;
                GroundAssetObject.transform.localPosition = new Vector3(0, ypos, 0);
                await Awaitable.NextFrameAsync();
            }

            _terrain.ReturnPatch(this);
        }
    }
}
