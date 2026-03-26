using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Charms.Services
{
    public interface ICharmService : IInjectable
    {
        List<PlayerCharmBonusList> CalcBonuses(string charmId);

        List<string> PrintBonuses(PlayerCharmBonusList list);
    }
}


