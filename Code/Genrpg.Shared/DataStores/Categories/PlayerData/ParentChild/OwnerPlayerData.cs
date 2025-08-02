using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild
{
    public abstract class OwnerPlayerData : BasePlayerData, IStringOwnerId, IChildUnitData
    { 
        [MessagePack.IgnoreMember]
        public abstract string OwnerId { get; set; }

        public override IUnitData Unpack() { return this; }
    }
}
