using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Interfaces;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Spells.Settings.Spells
{
    public class SpellType : ChildSettings, ISpell
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }

        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long ElementTypeId { get; set; }
        public long PowerStatTypeId { get; set; }
        public int PowerCost { get; set; }
        public int Cooldown { get; set; }
        public float CastTime { get; set; }
        public int MinRange { get; set; } = SpellConstants.MinRange;
        public int MaxRange { get; set; } = SpellConstants.MaxRange;
        public int MaxCharges { get; set; }
        public int Shots { get; set; }

        public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public List<SpellEffect> Effects { get; set; } = new List<SpellEffect>();

        public SpellType()
        {
        }
    }


    [MessagePackObject]
    public class SpellEffect
    {
        [Key(0)] public long SkillTypeId { get; set; } = 1;
        [Key(1)] public long EntityTypeId { get; set; }
        [Key(2)] public long EntityId { get; set; }
        [Key(3)] public int Radius { get; set; }
        [Key(4)] public int Duration { get; set; }
        [Key(5)] public int ExtraTargets { get; set; }
        [Key(6)] public int Scale { get; set; }
        [Key(7)] public int Flags { get; set; }
        [Key(8)] public string Name { get; set; }

        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }
    }

    public class SpellTypeSettings : ParentSettings<SpellType>
    {
        public override string Id { get; set; }
    }

    public class SpellTypeSettingsDto : ParentSettingsDto<SpellTypeSettings, SpellType>
    {
        public override List<SpellType> Children { get; set; }
        public override SpellTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class SpellTypeSettingsLoader : ParentSettingsLoader<SpellTypeSettings, SpellType> { }

    public class SpellTypeSettingsMapper : ParentSettingsMapper<SpellTypeSettings, SpellType, SpellTypeSettingsDto> { }



    public class SpellHelper : BaseEntityHelper<SpellTypeSettings, SpellType>
    {
        public override long HelperKey => EntityTypes.Spell;
    }
}


