using Assets.Scripts.MapTerrain;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.ProcGen.Constants;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

public class SaveMap : BaseZoneGenerator
{

    private ITextSerializer _textSerializer = null;

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        _mapProvider.GetMap().OverrideZonePercent = 0; // RandUtils.IntRange(20, 80, _rand.Rand);

        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gy = 0; gy < _mapProvider.GetMap().BlockCount; gy++)
            {
                await SaveOneTerrainPatch(gx, gy);
            }
        }
    }


    public async Awaitable SaveOneTerrainPatch(int gx, int gy)
    {

        List<ExtendedWorldObjectData> extendedObjects = new List<ExtendedWorldObjectData>();


        TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gy);

        if (patch == null)
        {
            _logService.Error("NO patch at " + gx + " " + gy);
            return;
        }

        if (patch.FullZoneIdList == null || patch.FullZoneIdList.Count < 1)
        {
            _logService.Error("No zone list at " + gx + " " + gy);
            return;
        }



        int maxFileSize = MapConstants.TerrainPatchSize * MapConstants.TerrainPatchSize * MapConstants.TerrainBytesPerUnit;

        byte[] origBytes = new byte[maxFileSize];
        int startX = gy * (MapConstants.TerrainPatchSize - 1);
        int startY = gx * (MapConstants.TerrainPatchSize - 1);


        ushort shortHeight = 0;
        int index = 0;

        // 1. Heights 2
        // 2. Objects 2
        // 3. Alphas 3
        // 4. Zone 1 
        // 5. SubZone 1
        // 6. OverrideZoneScale 1
        // Then extended objects?

        // 2 + 2 + 3 + 1 + 1 + 1 = 10;
        // Then extended bytes after for special objects.

        // 1 Heights: 2 bytes
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                shortHeight = (ushort)(MapConstants.HeightSaveMult * _md.Heights[x + startX, y + startY]);

                origBytes[index++] = (byte)(shortHeight);
                origBytes[index++] = (byte)(shortHeight >> 8);
            }
        }

        int objStartX = gx * (MapConstants.TerrainPatchSize - 1);
        int objStartY = gy * (MapConstants.TerrainPatchSize - 1);

        // 2 Objects: 2 bytes
        for (int x = 0; x < MapConstants.TerrainPatchSize - 1; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize - 1; y++)
            {
                int fx = x + objStartX;
                int fy = y + objStartY;

                long entityTypeId = _md.EntityTypeIds[fy, fx];
                long entityId = _md.EntityIds[fy, fx];


                if (x == MapConstants.TerrainPatchSize - 1 || y == MapConstants.TerrainPatchSize - 1)
                {
                    if (entityTypeId == EntityTypes.Plant)
                    {
                        entityTypeId = 0;
                        entityId = 0;
                    }
                }

                ExtendedWorldObjectData extObj = _md.ExtendedObjects[fy, fx];

                if (extObj != null)
                {
                    extendedObjects.Add(extObj);
                }

                origBytes[index++] = (byte)entityTypeId;
                origBytes[index++] = (byte)entityId;
            }
        }
        // 3 Alphas: 3 bytes (*divsq)
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {

                for (int i = 0; i < TerrainTexChannels.Max - 1; i++)
                {
                    origBytes[index++] = (byte)(_md.Alphas[x + startX, y + startY, i] * MapConstants.AlphaSaveMult);
                }
            }
        }


        List<long> zoneIds = new List<long>();
        // 4 Zones: 1 byte (*divsq)
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                byte zid = (byte)_md.MapZoneIds[x + startX, y + startY];
                if (zid <= MapConstants.MountainZoneId)
                {
                    _logService.Error("Found bad zoneId at " + (x + startX) + " " + (y + startY));
                }
                origBytes[index++] = zid;
                if (!zoneIds.Contains(zid))
                {
                    zoneIds.Add(zid);
                }
            }
        }
        // 5 subZoneIds 1 byte
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                origBytes[index++] = (byte)(_md.SubZoneIds[x + startX, y + startY]);
            }
        }

        // 6 OverrideZoneScale 1 byte 0 to MapConstants.OverrideZoneScaleMax
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                float val = MathUtil.Clamp(0, Math.Abs(_md.OverrideZoneScales[x + startX, y + startY]), 1);

                origBytes[index++] = (byte)(val * MapConstants.OverrideZoneScaleMax);
            }
        }

        string extendedOutput = _textSerializer.SerializeToString(extendedObjects);

        byte[] extBytes = Encoding.UTF8.GetBytes(extendedOutput);

        byte[] finalBytes = ByteUtils.ConcatenateArrays(origBytes, extBytes);

        await _clientRepoService.SaveBytes(patch.GetFilePath(), finalBytes);

    }
}



