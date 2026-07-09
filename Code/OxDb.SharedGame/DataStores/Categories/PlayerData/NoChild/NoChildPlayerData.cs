using MessagePack;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Core;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.DataStores.Interfaces;
using OxDb.SharedGame.DataStores.Utils;
using System.Collections.Generic;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild
{



    // Cannot add repo attribute here since the shared and private data split into different areas
    public abstract class NoChildPlayerData : BasePlayerData, ITopLevelUnitData
    {
        public override IUnitData Unpack() { return this; }

        public List<IUnitData> GetChildren() { return new List<IUnitData>(); }
    }


    // Cannot add repo attribute here since the shared and private data split into different areas
    public abstract class VersionedNoChildPlayerData : NoChildPlayerData, IVersionedData
    {
        public override IUnitData Unpack() { return this; }

        public List<IUnitData> GetChildren() { return new List<IUnitData>(); }

        [MessagePack.IgnoreMember] public abstract string VersionTag { get; set; }
    }

    public abstract class PartitionedNoChildPlayerData : NoChildPlayerData, IPartitionedData
    {
        public override IUnitData Unpack() { return this; }

        public List<IUnitData> GetChildren() { return new List<IUnitData>(); }

        [MessagePack.IgnoreMember]
        public string _etag { get; set; }
        private string _pk = null;
        [MessagePack.IgnoreMember]
        public string pk
        {
            get
            {
                if (string.IsNullOrEmpty(_pk))
                {
                    if (!string.IsNullOrEmpty(Id))
                    {
                        string justUserId = Id.Replace(NoSqlUtils.GetDocIdSuffix(GetType()), "");
                        if (!string.IsNullOrWhiteSpace(justUserId))
                        {
                            _pk = justUserId;
                        }
                    }
                }
                return _pk;
            }
        }
    }
}


