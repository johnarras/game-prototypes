using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Buildings.Constants;
using OxDb.SharedGame.Buildings.Settings;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Buildings
{


    public class CrawlerBuilding : MapBuilding
    {

        private ICrawlerWorldService _worldService = null;
        private ICrawlerService _crawlerService = null;

        public List<MeshRenderer> Walls = new List<MeshRenderer>();
        public List<MeshRenderer> Doors = new List<MeshRenderer>();
        public List<MeshRenderer> Windows = new List<MeshRenderer>();
        public List<MeshRenderer> Shingles = new List<MeshRenderer>();
        public List<MeshRenderer> RoofPeaks = new List<MeshRenderer>();

        private Dictionary<long, Color> _buildingColors = new Dictionary<long, Color>()
        {
            {BuildingTypes.Guild, new Color(1,0.5f,0) },
            {BuildingTypes.Equipment, new Color(1,0,0) },
            {BuildingTypes.Regen, new Color(0,0,1) },
            {BuildingTypes.Temple, new Color(1,1,1) },
            {BuildingTypes.Trainer, new Color(0,1,0) },
            {BuildingTypes.Tavern, new Color(1,0,1) },
            {BuildingTypes.Npc, new Color(1,1,0) },
        };


        public async ValueTask SetData(BuildingType btype, long seed, CrawlerMapRoot mapRoot, ClientMapCell mapCell, BuildingMats mats)
        {
            string overrideName = null;

            MapCellDetail detail = mapRoot.Map.Details.FirstOrDefault(x => x.X == mapCell.MapX && x.Z == mapCell.MapZ);

            if (detail != null)
            {
                if (detail.EntityTypeId == EntityTypes.Map)
                {
                    CrawlerMap otherMap = _worldService.GetMap(detail.EntityId);

                    if (otherMap != null)
                    {
                        overrideName = otherMap.Name;
                    }
                }
                else if (detail.EntityTypeId == EntityTypes.Npc)
                {
                    CrawlerWorld world = await _worldService.GetWorld(_crawlerService.GetParty().WorldId);

                    CrawlerNpc npc = world.Npcs.FirstOrDefault(x => x.IdKey == detail.EntityId);
                    if (npc != null)
                    {
                        overrideName = "A Mysterious Hut";
                    }
                }
            }

            base.Init(btype, new OnSpawn(), overrideName);
            MyRandom rand = new MyRandom(seed);

            SetMaterialToSlot(btype, Walls, mats, EBuildingMatSlots.Walls, rand);
            SetMaterialToSlot(btype, RoofPeaks, mats, EBuildingMatSlots.Walls, rand);
            SetMaterialToSlot(btype, Doors, mats, EBuildingMatSlots.Doors, rand);
            SetMaterialToSlot(btype, Windows, mats, EBuildingMatSlots.Windows, rand);
            SetMaterialToSlot(btype, Shingles, mats, EBuildingMatSlots.Shingles, rand);

        }

        public void SetMaterialToSlot(BuildingType btype, List<MeshRenderer> meshes, BuildingMats buildingMats, EBuildingMatSlots slot, IRandom rand)

        {

            List<WeightedBuildingMaterial> mats = buildingMats.GetMatsFromSlot(slot);
            if (mats.Count < 1)
            {
                return;
            }

            double weightSum = mats.Sum(x => x.Weight);
            double weightChosen = rand.NextDouble() * weightSum;

            WeightedBuildingMaterial chosenMat = null;
            foreach (WeightedBuildingMaterial mat in mats)
            {
                weightChosen -= mat.Weight;

                if (weightChosen <= 0)
                {
                    chosenMat = mat;
                    break;
                }
            }



            if (chosenMat == null)
            {
                chosenMat = mats[0];
            }
            foreach (MeshRenderer renderer in meshes)
            {
                renderer.sharedMaterial = chosenMat.Mat;
            }
        }
    }
}


