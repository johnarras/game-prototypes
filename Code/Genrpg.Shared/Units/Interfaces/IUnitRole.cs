using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Units.Interfaces
{
    public interface IUnitRole : IIndexedGameItem
    {
        public string PluralName { get; set; }
        int MinRange { get; set; }
        long MinLevel { get; set; }
        List<Effect> Effects { get; set; }
    }
}


