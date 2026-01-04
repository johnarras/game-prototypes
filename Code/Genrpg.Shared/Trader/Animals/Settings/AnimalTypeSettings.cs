using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Animals.Settings
{
    public class AnimalTypeSettings : ParentSettings<AnimalType>
    {
        public override string Id { get; set; }
    }

    public class AnimalType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long Speed { get; set; }
        public long Capacity { get; set; }
        public long Upkeep { get; set; }
        public long Price { get; set; }
        public bool StartsUnlocked { get; set; }
    }


    public class AnimalSettingsLoader :
        ParentSettingsLoader<AnimalTypeSettings, AnimalType>
    { }


    public class AnimalSettingsMapper :
        ParentSettingsMapper<AnimalTypeSettings, AnimalType, AnimalSettingsDto>
    { }

    public class AnimalSettingsDto : ParentSettingsDto<AnimalTypeSettings, AnimalType>
    {
        public override List<AnimalType> Children { get; set; }
        public override AnimalTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }


    public class AnimalEntityHelper : BaseEntityHelper<AnimalTypeSettings, AnimalType>
    {
        public override long HelperKey => EntityTypes.Animal;
    }
}


