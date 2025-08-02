using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Trader.Animals.Settings
{
    [MessagePackObject]
    public class AnimalSettings : ParentSettings<Animal>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class Animal : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public long Endurance { get; set; }
        [Key(9)] public double Speed { get; set; }
        [Key(10)] public long CarryingCapacity { get; set; }
        [Key(11)] public long PullingCapacity { get; set; }
        [Key(12)] public long RoughTerrain { get; set; }
        [Key(13)] public long HotClimate { get; set; }
        [Key(14)] public long ColdClimate { get; set; }
        [Key(15)] public long WetClimate { get; set; }
        [Key(16)] public long Weight { get; set; }
        [Key(17)] public long Cost { get; set; }
        [Key(18)] public long FoodPerDay { get; set; }
        [Key(19)] public long WaterPerDay { get; set; }
    }

    public class AnimalSettingsDto : ParentSettingsDto<AnimalSettings, Animal> { }

    public class AnimalSettingsLoader : ParentSettingsLoader<AnimalSettings, Animal> { }

    public class AnimalSettingsMapper : ParentSettingsMapper<AnimalSettings, Animal, AnimalSettingsDto> { }

    public class AnimalEntityHelper : BaseEntityHelper<AnimalSettings, Animal>
    {
        public override long Key => EntityTypes.Animal;
    }
}
