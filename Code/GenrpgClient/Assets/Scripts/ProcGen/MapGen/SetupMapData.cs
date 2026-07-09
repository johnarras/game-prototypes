
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.Spawns.WorldData;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SetupMapData : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        if (_md == null)
        {
            _zoneGenService.ShowGenError("Missing MapData");
            return;
        }

        if (_mapProvider.GetMap() == null)
        {
            _zoneGenService.ShowGenError("No world found");
            return;
        }
        if (_mapProvider.GetSpawns() == null || _mapProvider.GetSpawns().Data.Count < 1)
        {
            _mapProvider.SetSpawns(new MapSpawnData() { Id = _mapProvider.GetMap().Id.ToString() });
        }



        int mapSize = _mapProvider.GetMap().GetHwid();

        _md.DWid = mapSize;
        _md.DHgt = mapSize;
        _md.Ahgt = mapSize;
        _md.Awid = mapSize;

        if (string.IsNullOrEmpty(_zoneGenService.LoadedMapId))
        {
            for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
            {
                for (int gz = 0; gz < _mapProvider.GetMap().BlockCount; gz++)
                {
                    _awaitableService.ForgetAwaitable(_terrainManager.SetupOneTerrainPatch(gx, gz, token));
                }
            }
        }

        if (string.IsNullOrEmpty(_zoneGenService.LoadedMapId))
        {
            _md.GrassAmounts = new byte[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt(), MapConstants.MaxGrass];

            _md.MapZoneIds = new short[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];

            _md.SubZonePercents = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.SubZoneIds = new int[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.OverrideZoneScales = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];

            _terrainManager.SetAllTerrainNeighbors();

            _md.Alphas = new float[_md.Awid, _md.Ahgt, TerrainTexChannels.Max];
            _md.Heights = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];


            _md.ZoneCenters = new List<Point2I>();
            _md.MaintainHeights = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.NearestMountainTopHeight = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.MountainDistPercent = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.EdgeMountainDistPercent = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.MountainCenterDist = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.Flags = new int[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.RoadDistances = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];

            _md.EntityTypeIds = new byte[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.EntityIds = new byte[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.ExtendedObjects = new ExtendedWorldObjectData[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];

            for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
            {
                for (int z = 0; z < _mapProvider.GetMap().GetHhgt(); z++)
                {
                    _md.MapZoneIds[x, z] = 0;
                    _md.RoadDistances[x, z] = MapConstants.InitialRoadDistance;
                    _md.MountainDistPercent[x, z] = 1.0f;
                    _md.EdgeMountainDistPercent[x, z] = 1.0f;
                    _md.MountainCenterDist[x, z] = MapConstants.InitialMountainDistance;
                    _md.Flags[x, z] = 0;
                }

            }

            for (int x = 0; x < _md.Awid; x++)
            {
                for (int z = 0; z < _md.Ahgt; z++)
                {
                    _md.Alphas[x, z, TerrainTexChannels.Base] = 1.0f;
                }
            }
        }
        else
        {
            _md.Flags = null;
            _md.Alphas = null;
            _md.Heights = null;
            _md.RoadDistances = null;
            _md.Roads = null;
            _md.BridgeDistances = null;
        }
    }
}

