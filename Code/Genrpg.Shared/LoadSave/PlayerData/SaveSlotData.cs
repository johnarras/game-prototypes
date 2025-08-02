using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;

namespace Genrpg.Shared.LoadSave.PlayerData
{
    [MessagePackObject]
    public class SaveSlotData : NoChildPlayerData
    {
        public const string Filename = "Default";

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long SlotId { get; set; }
    }
}
