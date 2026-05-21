using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Characters.Utils;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Factions.Constants;
using OxDb.SharedGame.Inventory.Constants;
using OxDb.SharedGame.Networking.Interfaces;
using OxDb.SharedGame.Trades.Entities;
using OxDb.SharedGame.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Characters.PlayerData
{
    public class Character : Unit, ICoreCharacter
    {
        public string UserId { get; set; }
        public int Version { get; set; }

        public string _etag { get; set; }

        public int AbilityPoints { get; set; }
        public string MapId { get; set; }

        public List<PointXZ> NearbyGridsSeen { get; set; } = new List<PointXZ>();

        public DateTime LastServerStatTime { get; set; } = DateTime.UtcNow;

        public TradeObject Trade { get; set; }
        public ulong TradeModifyLockCount = 0;
        public object TradeLock { get; private set; } = new object();

        public CoreCharacter Core { get; }

        public Character(CoreCharacter core)
        {
            Level = 1;
            QualityTypeId = QualityTypes.Common;
            EntityId = 1;
            FactionTypeId = FactionTypes.Player;
            Core = core;
            CharacterUtils.CopyDataFromTo(core, this);
        }

        public override void Dispose()
        {
            base.Dispose();
            NearbyGridsSeen.Clear();
        }

        public override string GetGroupId()
        {
            return Id;
        }

        public void SetConn(IConnection conn)
        {
            if (_conn != null)
            {
                _conn.ForceClose();
            }
            _conn = conn;
        }

        public override bool IsPlayer() { return true; }


        public override List<IUnitData> GetAllData()
        {
            return _dataDict.Values.ToList();
        }

    }
}


