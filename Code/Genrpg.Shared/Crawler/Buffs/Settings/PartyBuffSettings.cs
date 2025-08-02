using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Settings.Stats;
using MessagePack;

namespace Genrpg.Shared.Crawler.Buffs.Settings
{

    [MessagePackObject]
    public class PartyBuff : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

    }


    [MessagePackObject]
    public class PartyBuffSettings : ParentConstantListSettings<PartyBuff,PartyBuffs>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class PartyBuffSettingsDto : ParentSettingsDto<PartyBuffSettings, PartyBuff> { }
    public class PartyBuffSettingsLoader : ParentSettingsLoader<PartyBuffSettings, PartyBuff> { }


    public class PartyBuffSettingsMapper : ParentSettingsMapper<PartyBuffSettings, PartyBuff, PartyBuffSettingsDto> { }



    public class PartyBuffHelper : BaseEntityHelper<PartyBuffSettings,PartyBuff>
    {
        public override long Key => EntityTypes.PartyBuff;
    }

}
