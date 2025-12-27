using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Categories.GameSettings;

namespace Genrpg.Shared.ProcGen.Settings.Trees
{
    /// <summary>
    /// Data about a particular tree type used in terrain generator
    /// </summary>
    /// 
    public class TreeFlags
    {
        public const int IsBush = 1 << 0;
        public const int IsWaterItem = 1 << 1;
        public const int NoNearbyItems = 1 << 2;
        public const int DirectPlaceObject = 1 << 3;
    }

    public class TreeType : ChildSettings, IVariationIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public float Scale { get; set; } = 1.0f;

        public int VariationCount { get; set; } = 1;

        public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }
        public TreeType()
        {
        }
    }
}


