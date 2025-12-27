using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using MessagePack;

namespace Genrpg.Shared.LoadSave.PlayerData
{
    [MessagePackObject]
    public class SaveSlotData : UniquePersonalUserData
    {
        public const string Filename = "Default";

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long SlotId { get; set; }
    }
}


