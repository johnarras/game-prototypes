using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Spells.PlayerData.Spells
{
    [MessagePackObject]
    public class SpellData : OwnerIdObjectList<Spell>
    {
        [Key(0)] public override string Id { get; set; }

        protected override bool CreateMissingChildOnGet() { return false; }

        public void Add(Spell spell)
        {
            _data = _data.Where(x => x.IdKey != spell.IdKey).ToList();
            _data.Add(spell);
            _lookup = null;
        }

        public void Remove(long spellId)
        {
            _data = _data.Where(x => x.IdKey != spellId).ToList();
        }
    }


    [MessagePackObject]
    public class SpellDto : OwnerDtoList<SpellData, Spell>
    {
        [Key(0)] public override List<Spell> Children { get; set; }
        [Key(1)] public override SpellData Parent { get; set; }
        [Key(2)] public override string Id { get; set; }
    }


    public class SpellDataLoader : OwnerIdDataLoader<SpellData, Spell> { }

    public class SpellDataMapper : OwnerDataMapper<SpellData, Spell, SpellDto> { }
}


