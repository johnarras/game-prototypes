using Genrpg.Shared.Serialization.Attributes;
using MessagePack;
using Newtonsoft.Json;

// The purpose behind this is to make the most frequent messages only use one byte,
// and for less frequent messages we are forced to use at least 2 bytes to send
// them, but like Huffman encoding, on average the total bytes should be smaller
// than if all messages were at the same level and eventually the common ones
// needed 2 bytes to send them.

namespace Genrpg.Shared.MapMessages.Interfaces
{
    [MessagePackInterface]

    [Union(0, typeof(Genrpg.Shared.Trades.Messages.AcceptTrade))]
    [Union(1, typeof(Genrpg.Shared.Trades.Messages.CancelTrade))]
    [Union(2, typeof(Genrpg.Shared.Trades.Messages.OnAcceptTrade))]
    [Union(3, typeof(Genrpg.Shared.Trades.Messages.OnCancelTrade))]
    [Union(4, typeof(Genrpg.Shared.Trades.Messages.OnStartTrade))]
    [Union(5, typeof(Genrpg.Shared.Trades.Messages.OnUpdateTrade))]
    [Union(6, typeof(Genrpg.Shared.Trades.Messages.StartTrade))]
    [Union(7, typeof(Genrpg.Shared.Trades.Messages.UpdateTrade))]
    [Union(8, typeof(Genrpg.Shared.SpellCrafting.Messages.CraftSpell))]
    [Union(9, typeof(Genrpg.Shared.SpellCrafting.Messages.DeleteSpell))]
    [Union(10, typeof(Genrpg.Shared.SpellCrafting.Messages.OnCraftSpell))]
    [Union(11, typeof(Genrpg.Shared.SpellCrafting.Messages.OnDeleteSpell))]
    [Union(12, typeof(Genrpg.Shared.SpellCrafting.Messages.OnRemoveActionBarItem))]
    [Union(13, typeof(Genrpg.Shared.SpellCrafting.Messages.OnSetActionBarItem))]
    [Union(14, typeof(Genrpg.Shared.SpellCrafting.Messages.RemoveActionBarItem))]
    [Union(15, typeof(Genrpg.Shared.SpellCrafting.Messages.SetActionBarItem))]
    [Union(16, typeof(Genrpg.Shared.Inventory.Messages.BuyItem))]
    [Union(17, typeof(Genrpg.Shared.Inventory.Messages.EquipItem))]
    [Union(18, typeof(Genrpg.Shared.Inventory.Messages.OnAddItem))]
    [Union(19, typeof(Genrpg.Shared.Inventory.Messages.OnEquipItem))]
    [Union(20, typeof(Genrpg.Shared.Inventory.Messages.OnRemoveItem))]
    [Union(21, typeof(Genrpg.Shared.Inventory.Messages.OnUnequipItem))]
    [Union(22, typeof(Genrpg.Shared.Inventory.Messages.OnUpdateItem))]
    [Union(23, typeof(Genrpg.Shared.Inventory.Messages.SellItem))]
    [Union(24, typeof(Genrpg.Shared.Inventory.Messages.UnequipItem))]
    [Union(25, typeof(Genrpg.Shared.Interactions.Messages.CompleteInteract))]
    [Union(26, typeof(Genrpg.Shared.Interactions.Messages.InteractCommand))]
    [Union(27, typeof(Genrpg.Shared.GameSettings.Messages.UpdateGameSettings))]
    [Union(28, typeof(Genrpg.Shared.Ftue.Messages.CompleteFtueStepMessage))]
    public interface IInfrequentMapApiMessage : IMapApiMessage
    {

    }

    [MessagePackObject]
    public sealed class InfrequentMessageEnvelope : BaseMapApiMessage
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(0)] public IInfrequentMapApiMessage InfrequentApiMessage { get; set; }
    }


}


