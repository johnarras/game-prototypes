using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.Stats.Entities;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Inventory.Settings.ItemSets
{
    public class SetType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public List<SetPiece> Pieces { get; set; }

        public List<SetStat> Stats { get; set; }

        public List<SetSpellProc> Procs { get; set; }


        public SetType()
        {
            Pieces = new List<SetPiece>();
            Stats = new List<SetStat>();
            Procs = new List<SetSpellProc>();
        }
    }
    public class SetStat : IStatPct
    {
        public int ItemCount { get; set; }
        public long StatTypeId { get; set; }
        public int Percent { get; set; }
        public string Name { get; set; }
    }
    public class SetSpellProc : IOldSpellProc
    {
        public int Chance { get; set; }
        public long SpellId { get; set; }
        public int Cooldown { get; set; }
        public long ProcTypeId { get; set; }
        public long FromElementTypeId { get; set; }
        public long FromSkillTypeId { get; set; }
        public int Scale { get; set; }
        public int ItemCount { get; set; }
        public string Name { get; set; }
    }
    public class SetPiece
    {
        public long ItemTypeId { get; set; }
        public string Name { get; set; }

        public List<StatPct> Stats { get; set; } = new List<StatPct>();

        public List<OldSpellProc> OldProcs { get; set; } = new List<OldSpellProc>();

        public SetPiece()
        {
        }

    }
}


