using OxDb.Client.Assets.Scripts.Assets.Materials;
using OxDb.Client.Dungeons;
using OxDb.SharedGame.Crawler.Maps.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.GameObjects
{

    public class DungeonAssetPosition
    {
        public const int None = 0;
        public const int NorthWall = 1;
        public const int EastWall = 2;
        public const int Ceiling = 4;
        public const int Floor = 5;
        public const int Pillar = 6;
        public const int NorthUpper = 7;
        public const int EastUpper = 8;
        public const int Max = 9;
    }





    public class ClientMapCell : BaseBehaviour
    {
        public bool DidInit { get; set; }
        public bool DidJustDraw { get; set; }
        public int WorldX { get; set; }
        public int WorldZ { get; set; }
        public int MapX { get; set; }
        public int MapZ { get; set; }
        public List<MapCellDetail> Details { get; set; } = new List<MapCellDetail>();
        public GameObject Content;
        public ObjectFader Fader;
        public bool KeepActive { get; set; }



        public DungeonAsset[] AssetPositions { get; set; } = new DungeonAsset[DungeonAssetPosition.Max];


        public override void Init()
        {
            base.Init();
        }

        protected override void OnDestroy()
        {
            ClearFullCell();

            base.OnDestroy();
        }


        public void ClearFullCell()
        {
            ClearProps();
            _clientEntityService?.DestroyAllChildren(Content);

            for (int a = 0; a < AssetPositions.Length; a++)
            {
                AssetPositions[a] = null;
            }
        }

        public void ClearProps()
        {
            Fader.ClearObjects();
        }

        public void SetPropAlphas(float alpha)
        {
            Fader.SetObjectAlphas(alpha);
        }

        public void AddProp(GameObject go)
        {
            Fader.AddObject(go);
        }
    }
}


