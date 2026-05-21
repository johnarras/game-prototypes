using MessagePack;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;

namespace OxDb.SharedGame.LoadSave.PlayerData
{
    [MessagePackObject]
    public class SaveSlotData : NoChildPlayerData
    {
        public const string Filename = "Default";

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long SlotId { get; set; }
    }
}


