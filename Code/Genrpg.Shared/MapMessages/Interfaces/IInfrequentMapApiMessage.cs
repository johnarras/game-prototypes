using MessagePack;

// The purpose behind this is to make the most frequent messages only use one byte,
// and for less frequent messages we are forced to use at least 2 bytes to send
// them, but like Huffman encoding, on average the total bytes should be smaller
// than if all messages were at the same level and eventually the common ones
// needed 2 bytes to send them.

namespace Genrpg.Shared.MapMessages.Interfaces
{
    [Union(0,typeof(Ftue.Messages.CompleteFtueStepMessage))]
    [Union(1,typeof(GameSettings.Messages.UpdateGameSettings))]
    [Union(2,typeof(Interactions.Messages.CompleteInteract))]
    [Union(3,typeof(Interactions.Messages.InteractCommand))]
    [Union(4,typeof(Inventory.Messages.BuyItem))]
    [Union(5,typeof(Inventory.Messages.EquipItem))]
    [Union(6,typeof(Inventory.Messages.OnAddItem))]
    [Union(7,typeof(Inventory.Messages.OnEquipItem))]
    [Union(8,typeof(Inventory.Messages.OnRemoveItem))]
    [Union(9,typeof(Inventory.Messages.OnUnequipItem))]
    [Union(10,typeof(Inventory.Messages.OnUpdateItem))]
    [Union(11,typeof(Inventory.Messages.SellItem))]
    [Union(12,typeof(Inventory.Messages.UnequipItem))]
    [Union(13,typeof(SpellCrafting.Messages.CraftSpell))]
    [Union(14,typeof(SpellCrafting.Messages.DeleteSpell))]
    [Union(15,typeof(SpellCrafting.Messages.OnCraftSpell))]
    [Union(16,typeof(SpellCrafting.Messages.OnDeleteSpell))]
    [Union(17,typeof(SpellCrafting.Messages.OnRemoveActionBarItem))]
    [Union(18,typeof(SpellCrafting.Messages.OnSetActionBarItem))]
    [Union(19,typeof(SpellCrafting.Messages.RemoveActionBarItem))]
    [Union(20,typeof(SpellCrafting.Messages.SetActionBarItem))]
    [Union(21,typeof(Trades.Messages.AcceptTrade))]
    [Union(22,typeof(Trades.Messages.CancelTrade))]
    [Union(23,typeof(Trades.Messages.OnAcceptTrade))]
    [Union(24,typeof(Trades.Messages.OnCancelTrade))]
    [Union(25,typeof(Trades.Messages.OnStartTrade))]
    [Union(26,typeof(Trades.Messages.OnUpdateTrade))]
    [Union(27,typeof(Trades.Messages.StartTrade))]
    [Union(28,typeof(Trades.Messages.UpdateTrade))]
    public interface IInfrequentMapApiMessage : IMapApiMessage
    {

    }

    [MessagePackObject]
    public sealed class InfrequentMessageEnvelope : BaseMapApiMessage
    {
        [Key(0)] public IInfrequentMapApiMessage InfrequentApiMessage { get; set; }
    }

     
}
