using Assets.Scripts.Crawler.Maps.GameObjects;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Utils;
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


        public async Awaitable InitData(BuildingType btype, long seed, CrawlerMapRoot mapRoot, ClientMapCell mapCell, BuildingMats mats)
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

            SetMaterialToSlot(btype, Walls, mats.GetMatsFromSlot(EBuildingMatSlots.Walls), rand);
            SetMaterialToSlot(btype, RoofPeaks, mats.GetMatsFromSlot(EBuildingMatSlots.Walls), rand);
            SetMaterialToSlot(btype, Doors, mats.GetMatsFromSlot(EBuildingMatSlots.Doors), rand);
            SetMaterialToSlot(btype, Windows, mats.GetMatsFromSlot(EBuildingMatSlots.Windows), rand);
            SetMaterialToSlot(btype, Shingles, mats.GetMatsFromSlot(EBuildingMatSlots.Shingles), rand);

            if (_buildingColors.ContainsKey(btype.IdKey))
            {
                StoreSign sign = _clientEntityService.GetComponent<StoreSign>(gameObject);
                if (sign != null)
                {
                    _uiService.SetImageColor(sign.BGImage, _buildingColors[btype.IdKey]);
                }
            }
        }

        public void SetMaterialToSlot(BuildingType btype, List<MeshRenderer> meshes, List<WeightedBuildingMaterial> mats, IRandom rand)
        {
            if (mats.Count < 1)
            {
                return;
            }

            double weightSum = mats.Sum(x => x.Weight);
            double weightChosen = rand.NextDouble() * weightSum;

            foreach (WeightedBuildingMaterial mat in mats)
            {
                weightChosen -= mat.Weight;

                if (weightChosen <= 0)
                {
                    foreach (MeshRenderer renderer in meshes)
                    {
                        renderer.material = mat.Mat;
                    }

                    if (false && _buildingColors.ContainsKey(btype.IdKey))
                    {
                        foreach (MeshRenderer renderer in meshes)
                        {
                            renderer.material.color = _buildingColors[btype.IdKey];
                        }
                    }
                    else if (mat.ColorTargets.Count > 0)
                    {
                        Color colorTarget = mat.ColorTargets[rand.Next() % mat.ColorTargets.Count];

                        float targetPercent = (float)rand.NextDouble();
                        Color newColor = new Color((float)(colorTarget.r + (1 - colorTarget.r) * targetPercent),
                            (float)(colorTarget.g + (1 - colorTarget.g) * targetPercent),
                            (float)(colorTarget.b + (1 - colorTarget.b) * targetPercent), 1);

                        foreach (MeshRenderer renderer in meshes)
                        {
                            renderer.material.color = newColor;
                        }
                    }
                    break;
                }
            }
        }

    }
}
