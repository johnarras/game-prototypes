
using OxDb.Client.ProcGen.Loading.Utils;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.ProcGen.Settings.Bridges;
using OxDb.SharedGame.ProcGen.Settings.Locations;
using OxDb.SharedGame.ProcGen.Settings.MapWater;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AddBridges : BaseZoneGenerator
{

    private IAddPoolService _addPoolService = null;
    private List<WaterGenData> _waterGenData = new List<WaterGenData>();

    public const string DefaultBridgeArtName = "Bridge";
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        if (_md.BridgeDistances == null)
        {
            _md.BridgeDistances = new ushort[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
        }

        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int z = 0; z < _mapProvider.GetMap().GetHhgt(); z++)
            {
                _md.BridgeDistances[x, z] = 10000;
            }
        }
        if (_md.CurrBridges == null)
        {
            _md.CurrBridges = new List<Point2I>();
        }

        if (_md.Roads == null)
        {
            return;
        }

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed % 12389292 + 333);

        if (_md.CreviceBridges != null)
        {
            foreach (Point2I bpos in _md.CreviceBridges)
            {
                foreach (List<LineCell> road in _md.Roads)
                {
                    foreach (LineCell pt in road)
                    {
                        if (Math.Abs(bpos.X - pt.X) <= 2 && Math.Abs(bpos.Z - pt.Z) <= 2 &&
                            pt.IsCenter)
                        {
                            AddOneBridge(road, rand, bpos);
                        }
                    }
                }
            }
        }



        foreach (List<LineCell> road in _md.Roads)
        {
            int numBridgesToTry = 0;
            if (rand.Next() % 14 == 0)
            {
                numBridgesToTry++;
            }

            if (rand.Next() % 50 == 0)
            {
                numBridgesToTry++;
            }

            if (road.Count > 0)
            {
                int dx = (int)Math.Abs(road[0].X - road[road.Count - 1].X);
                int dy = (int)Math.Abs(road[0].Z - road[road.Count - 1].Z);
                int maxDist = Math.Max(dx, dy);
                numBridgesToTry += maxDist / (60 + rand.Next() % 60);
            }

            for (int tries = 0; tries < numBridgesToTry; tries++)
            {
                AddOneBridge(road, rand);
            }
        }

        foreach (WaterGenData wgd in _waterGenData)
        {
            _addPoolService.TryAddPool(wgd);
        }
    }

    protected void AddOneBridge(
                                 List<LineCell> road,
                                 MyRandom rand,
                                 Point2I centerPointIn = null)
    {
        if (road == null)
        {
            return;
        }

        List<LineCell> road2 = new List<LineCell>();

        foreach (LineCell pt in road)
        {
            if (pt.IsCenter)
            {
                road2.Add(pt);
            }
        }

        road = road2;

        int xpos = MapConstants.TerrainPatchSize;
        int zpos = MapConstants.TerrainPatchSize;

        if (road.Count > 0)
        {
            LineCell midpoint = road[road.Count / 2];
            xpos = (int)midpoint.X;
            zpos = (int)midpoint.Z;
        }


        int zoneId = _md.MapZoneIds[xpos, zpos]; // zoneobject

        Zone zone = _mapProvider.GetMap().Get<Zone>(zoneId);

        long zoneTypeId = (zone != null ? zone.ZoneTypeId : 1);

        BridgeType bt = GetRandomBridgeType(zoneTypeId, rand);

        float bridgeLength = 6;
        string bridgeArt = DefaultBridgeArtName;

        if (bt != null && !string.IsNullOrEmpty(bt.Art))
        {
            bridgeArt = bt.Art;
            bridgeLength = bt.Length;
        }

        float radius = 12;

        // Get length of bridge
        int halfBridgeLength = (int)(bridgeLength / 2);
        int bridgeDistanceFromEnd = (int)(radius + halfBridgeLength * 2);
        int bridgeDistanceFromStart = (int)(radius + halfBridgeLength * 2);
        if (road.Count <= bridgeDistanceFromEnd + bridgeDistanceFromStart)
        {
            return;
        }

        int cval = 0;

        Point2I centerpt = null;

        // Don't let the bridge go too close to a location.

        for (int times = 0; times < 80; times++)
        {
            if (times > 0 && centerPointIn != null)
            {
                break;
            }

            // Not to close to road end
            centerpt = null;
            cval = RandUtils.IntRange(bridgeDistanceFromStart, road.Count - bridgeDistanceFromEnd, rand);

            if (cval < halfBridgeLength || cval >= road.Count - halfBridgeLength)
            {
                continue;
            }

            centerpt = new Point2I(road[cval].X, road[cval].Z);

            if (centerPointIn != null)
            {
                centerpt = centerPointIn;
            }

            if (centerpt == null)
            {
                continue;
            }

            int avgRadius = 3; // Average points down the road, with 2*this+1 points being used.

            int numCellsChecked = 2 * avgRadius + 1;
            int startIndex = MathUtil.Clamp(avgRadius, cval - halfBridgeLength, road.Count - 1 - avgRadius);
            int endIndex = MathUtil.Clamp(avgRadius, cval + halfBridgeLength, road.Count - 1 - avgRadius);

            float fsx = 0;
            float fsz = 0;
            float fex = 0;
            float fez = 0;

            for (int i = -avgRadius; i <= avgRadius; i++)
            {
                int sindex = startIndex + i;
                int eindex = endIndex + i;

                fsx += road[sindex].X / numCellsChecked;
                fsz += road[sindex].Z / numCellsChecked;
                fex += road[eindex].X / numCellsChecked;
                fez += road[eindex].Z / numCellsChecked;
            }


            int ex = MathUtil.Clamp(0, (int)(fex), _mapProvider.GetMap().GetHwid());
            int ez = MathUtil.Clamp(0, (int)(fez), _mapProvider.GetMap().GetHhgt());
            int sx = MathUtil.Clamp(0, (int)(fsx), _mapProvider.GetMap().GetHwid());
            int sz = MathUtil.Clamp(0, (int)(fsz), _mapProvider.GetMap().GetHhgt());


            int shrinkMod = rand.Next() % 2;
            // Move ex and sx closer together if they're too far apart.
            int shrinkLengthTimes = 0;
            do
            {
                float lx = ex - sx;
                float lz = ez - sz;
                float len = MathUtil.Sqrt(lx * lx + lz * lz);
                if (len <= bridgeLength + 1)
                {
                    break;
                }

                if (ez < sz)
                {
                    ez++;
                }
                else if (sz < ez)
                {
                    sz++;
                }

                if (ex < sx)
                {
                    ex++;
                }
                else if (sx < ex)
                {
                    sx++;
                }
            }
            while (++shrinkLengthTimes < 20);




            int cx = (int)(ex + sx) / 2;
            int cz = (int)(ez + sz) / 2;

            Location loc = _zoneGenService.FindMapLocation(cx, cz, 15);

            if (loc != null)
            {
                continue;
            }

            // These are the actual location points for the bridge. They are
            // calced using s and e average, rather than cx,cz.
            float px = (ex + sx) / 2.0f;
            float pz = (ez + sz) / 2.0f;

            // Not too close to the edge of the map
            float edgeSize = MapConstants.TerrainPatchSize;

            if (px < edgeSize || px > _mapProvider.GetMap().GetHwid() - edgeSize ||
                pz < edgeSize || pz > _mapProvider.GetMap().GetHhgt() - edgeSize)
            {
                continue;
            }


            // Now make sure we're not near a bridge.

            int minBridgeSeparation = 110;



            bool nearBridge = false;
            foreach (Point2I pt3 in _md.CurrBridges)
            {

                if (Math.Abs(px - pt3.X) <= minBridgeSeparation &&
                    Math.Abs(pz - pt3.Z) <= minBridgeSeparation)
                {
                    nearBridge = true;
                    break;
                }
            }
            if (nearBridge)
            {
                centerpt = null;
                continue;

            }



            if (cx - halfBridgeLength < 0 ||
                cz - halfBridgeLength < 0 ||
                cx + halfBridgeLength >= _mapProvider.GetMap().GetHwid() ||
                cz + halfBridgeLength >= _mapProvider.GetMap().GetHhgt())
            {
                centerpt = null;
                continue;
            }


            float sy = _md.Heights[sx, sz] * MapConstants.MapHeight;
            float my = _md.Heights[(sx + ex) / 2, (sz + ez) / 2] * MapConstants.MapHeight;
            float ey = _md.Heights[ex, ez] * MapConstants.MapHeight;
            float cy = (sy + ey + my) / 3;

            float minHeight = Math.Min(sy, Math.Min(my, ey));
            float maxHeight = Math.Max(sy, Math.Max(my, ey));

            float startHeightDiff = maxHeight - minHeight;

            cy = minHeight + RandUtils.FloatRange(0.0f, 0.25f, rand) * startHeightDiff;

            float cyscale = cy / MapConstants.MapHeight;

            float bridgeHeight = cy - 0.4f;

            if (bridgeHeight <= MapConstants.MinLandHeight)
            {
                continue;
            }

            float bridgeHeightPercent = bridgeHeight / MapConstants.MapHeight;
            float heightAtEnds = bridgeHeightPercent + 0.15f / MapConstants.MapHeight;

            float maxHeightchange = 3.0f;
            if (Math.Abs(sy - ey) > maxHeightchange)
            {
                continue;
            }
            if (centerpt == null)
            {
                continue;
            }

            string bridgeName = "Bridge" + (int)(px) + "x" + (int)(pz);

            float dx = ex - sx;
            float dz = ez - sz;

            float angle = (float)Math.Atan2(dz, dx);

            angle = (float)(angle * 180.0f / Math.PI);


            if (rand.Next() % 2 == 0)
            {
                angle += 180;
            }

            float oldpx = px;
            float oldpz = pz;

            int ipx = (int)(px);
            int ipz = (int)(pz);

            if (ipz % MapConstants.TerrainPatchSize > MapConstants.TerrainPatchSize - 3)
            {
                continue;
            }

            float lengthMult = Math.Max(1.0f, halfBridgeLength / 5.0f);

            // Now dig out the middle.

            float bdist = MathUtil.Sqrt((ex - sx) * (ex - sx) + (ez - sz) * (ez - sz));

            int fullcl = RandUtils.IntRange(8 * halfBridgeLength, 22 * halfBridgeLength, rand);

            List<int> xvals = new List<int>();

            xvals.Add((int)(Math.Max(0, cx - fullcl)));
            xvals.Add((int)(Math.Min(_mapProvider.GetMap().GetHwid() - 1, cx + fullcl)));

            List<int> zvals = new List<int>();
            zvals.Add((int)(Math.Max(0, cy - fullcl)));
            zvals.Add((int)(Math.Min(_mapProvider.GetMap().GetHhgt() - 1, cy + fullcl)));

            bool nearWater = false;

            for (int x = xvals[0]; x <= xvals[1]; x++)
            {
                if (nearWater)
                {
                    break;
                }

                for (int z = 0; z < zvals.Count; z++)
                {
                    if (FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.NearWater))
                    {
                        nearWater = true;
                        break;
                    }
                }
            }

            for (int z = zvals[0]; z <= zvals[1]; z++)
            {
                if (nearWater)
                {
                    break;
                }

                for (int x = 0; x < xvals.Count; x++)
                {
                    if (FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.NearWater))
                    {
                        nearWater = true;
                        break;
                    }
                }
            }

            if (nearWater)
            {
                continue;
            }


            // This obtuseness stuff below is representative of 3 points:
            // start, end and the current (x,z) that's somewhere near them.
            // Those three points make a triangle, and the obtusness
            // is a mreasurement of how obtuse this triangle can be.


            float endDistScale = RandUtils.FloatRange(1.5f, 4.0f, rand);
            float edgeDistScale = RandUtils.FloatRange(3.0f, 7.0f, rand);

            float baseObtuseness = RandUtils.FloatRange(0.95f, 1.40f, rand);
            // The obtuseness allowed for the dug out area increases as we
            // move toward the edge of the region, but randomize how much
            // it can increase so some walls don't curve out so much
            // 1.0f was the original value here.
            float obtusnessIncreaseNearEdgesScale = RandUtils.FloatRange(1.2f, 2.0f, rand);

            // Normally the walls stop after the obtuse triangle gets too big.
            // 0.7f was the original value here
            //float maxExtraObtusenessAllowed = RandUtils.FloatRange (0.6f,0.85f,rand);
            float maxExtraObtusenessAllowed = RandUtils.FloatRange(0.5f, 0.7f, rand);


            float holeDepthScale = RandUtils.FloatRange(1.2f, 1.6f, rand) * (float)Math.Sqrt(lengthMult);


            // Used for scaling how far up the ends of the roads go to make them
            // easier to walk on
            float bridgeEndRoadSmoothScale = 0.20f;

            // If this is a crevice bridge, use some special stats)
            if (centerPointIn != null)
            {
                fullcl = (int)(fullcl * 1.4f);
                holeDepthScale *= 1.1f;

            }

            List<float[,]> noises = new List<float[,]>();

            int numNoises = 2;

            int noiseSize = fullcl * 2 + 1;
            for (int n = 0; n < numNoises; n++)
            {
                float freq = RandUtils.FloatRange(0.02f, 0.05f, rand) * noiseSize;
                float amp = RandUtils.FloatRange(0.4f, 0.8f, rand);
                float pers = RandUtils.FloatRange(0.2f, 0.5f, rand);
                int octaves = 2;

                float[,] noise = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), noiseSize, noiseSize);
                noises.Add(noise);
            }


            List<Point2F> loweredPoints = new List<Point2F>();

            ipx = (int)(px);
            ipz = (int)(pz);

            if (ipz % MapConstants.TerrainPatchSize > MapConstants.TerrainPatchSize - 1 ||
                ipx % MapConstants.TerrainPatchSize > MapConstants.TerrainPatchSize - 1)
            {
                continue;
            }


            for (int x = cx - fullcl; x <= cx + fullcl; x++)
            {
                if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
                {
                    continue;
                }
                for (int z = cz - fullcl; z <= cz + fullcl; z++)
                {
                    if (z < 0 || z >= _mapProvider.GetMap().GetHhgt())
                    {
                        continue;
                    }
                    float cdist = MathUtil.Sqrt((x - cx) * (x - cx) + (z - cz) * (z - cz));
                    float sdist = MathUtil.Sqrt((x - sx) * (x - sx) + (z - sz) * (z - sz));
                    float edist = MathUtil.Sqrt((x - ex) * (x - ex) + (z - ez) * (z - ez));

                    int ax = (int)(1.0f * x / _mapProvider.GetMap().GetHwid() * _md.Awid);
                    int az = (int)(1.0f * z / _mapProvider.GetMap().GetHhgt() * _md.Ahgt);
                    // Set splats under the bridge to base terrain

                    // Get length of hypotenuse and legs of the b e s triangle.

                    float legSum = MathUtil.Sqrt(edist * edist + sdist * sdist);

                    bool closeToMid = false;
                    if (false && Math.Max(edist, sdist) < bridgeLength + 2.0f)
                    {
                        closeToMid = true;
                        _md.ClearAlphasAt(ax, az);
                        _md.Alphas[ax, az, TerrainTexChannels.Base] = 0.99f;
                        _md.Alphas[ax, az, TerrainTexChannels.Road] = 0.01f;
                    }

                    float minEndPtSize = 2.0f;
                    float maxEndPtSize = 3.5f + lengthMult - 1;
                    float legSumVal = 3.3f;

                    float distDiff = Math.Abs(edist - sdist);
                    // Don't sink the road if it's near the endpoints.
                    if ((edist > bdist - minEndPtSize || sdist > bdist - minEndPtSize) &&
                        (sdist <= maxEndPtSize || edist <= maxEndPtSize))
                    {
                        if (bdist <= legSum + legSumVal && distDiff >= bdist - minEndPtSize * 2)
                        {
                            float backDist = Math.Max(0, Math.Max(sdist, edist) - bdist);
                            float delta = bridgeEndRoadSmoothScale * backDist / MapConstants.MapHeight;
                            _md.Heights[x, z] = MathUtil.Clamp(heightAtEnds, _md.Heights[x, z],
                                                         heightAtEnds + delta);
                            if (!closeToMid)
                            {
                                //_md.ClearAlphasAt(ax, az);
                                //_md.alphas[ax, az, TerrainTexChannels.Road] = 1.0f;
                            }
                            continue;
                        }
                    }

                    float radiusPercent = cdist / fullcl;
                    float obtusenessMult = baseObtuseness * (1 - (0.1f * radiusPercent +
                                                              0.2f * radiusPercent * radiusPercent)
                                                           * obtusnessIncreaseNearEdgesScale);

                    float smoothingScale = 1.0f;


                    // If too far along an obtuse angle, lower areas not near the
                    // road
                    float ratio = 0.0f;

                    if (sdist * sdist + bdist * bdist <= edist * edist * obtusenessMult)
                    {
                        ratio = (sdist * sdist + bdist * bdist) / (edist * edist);
                    }
                    if (edist * edist + bdist * bdist <= sdist * sdist * obtusenessMult)
                    {
                        float ratio2 = (edist * edist + bdist * bdist) / (sdist * sdist);
                        if (ratio2 < ratio || ratio == 0)
                        {
                            ratio = ratio2;
                        }
                    }

                    if (ratio > 0)
                    {
                        float minObtuseness = obtusenessMult * maxExtraObtusenessAllowed;

                        if (ratio < minObtuseness)
                        {
                            smoothingScale = 0.0f;
                            continue;
                        }
                        else
                        {
                            smoothingScale = (ratio - minObtuseness) / (obtusenessMult - minObtuseness) * 1.0f;
                        }

                    }


                    float distFromBridgeEnd = Math.Min(edist, sdist) * endDistScale;
                    float distFromEdge = (fullcl * 0.95f - cdist) / fullcl * edgeDistScale;
                    float depthMult = Math.Min(distFromEdge, distFromBridgeEnd);
                    if (depthMult < 0)
                    {
                        depthMult = 0;
                    }

                    int rad = 4;


                    float aveSplat = 0;
                    float distToRoad = _md.RoadDistances[ax, az];
                    if (distToRoad < rad)
                    {
                        aveSplat = _md.GetAverageSplatNear(ax, az, rad, TerrainTexChannels.Road);
                    }
                    float currSplat = _md.Alphas[ax, az, TerrainTexChannels.Road];
                    float maxRoadSplatAllowed = 0.50f;

                    currSplat = 0;
                    // If this cell is near another bridge, don't let it get sunk.
                    if (currSplat > 0)
                    {
                        if (_md.BridgeDistances[x, z] < 20)
                        {
                            continue;
                        }
                    }

                    // Are we too close to a straight line?
                    bool almostStraightLineAlongBridge = false;
                    float almostStraightScale = 1.03f;

                    if ((edist > bdist &&
                         (bdist + sdist) <= almostStraightScale * edist) ||
                        (sdist > bdist &&
                     (bdist + edist) <= almostStraightScale * sdist))
                    {
                        almostStraightLineAlongBridge = true;
                    }

                    if (edist < bdist - 1 && sdist < bdist - 1)
                    {
                        currSplat = 0;
                        aveSplat = aveSplat * aveSplat;
                    }
                    if (almostStraightLineAlongBridge)
                    {

                        depthMult = 0;
                    }
                    else
                    {
                        depthMult *= (1 - aveSplat) / (maxRoadSplatAllowed);

                        // If this thing is far away from the main road, repaint the 
                        // cell as base.
                        if (currSplat > 0)
                        {
                            // If encounter a road segment not near the current road, ignore
                            // it.
                            float minDist = 10000;

                            foreach (LineCell rd in road)
                            {
                                float currDist = MathUtil.Sqrt((x - rd.X) * (x - rd.X) +
                                                             (z - rd.Z) * (z - rd.Z));
                                if (currDist < minDist)
                                {
                                    minDist = currDist;
                                }
                            }

                            if (minDist >= 10)
                            {
                                _md.ClearAlphasAt(x, z);
                                _md.Alphas[x, z, TerrainTexChannels.Base] = 1;
                            }
                        }
                    }

                    // How far down the hole goes
                    float holeDepth = depthMult / MapConstants.MapHeight * holeDepthScale * smoothingScale;
                    if (holeDepth > 0)
                    {
                        loweredPoints.Add(new Point2F(ax, az));
                    }
                    float locy = _md.Heights[x, z];
                    if (locy < cyscale)
                    {
                        holeDepth -= (cyscale - locy) * 0.95f;
                        if (holeDepth < 0)
                        {
                            holeDepth = 0;
                        }
                    }

                    float noiseDepthScale = 1.0f;

                    foreach (float[,] noise in noises)
                    {
                        noiseDepthScale *= (1 + MathUtil.Clamp(-1, noise[x - (cx - fullcl), z - (cz - fullcl)], 1));
                    }

                    holeDepth *= noiseDepthScale;

                    _md.Heights[x, z] -= holeDepth;

                    if (cdist < halfBridgeLength + 2 && distFromBridgeEnd > 2)
                    {
                        float currRoad = _md.Alphas[ax, az, TerrainTexChannels.Road];
                        _md.Alphas[ax, az, TerrainTexChannels.Road] = 0;
                        _md.Alphas[ax, az, TerrainTexChannels.Base] += currRoad / 2;
                        _md.Alphas[ax, az, TerrainTexChannels.Dirt] += currRoad / 2;
                    }

                }
            }


            if (ipx >= 0 && ipz >= 0 && ipx < _mapProvider.GetMap().GetHwid() - 1 && ipz < _mapProvider.GetMap().GetHhgt() - 1)
            {
                if (!_md.SetEntityData(ipx, ipz, EntityTypes.Bridge, bt.IdKey))
                {
                    continue;
                }
                float clampedAngle = angle;
                while (clampedAngle < 0)
                {
                    clampedAngle += MapConstants.FullCircleAngle;
                }

                while (clampedAngle >= 360)
                {
                    clampedAngle -= MapConstants.FullCircleAngle;
                }

                ushort finalAngle = (ushort)angle;

                ushort finalHeight = (ushort)(bridgeHeight * MapConstants.ObjectHeightMult);


                int maxDelta = 3;
                int midLen = (int)(bridgeLength * 1.5);


                for (int poolTries = 0; poolTries < 10; poolTries++)
                {
                    int poolx = ipx + RandUtils.IntRange(-maxDelta, maxDelta, rand);
                    int poolz = ipz + RandUtils.IntRange(-maxDelta, maxDelta, rand);

                    WaterGenData wgd = new WaterGenData()
                    {
                        x = poolx,
                        z = poolz,
                        maxHeight = bridgeHeight,
                    };

                    if (poolx != ipx || poolz != ipz)
                    {

                        _waterGenData.Add(wgd);
                        break;
                    }
                }

                _md.ExtendedObjects[ipx, ipz] = new ExtendedWorldObjectData()
                {
                    X = ipx,
                    Z = ipz,
                    EntityTypeId = EntityTypes.Bridge,
                    EntityId = bt.IdKey,
                    Angle = finalAngle,
                    Height = finalHeight,
                };

                _md.CurrBridges.Add(new Point2I(ipx, ipz));
                SetBridgeDistancesNear((int)(ipx), (int)(ipz));

            }


            break;
        }

    }



    public BridgeType GetRandomBridgeType(long zoneTypeId, MyRandom rand)
    {
        ZoneType zt = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);
        if (zt == null || zt.BridgeTypes == null || zt.BridgeTypes.Count < 1)
        {
            return null;
        }

        double totalChance = 0;
        double chanceChosen = 0;


        for (int times = 0; times < 2; times++)
        {
            for (int b = 0; b < zt.BridgeTypes.Count; b++)
            {
                ZoneBridgeType zbt = zt.BridgeTypes[b];
                BridgeType bt = _gameData.Get<BridgeTypeSettings>(_gs.ch).Get(zbt.BridgeTypeId);
                if (bt != null && !string.IsNullOrEmpty(bt.Art))
                {
                    if (times == 0)
                    {
                        totalChance += zbt.Weight;
                    }
                    else
                    {
                        chanceChosen -= zbt.Weight;
                        if (chanceChosen <= 0)
                        {
                            return bt;
                        }
                    }
                }
            }

            if (times == 0)
            {
                if (totalChance < 1)
                {
                    return null;
                }
                chanceChosen = rand.NextDouble() * totalChance;
            }
            else
            {
                break;
            }

        }

        return null;

    }

    protected void SetBridgeDistancesNear(int cx, int cz)
    {
        if (_md.BridgeDistances == null)
        {
            return;
        }

        int bridgeRadius = MapConstants.MaxBridgeCheckDistance;
        for (int xx = cx - bridgeRadius; xx <= cx + bridgeRadius; xx++)
        {
            if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
            {
                continue;
            }
            float dbx = cx - xx;

            for (int zz = cz - bridgeRadius; zz <= cz + bridgeRadius; zz++)
            {
                if (zz < 0 || zz >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }
                float dbz = zz - cz;
                float dist = (float)Math.Sqrt(dbx * dbx + dbz * dbz);
                if (dist < bridgeRadius)
                {
                    if (dist < _md.BridgeDistances[xx, zz])
                    {
                        _md.BridgeDistances[xx, zz] = (ushort)dist;
                    }
                }
            }
        }
    }
}



