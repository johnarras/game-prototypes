

using Assets.Scripts.MapTerrain;
using OxDb.SharedGame.ProcGen.Constants;
using System.Collections.Generic;
using System.Threading;
using UnityEngine; // Needed

public class SetBasicTerrainTextures : BaseZoneGenerator
{

    protected ITerrainTextureManager _terrainTextureManager = null;

    public class MaterialChosen
    {
        public int TextureTypeId;
        public int SplatmapIndex;
        public int ZoneTypeId;

        public Texture2D RegTexture;
        public Texture2D NormTexture;

    }

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        // Set up splat prototypes based on the things given in the zone data, if any
        // and then fall back to the defaults if that fails.
        // And if all of those fail and anything loads, use the thing that loaded
        // for all channels to try to deal with errors.
        TerrainLayer[] layers = new TerrainLayer[TerrainTexChannels.Max];

        for (int s = 0; s < layers.Length; s++)
        {
            layers[s] = _terrainTextureManager.CreateTerrainLayer(_terrainTextureManager.GetBasicTerrainTexture(s));
        }

        List<IndexedTerrainLayer> indexedList = new List<IndexedTerrainLayer>();

        for (int l = 0; l < layers.Length; l++)
        {
            indexedList.Add(new IndexedTerrainLayer() { Index = l, TerrainLayer = layers[l] });
        }
        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gz = 0; gz < _mapProvider.GetMap().BlockCount; gz++)
            {
                TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gz);
                if (patch == null)
                {
                    continue;
                }
                TerrainData tdata = patch.Core.TerrainData as TerrainData;
                if (tdata != null)
                {
                    tdata.terrainLayers = layers;
                }

                patch.Core.Layers = indexedList;
            }
        }
    }
}



