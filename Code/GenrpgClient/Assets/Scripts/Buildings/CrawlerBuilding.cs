using Assets.Scripts.Crawler.Maps.GameObjects;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Buildings.Constants;
using OxDb.SharedGame.Buildings.Settings;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Maps.Services;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Buildings
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


        public async Awaitable SetData(BuildingType btype, long seed, CrawlerMapRoot mapRoot, ClientMapCell mapCell, BuildingMats mats)
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

            Color redRemap = Color.red;
            Color greenRemap = Color.green;
            Color blueRemap = Color.blue;

            if (_buildingColors.ContainsKey(btype.IdKey))
            {
                redRemap = _buildingColors[btype.IdKey];
            }
            else
            {
                if (rand.NextDouble() < 0.5f)
                {

                    redRemap = Color.white * RandUtils.FloatRange(0, 1, rand);
                }
                else
                {
                    redRemap = Color.brown * RandUtils.FloatRange(0.5f, 1.5f, rand);
                }
            }
            SetMaterialToSlot(btype, Walls, mats.GetMatsFromSlot(EBuildingMatSlots.Walls), rand, redRemap, greenRemap, blueRemap);
            SetMaterialToSlot(btype, RoofPeaks, mats.GetMatsFromSlot(EBuildingMatSlots.Walls), rand, redRemap, greenRemap, blueRemap);
            SetMaterialToSlot(btype, Doors, mats.GetMatsFromSlot(EBuildingMatSlots.Doors), rand, redRemap, greenRemap, blueRemap);
            SetMaterialToSlot(btype, Windows, mats.GetMatsFromSlot(EBuildingMatSlots.Windows), rand, redRemap, greenRemap, blueRemap);
            SetMaterialToSlot(btype, Shingles, mats.GetMatsFromSlot(EBuildingMatSlots.Shingles), rand, redRemap, greenRemap, blueRemap);

            StoreSign sign = _clientEntityService.GetComponent<StoreSign>(gameObject);
            if (sign != null)
            {
                sign.BGImage.SetColor(redRemap);
            }
        }

        public void SetMaterialToSlot(BuildingType btype, List<MeshRenderer> meshes, List<WeightedBuildingMaterial> mats, IRandom rand,
            Color redColor, Color greenColor, Color blueColor)
        {
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
                }
            }

            if (chosenMat == null)
            {
                chosenMat = mats[0];
            }

            MaterialPropertyBlock mainBlock = new MaterialPropertyBlock();
            mainBlock.SetColor("_RedRemap", redColor);
            mainBlock.SetColor("_GreenRemap", greenColor);
            mainBlock.SetColor("_BlueRemap", blueColor);
            mainBlock.SetColor("_MainColor", Color.white);

            Color mainColor = Color.white;
            if (chosenMat.ColorTargets.Count > 0)
            {
                Color colorTarget = chosenMat.ColorTargets[rand.Next() % chosenMat.ColorTargets.Count];

                float targetPercent = (float)rand.NextDouble();
                Color newColor = new Color((float)(colorTarget.r + (1 - colorTarget.r) * targetPercent),
                    (float)(colorTarget.g + (1 - colorTarget.g) * targetPercent),
                    (float)(colorTarget.b + (1 - colorTarget.b) * targetPercent), 1);

                mainColor = newColor;
                mainBlock.SetColor("_MainColor", mainColor);
            }

            //Material newMat = new Material(chosenMat.Mat);
            //newMat.SetColor("_RedRemap", redColor);
            //newMat.SetColor("_GreenRemap", greenColor);
            //newMat.SetColor("_BlueRemap", blueColor);
            //newMat.SetColor("_MainColor", Color.white);
            foreach (MeshRenderer renderer in meshes)
            {
                renderer.sharedMaterial = chosenMat.Mat;
                renderer.SetPropertyBlock(mainBlock);
            }
        }
    }
}


