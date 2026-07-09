using MessagePack;
using Newtonsoft.Json;
using OxDb.SharedCore.Serialization.Attributes;

// The purpose behind this is to make the most frequent messages only use one byte,
// and for less frequent messages we are forced to use at least 2 bytes to send
// them, but like Huffman encoding, on average the total bytes should be smaller
// than if all messages were at the same level and eventually the common ones
// needed 2 bytes to send them.

namespace OxDb.SharedGame.MapMessages.Interfaces
{
    [MessagePackInterface]
    [Union(0 ,typeof(OxDb.SharedGame.Trades.Messages.AcceptTrade))]
    [Union(1 ,typeof(OxDb.SharedGame.Trades.Messages.CancelTrade))]
    [Union(2 ,typeof(OxDb.SharedGame.Trades.Messages.OnAcceptTrade))]
    [Union(3 ,typeof(OxDb.SharedGame.Trades.Messages.OnCancelTrade))]
    [Union(4 ,typeof(OxDb.SharedGame.Trades.Messages.OnStartTrade))]
    [Union(5 ,typeof(OxDb.SharedGame.Trades.Messages.OnUpdateTrade))]
    [Union(6 ,typeof(OxDb.SharedGame.Trades.Messages.StartTrade))]
    [Union(7 ,typeof(OxDb.SharedGame.Trades.Messages.UpdateTrade))]
    [Union(8 ,typeof(OxDb.SharedGame.SpellCrafting.Messages.CraftSpell))]
    [Union(9 ,typeof(OxDb.SharedGame.SpellCrafting.Messages.DeleteSpell))]
    [Union(10 ,typeof(OxDb.SharedGame.SpellCrafting.Messages.OnCraftSpell))]
    [Union(11 ,typeof(OxDb.SharedGame.SpellCrafting.Messages.OnDeleteSpell))]
    [Union(12 ,typeof(OxDb.SharedGame.SpellCrafting.Messages.OnRemoveActionBarItem))]
    [Union(13 ,typeof(OxDb.SharedGame.SpellCrafting.Messages.OnSetActionBarItem))]
    [Union(14 ,typeof(OxDb.SharedGame.SpellCrafting.Messages.RemoveActionBarItem))]
    [Union(15 ,typeof(OxDb.SharedGame.SpellCrafting.Messages.SetActionBarItem))]
    [Union(16 ,typeof(OxDb.SharedGame.Inventory.Messages.BuyItem))]
    [Union(17 ,typeof(OxDb.SharedGame.Inventory.Messages.EquipItem))]
    [Union(18 ,typeof(OxDb.SharedGame.Inventory.Messages.OnAddItem))]
    [Union(19 ,typeof(OxDb.SharedGame.Inventory.Messages.OnEquipItem))]
    [Union(20 ,typeof(OxDb.SharedGame.Inventory.Messages.OnRemoveItem))]
    [Union(21 ,typeof(OxDb.SharedGame.Inventory.Messages.OnUnequipItem))]
    [Union(22 ,typeof(OxDb.SharedGame.Inventory.Messages.OnUpdateItem))]
    [Union(23 ,typeof(OxDb.SharedGame.Inventory.Messages.SellItem))]
    [Union(24 ,typeof(OxDb.SharedGame.Inventory.Messages.UnequipItem))]
    [Union(25 ,typeof(OxDb.SharedGame.Interactions.Messages.CompleteInteract))]
    [Union(26 ,typeof(OxDb.SharedGame.Interactions.Messages.InteractCommand))]
    [Union(27 ,typeof(OxDb.SharedGame.GameSettings.Messages.UpdateGameSettings))]
    [Union(28 ,typeof(OxDb.SharedGame.Ftue.Messages.CompleteFtueStepMessage))]
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


