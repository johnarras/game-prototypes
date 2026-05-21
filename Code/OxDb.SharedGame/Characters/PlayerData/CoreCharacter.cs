using MessagePack;
using OxDb.SharedCore.DataStores.Constants;
using OxDb.SharedCore.GameSettings.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using System;
using System.Linq;

namespace OxDb.SharedGame.Characters.PlayerData
{
    [MessagePackObject]
    public class CoreCharacter : NoChildIndexedUserData, ICoreCharacter
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string UserId { get; set; }
        [Key(3)] public int Version { get; set; }
        [Key(4)] public string MapId { get; set; }
        [Key(5)] public DateTime Created { get; set; } = DateTime.UtcNow;
        [Key(6)] public string Client { get; set; } = VersionConstants.MinVersion.ToString();
        [Key(7)] public long EntityTypeId { get; set; }
        [Key(8)] public long EntityId { get; set; }
        [Key(9)] public float X { get; set; }
        [Key(10)] public float Y { get; set; }
        [Key(11)] public float Z { get; set; }
        [Key(12)] public float Rot { get; set; }
        [Key(13)] public float Speed { get; set; }
        [Key(14)] public long ZoneId { get; set; }
        [Key(15)] public long Level { get; set; } = 1;
        [Key(16)] public long FactionTypeId { get; set; }
        [Key(17)] public long AddonBits { get; set; }
        [Key(18)] public long SexTypeId { get; set; }
        [Key(19)] public ABList AB { get; set; } = new ABList();
        public string GetId() { return Id; }

        public string GetDocName(long settingsNameId)
        {
            if (_overrideList == null)
            {
                return GameDataConstants.DefaultFilename;
            }
            ABItem item = _overrideList.Items.FirstOrDefault(x => x.SettingsId == settingsNameId);
            return item?.DocId ?? GameDataConstants.DefaultFilename;
        }


        private ABList _overrideList = null;

        public ABList GetGameDataOverrides()
        {
            return _overrideList;
        }

        public void SetGameDataOverrides(ABList overrideList)
        {
            _overrideList = overrideList;
        }
    }
}


