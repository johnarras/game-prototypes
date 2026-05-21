using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.CaravanMembers.Settings
{
    public class CaravanMemberSettings : ParentSettings<CaravanMember>
    {
        public override string Id { get; set; }

        public long MoveOutsideCityCostMult { get; set; }
    }


    public class CaravanMember : ChildSettings, IIndexedGameItem, IEffectList<Effect>
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public int Speed { get; set; }
        public int Size { get; set; }
        public long Price { get; set; }
        public long DefaultSkinTypeId { get; set; }

        public List<Effect> Effects { get; set; } = new List<Effect>();
    }


    public class CaravanMemberSettingsLoader :
        ParentSettingsLoader<CaravanMemberSettings, CaravanMember>
    { }


    public class CaravanMemberSettingsMapper :
        ParentSettingsMapper<CaravanMemberSettings, CaravanMember, CaravanMemberSettingsDto>
    { }

    public class CaravanMemberSettingsDto : ParentSettingsDto<CaravanMemberSettings, CaravanMember>
    {
        public override List<CaravanMember> Children { get; set; }
        public override CaravanMemberSettings Parent { get; set; }
        public override string Id { get; set; }
    }


    public class CaravanMemberEntityHelper : BaseEntityHelper<CaravanMemberSettings, CaravanMember>
    {
        public override long HelperKey => EntityTypes.CaravanMember;
    }
}


