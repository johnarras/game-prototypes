using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.Interfaces;

namespace OxDb.SharedGame.ProcGen.Settings.Locations
{
    public class LocationType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }

        public int XSize { get; set; }
        public int ZSize { get; set; }

        public string SetupType { get; set; }
        public string Art { get; set; }
    }
}


