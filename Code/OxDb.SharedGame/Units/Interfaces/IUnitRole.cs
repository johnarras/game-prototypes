using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Units.Interfaces
{
    public interface IUnitRole : IIndexedGameItem
    {
        public string PluralName { get; set; }
        int MinRange { get; set; }
        long MinLevel { get; set; }
        List<Effect> Effects { get; set; }
    }
}


