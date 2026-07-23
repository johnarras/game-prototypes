using Unity.Entities;

namespace OxDb.Client.Lockstep.Maps.Components
{
    public struct MapLibrary : IComponentData
    {
        // A list of available maps the simulation can load
        public BlobAssetReference<BlobArray<BlobAssetReference<MapBlob>>> Maps;
    }
}
