using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Charms.PlayerData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Charms.Services
{
    public interface ICharmService : IInjectable
    {
        List<PlayerCharmBonusList> CalcBonuses(string charmId);

        List<string> PrintBonuses(PlayerCharmBonusList list);
    }
}


