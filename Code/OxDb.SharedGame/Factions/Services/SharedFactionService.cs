using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Factions.Constants;
using OxDb.SharedGame.Factions.PlayerData;
using OxDb.SharedGame.Units.Entities;

namespace OxDb.SharedGame.Factions.Services
{

    public interface ISharedFactionService : IInjectable
    {
        long GetRepLevel(Unit unit, long factionTypeId);
        bool CanInteract(Unit unit, long factionTypeId);
        bool CanFight(Unit unit, long factionTypeId);
        bool WillAttack(Unit unit, long factionTypeId);
        long GetRep(Unit unit, long factionTypeId);
        void SetRep(Unit unit, long factionTypeId, long val);
        void AddRep(Unit unit, long factionTypeId, long val);
    }

    public class SharedFactionService : ISharedFactionService
    {
        private IGameData _gameData = null;


        public long GetRep(Unit unit, long factionTypeId)
        {
            ReputationData repData = unit.Get<ReputationData>();
            return repData.Data[factionTypeId];
        }

        public long GetRepLevel(Unit unit, long factionTypeId)
        {
            return 0;
        }

        public bool CanInteract(Unit unit, long factionTypeId)
        {
            return GetRepLevel(unit, factionTypeId) >= RepLevels.Neutral;
        }

        public bool CanFight(Unit unit, long factionTypeId)
        {
            return GetRepLevel(unit, factionTypeId) <= RepLevels.Unfriendly;
        }

        public bool WillAttack(Unit unit, long factionTypeId)
        {
            return GetRepLevel(unit, factionTypeId) <= RepLevels.Hostile;
        }

        public void AddRep(Unit unit, long factionTypeId, long quantity)
        {
            SetRep(unit, factionTypeId, quantity + GetRep(unit, factionTypeId));
        }

        public void SetRep(Unit unit, long factionTypeId, long quantity)
        {
            if (quantity < 0)
            {
                quantity = 0;
            }

            ReputationData repData = unit.Get<ReputationData>();

            repData.Data[factionTypeId] += quantity;
        }
    }
}


