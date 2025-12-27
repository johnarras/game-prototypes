using MessagePack;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Buffs.Settings
{

    public class PartyBuff : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public double ProcChanceScale { get; set; }
        public double EffectScale { get; set; }
    }


    public class PartyBuffSettings : ParentConstantListSettings<PartyBuff, PartyBuffs>
    {
        public override string Id { get; set; }
        public double BuffPowerPerLevel { get; set; }


        public double GetEffectScale(long partyBuffId)
        {
            return Get(partyBuffId)?.EffectScale ?? 1;

        }

        public double GetProcChanceScale(long partyBuffId)
        {
            return Get(partyBuffId)?.ProcChanceScale ?? 1;
        }
    }

    public class PartyBuffSettingsDto : ParentSettingsDto<PartyBuffSettings, PartyBuff>
    {
        public override List<PartyBuff> Children { get; set; }
        public override PartyBuffSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class PartyBuffSettingsLoader : ParentSettingsLoader<PartyBuffSettings, PartyBuff> { }


    public class PartyBuffSettingsMapper : ParentSettingsMapper<PartyBuffSettings, PartyBuff, PartyBuffSettingsDto> { }



    public class PartyBuffHelper : BaseEntityHelper<PartyBuffSettings, PartyBuff>
    {
        public override long HelperKey => EntityTypes.PartyBuff;
    }

}


