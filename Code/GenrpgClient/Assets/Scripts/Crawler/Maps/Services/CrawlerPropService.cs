using OxDb.Client.Crawler.Maps.Constants;
using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.Crawler.Maps.Services;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.Client.GameObjects;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.ProcGen.Settings.Props;
using OxDb.SharedGame.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Assets.Scripts.Crawler.Maps.Services
{

    public class DrawCellPropsArgs
    {
        public PartyData Party { get; set; }
        public CrawlerWorld World { get; set; }
        public CrawlerMapRoot MapRoot { get; set; }
        public ClientMapCell Cell { get; set; }
        public bool HasLargeProp { get; set; }
        public ZoneType ZoneType { get; set; }
        public int Dir { get; set; }
        public bool BlockedOutdoorDungeonCell { get; set; }
        public bool IsOutdoorDungeon { get; set; }
    }


    public interface ICrawlerPropService : IInjectable
    {
        ValueTask DrawPropsAtCell(DrawCellPropsArgs args, CancellationToken token);
        Awaitable DrawEdgeProps(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, CancellationToken token);
    }
    public class CrawlerPropService : ICrawlerPropService
    {
        protected IClientEntityService _clientEntityService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected ILogService _logService = null;
        protected IAssetService _assetService = null;
        protected ICrawlerMapService _mapService = null;
        protected IEntityService _entityService = null;
        public async ValueTask DrawPropsAtCell(DrawCellPropsArgs args, CancellationToken token)
        {
            PropTypeSettings propSettings = _gameData.Get<PropTypeSettings>(_gs.ch);

            IRandom rand = new MyRandom(args.World.Seed / 11 + args.MapRoot.Map.ArtSeed / 3 + args.Cell.MapX * 113 + args.Cell.MapZ * 23);

            List<WeightedEntity> treeItems = args.ZoneType.GetPropsOfType(EntityTypes.Tree);

            if (args.HasLargeProp)
            {
                List<WeightedEntity> propItems = args.ZoneType.GetPropsOfType(EntityTypes.Prop);

                List<PropType> mustUsePropTypes = new List<PropType>();

                foreach (WeightedEntity we in propItems)
                {
                    PropType propType = propSettings.Get(we.EntityId);

                    if (propType != null && propType.MustUse)
                    {
                        mustUsePropTypes.Add(propType);
                    }
                }

                if (mustUsePropTypes.Count > 0)
                {
                    PropType finalPropType = mustUsePropTypes[rand.Next() % mustUsePropTypes.Count];

                    int index = Math.Max(1, RandUtils.IntRange(1, finalPropType.VariationCount, rand));

                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        Angle = args.Dir * CrawlerMapConstants.DirToAngleMult,
                        Cell = args.Cell,
                        MapRoot = args.MapRoot,
                        Seed = _mapService.GetMapCellHash(args.MapRoot.Map.IdKey, args.Cell.MapX, args.Cell.MapZ, rand.Next()),
                        XOffset = 0,
                        ZOffset = 0,
                    };

                    _mapService.LoadProp(loadData, finalPropType.Art + index, token);

                    return;
                }
                else if (treeItems.Count > 0)
                {
                    int largePropCount = 1;

                    float maxScale = 1.25f;
                    if ((args.Cell.MapX <= 0 || args.Cell.MapZ <= 0 ||
                        args.Cell.MapX >= args.MapRoot.Map.Width - 1 || args.Cell.MapZ >= args.MapRoot.Map.Height - 1))
                    {
                        largePropCount = RandUtils.IntRange(2, 4, rand);
                        maxScale = 2.0f;
                    }

                    for (int times = 0; times < largePropCount; times++)
                    {
                        WeightedEntity finalEntity = RandUtils.GetRandomElement(treeItems, rand);

                        IVariationIndexedGameItem finalItem = _entityService.Find<IVariationIndexedGameItem>(_gs.ch, finalEntity.EntityTypeId, finalEntity.EntityId);

                        if (finalItem != null)
                        {
                            int propIndex = Math.Max(1, RandUtils.IntRange(1, finalItem.VariationCount, rand));

                            float offsetDelta = args.MapRoot.XZBlockSize * 0.3f;
                            CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                            {
                                Angle = args.Dir * CrawlerMapConstants.DirToAngleMult,
                                Cell = args.Cell,
                                MapRoot = args.MapRoot,
                                Seed = _mapService.GetMapCellHash(args.MapRoot.Map.IdKey, args.Cell.MapX, args.Cell.MapZ, rand.Next()),
                                AssetCategoryNameOverride = _assetService.GetAssetCategoryFromEntityTypeId(finalEntity.EntityTypeId),
                                XOffset = RandUtils.DeltaRange(offsetDelta, rand),
                                ZOffset = RandUtils.DeltaRange(offsetDelta, rand),
                                Scale = RandUtils.FloatRange(1.0f, maxScale, rand)
                            };

                            _mapService.LoadProp(loadData, finalItem.Art + propIndex, token);
                        }
                    }
                }
            }

            if (args.ZoneType.SmallPropChance > 0 && args.ZoneType.MaxSmallPropQuantity > 0)
            {
                List<WeightedEntity> remainingItems = args.ZoneType.Props.Except(treeItems).ToList();

                if (remainingItems.Count < 1)
                {
                    return;
                }

                double smallPropChance = args.ZoneType.SmallPropChance;
                int maxSmallPropQuantity = args.ZoneType.MaxSmallPropQuantity;



                int minGuaranteedSmallObjects = 0;
                if (args.BlockedOutdoorDungeonCell)
                {
                    minGuaranteedSmallObjects = 3;
                    smallPropChance *= 1.3f;
                    maxSmallPropQuantity *= 2;

                    if (maxSmallPropQuantity < minGuaranteedSmallObjects)
                    {
                        maxSmallPropQuantity = minGuaranteedSmallObjects;
                    }
                }
                else if (args.IsOutdoorDungeon)
                {
                    smallPropChance /= 2;
                }

                for (int times = 0; times < maxSmallPropQuantity; times++)
                {
                    if (times >= minGuaranteedSmallObjects && rand.NextDouble() > smallPropChance)
                    {
                        continue;
                    }

                    WeightedEntity we = RandUtils.GetRandomElement(remainingItems, rand);

                    IVariationIndexedGameItem finalItem = _entityService.Find<IVariationIndexedGameItem>(_gs.ch, we.EntityTypeId, we.EntityId);

                    if (finalItem == null)
                    {
                        continue;
                    }

                    int smallIndex = Math.Max(1, RandUtils.IntRange(1, finalItem.VariationCount, rand));

                    float offsetDelta = args.MapRoot.XZBlockSize * 0.4f;
                    float maxScale = 1.25f;
                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        Angle = rand.Next() % 360,
                        Cell = args.Cell,
                        MapRoot = args.MapRoot,
                        Seed = _mapService.GetMapCellHash(args.MapRoot.Map.IdKey, args.Cell.MapX, args.Cell.MapZ, rand.Next()),
                        AssetCategoryNameOverride = _assetService.GetAssetCategoryFromEntityTypeId(we.EntityTypeId),
                        XOffset = RandUtils.DeltaRange(offsetDelta, rand),
                        ZOffset = RandUtils.DeltaRange(offsetDelta, rand),
                        Scale = RandUtils.FloatRange(1.0f, maxScale, rand)
                    };

                    _mapService.LoadProp(loadData, finalItem.Art + smallIndex, token);
                }
            }
        }




        public async Awaitable DrawEdgeProps(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, CancellationToken token)
        {
            mapRoot.DidDrawEdgeProps = true;

            int edgePropRadius = CrawlerDrawMapService.ViewRadius + 1;
            foreach (ZoneEdge edge in mapRoot.Map.EdgePoints)
            {
                List<Point2I> edgePoints = new List<Point2I>();
                int cx = edge.X + edge.DX;
                int cz = edge.Z + edge.DZ;

                int oppDX = edge.DX == 0 ? 1 : 0;
                int oppDZ = edge.DZ == 0 ? 1 : 0;


                int iterationTimes = 0;
                while (cx > -edgePropRadius && cx < mapRoot.Map.Width+edgePropRadius-1 &&
                    cz > - edgePropRadius && cz < mapRoot.Map.Height+edgePropRadius-1)
                {
                    int maxWidth = Math.Max(1, 3 - iterationTimes);
                    iterationTimes++;

                    for (int w = -maxWidth; w <= maxWidth; w++)
                    {
                        if (w == 0)
                        {
                            continue;
                        }
                        edgePoints.Add(new Point2I()
                        {
                            X = cx + oppDX*w,
                            Z = cz + oppDZ*w,
                        });
                    }
                    cx += edge.DX;
                    cz += edge.DZ;
                }

                foreach (Point2I pt in edgePoints)
                {
                    ClientMapCell mapCell = mapRoot.GetCellAtWorldPos(pt.X,pt.Z, true, false);

                    mapCell.KeepActive = true;
                    mapCell.Content.transform.position = new Vector3(pt.X * mapRoot.XZBlockSize, 0, pt.Z * mapRoot.XZBlockSize);
                    DrawCellPropsArgs args = new DrawCellPropsArgs()
                    {
                        BlockedOutdoorDungeonCell = true,
                        Party = party,
                        Cell = mapCell,
                        HasLargeProp = true,
                        IsOutdoorDungeon = true,
                        World = world,
                        MapRoot = mapRoot,
                        ZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(edge.ZoneTypeId),
                        Dir = 0,
                    };
                    await DrawPropsAtCell(args, token);
                    await Awaitable.NextFrameAsync(token);
                }
            }
        }
    }
}
