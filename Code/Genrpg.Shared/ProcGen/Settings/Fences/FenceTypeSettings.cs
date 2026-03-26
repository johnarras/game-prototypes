using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.ProcGen.Settings.Fences
{
    public class FenceType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public float Length { get; set; }

        public FenceType()
        {
            Art = "Fence";
            Length = 6;
        }

    }
    public class FenceTypeSettings : ParentSettings<FenceType>
    {
        public override string Id { get; set; }
    }

    public class FenceTypeSettingsDto : ParentSettingsDto<FenceTypeSettings, FenceType>
    {
        public override List<FenceType> Children { get; set; }
        public override FenceTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class FenceTypeSettingsLoader : ParentSettingsLoader<FenceTypeSettings, FenceType> { }

    public class FenceSettingsMapper : ParentSettingsMapper<FenceTypeSettings, FenceType, FenceTypeSettingsDto> { }

}


