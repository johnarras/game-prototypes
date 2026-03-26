using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Trader.Encounters.Settings
{
    public class TravelEncounterSettings : ParentSettings<TravelEncounter>
    {
        public override string Id { get; set; }
        public double GoodEncounterChance { get; set; }

        public double BadEncounterChance { get; set; }

        public override void SetData(List<TravelEncounter> data)
        {
            base.SetData(data);

            _goodEncounters = data.Where(x => !x.BadEffects.Any()).ToList();
            _badEncounters = data.Where(x => x.BadEffects.Any()).ToList();
        }

        private List<TravelEncounter> _goodEncounters { get; set; } = new List<TravelEncounter>();

        public IReadOnlyList<TravelEncounter> GetGoodEncounters()
        {
            return _goodEncounters;
        }

        private List<TravelEncounter> _badEncounters { get; set; } = new List<TravelEncounter>();

        public IReadOnlyList<TravelEncounter> GetBadEncounters()
        {
            return _badEncounters;
        }


    }

    public class TravelEncounter : ChildSettings, IIndexedGameItem, IWeightedItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }

        public string Text { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public double Weight { get; set; }

        public List<SpawnItem> GoodEffects { get; set; } = new List<SpawnItem>();

        public List<SpawnItem> BadEffects { get; set; } = new List<SpawnItem>();

        public List<SpawnItem> FailureEffects { get; set; } = new List<SpawnItem>();


    }
    public class TravelEncounterSettingsLoader : ParentSettingsLoader<TravelEncounterSettings, TravelEncounter> { }

    // No DTO Or mapper, don't send to client.
}


