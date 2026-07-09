using MessagePack;
using OxDb.SharedCore.DataStores.Constants;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using System;
using System.Collections.Generic;

namespace OxDb.SharedGame.Core.PlayerData
{
    [MessagePackObject]
    public class GameAccount : UniquePersonalUserData, IUserData
    {

        public override PersonalDataAccumulation GetAccumulation() { return new PersonalDataAccumulation(); }
        public override int GetOffsetBit() { return PersonalDataOffsetBits.GameAccount; }

        /// <summary>
        /// Used for the id found in the relational database
        /// </summary>
        /// 
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        [Key(2)] public string CurrCharId { get; set; }
        [Key(3)] public string ClientVersion { get; set; } = VersionConstants.MinVersion.ToString();
        [Key(4)] public string AccountId { get; set; }
        [Key(5)] public bool Deleted { get; set; }
        [Key(6)] public string ClientPlatformName { get; set; }
        [Key(7)] public string RefreshToken { get; set; }
        [Key(8)] public string GameUserId { get; set; }
        [Key(9)] public string FullToken { get; set; }
        [Key(10)] public string DisplayName { get; set; }
        [Key(11)] public long DataBits { get; set; }

        [Key(12)] public DataAccumulationSet Accumulations { get; set; } = new DataAccumulationSet();
    }

    [MessagePackObject]
    public class DataAccumulationSet : BaseSmallIdObjectCollection<PersonalDataAccumulation>
    {
        [Key(0)] public PersonalDataAccumulation[] Data { get => _data; set => _data = value; }
        public override long GetAccumulation() { return 0; }

        protected override PersonalDataAccumulation InternalAdd(PersonalDataAccumulation first, PersonalDataAccumulation second)
        {
            throw new NotImplementedException("Cannot add two ChecksumSets together");
        }
    }

    [MessagePackObject]
    public class PersonalDataAccumulation
    {
        [Key(0)] public List<long> SumValues { get; set; } = new List<long>();
    }
}


