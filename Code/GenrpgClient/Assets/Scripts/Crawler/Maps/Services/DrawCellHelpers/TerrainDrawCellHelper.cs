using Assets.Scripts.Assets.Textures;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.ProcGen.Settings.Textures;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{
    public class TerrainDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override int Order => 200;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token)
        {
            if (mapRoot.Map.HasFlag(CrawlerMapFlags.IsIndoors) ||
                mapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon)
            {
                return;
            }

            byte biomeTypeId = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Terrain);

            if (biomeTypeId > 0)
            {
                ZoneType biomeType = _gameData.Get<ZoneTypeSettings>(null).Get(biomeTypeId);

                if (biomeType != null)
                {
                    LoadTerrainTexture(cell.Content, biomeType.Textures.Where(x => x.TextureChannelId == MapConstants.BaseTerrainIndex).First().TextureTypeId, token);
                }
            }
            try
            {
            }
            catch (Exception e)
            {
                _logService.Info("Draw Cell Error: " + e.Message);
            }


            await Task.CompletedTask;
        }

        protected virtual void LoadTerrainTexture(object parent, long terrainTextureId, CancellationToken token)
        {
            TextureType ttype = _gameData.Get<TextureTypeSettings>(null).Get(terrainTextureId);

            if (ttype != null && !string.IsNullOrEmpty(ttype.Art))
            {
                _assetService.LoadAssetInto(parent, AssetCategoryNames.Dungeons, "TerrainFloor", OnLoadTerrainFloor, ttype, token);
            }
        }

        private void OnLoadTerrainFloor(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            TextureType ttype = data as TextureType;

            SpriteRenderer renderer = _clientEntityService.GetComponent<SpriteRenderer>(go);
            if (renderer == null || ttype == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            _assetService.LoadAsset(AssetCategoryNames.TerrainTex, ttype.Art, OnDownloadTerrainTexture, renderer, renderer.gameObject, token);

        }

        private void OnDownloadTerrainTexture(object obj, object data, CancellationToken token)
        {

            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            SpriteRenderer renderer = data as SpriteRenderer;

            if (renderer == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            TextureList tlist = _clientEntityService.GetComponent<TextureList>(go);

            if (tlist == null || tlist.Textures == null || tlist.Textures.Count < 1 || tlist.Textures[0] == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            _clientEntityService.AddToParent(go, renderer.gameObject);

            renderer.gameObject.transform.localPosition = new Vector3(-CrawlerMapConstants.XZBlockSize / 2, 0, -CrawlerMapConstants.XZBlockSize / 2);
            Sprite spr = Sprite.Create(tlist.Textures[0], new Rect(0, 0, tlist.Textures[0].width, tlist.Textures[0].height), Vector2.zero,
                tlist.Textures[0].width / CrawlerMapConstants.XZBlockSize);

            renderer.sprite = spr;
        }


    }
}
