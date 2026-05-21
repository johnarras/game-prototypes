using MessagePack;
using Newtonsoft.Json;
using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedCore.Serialization.Attributes;

namespace OxDb.SharedGame.MapMessages.Interfaces
{

    [MessagePackInterface]
    [Union(0, typeof(OxDb.SharedGame.WhoList.Messages.GetWhoList))]
    [Union(1, typeof(OxDb.SharedGame.WhoList.Messages.OnGetWhoList))]
    [Union(2, typeof(OxDb.SharedGame.Trades.Messages.OnCompleteTrade))]
    [Union(3, typeof(OxDb.SharedGame.Targets.Messages.OnSetTarget))]
    [Union(4, typeof(OxDb.SharedGame.Targets.Messages.OnTargetIsDead))]
    [Union(5, typeof(OxDb.SharedGame.Targets.Messages.SetTarget))]
    [Union(6, typeof(OxDb.SharedGame.Stats.Messages.Regen))]
    [Union(7, typeof(OxDb.SharedGame.Stats.Messages.StatUpd))]
    [Union(8, typeof(OxDb.SharedGame.Spells.Settings.Effects.ActiveSpellEffect))]
    [Union(9, typeof(OxDb.SharedGame.Spells.Messages.CastingSpell))]
    [Union(10, typeof(OxDb.SharedGame.Spells.Messages.CastSpell))]
    [Union(11, typeof(OxDb.SharedGame.Spells.Messages.CombatText))]
    [Union(12, typeof(OxDb.SharedGame.Spells.Messages.FX))]
    [Union(13, typeof(OxDb.SharedGame.Spells.Messages.OnAddEffect))]
    [Union(14, typeof(OxDb.SharedGame.Spells.Messages.OnRemoveEffect))]
    [Union(15, typeof(OxDb.SharedGame.Spells.Messages.OnStartCast))]
    [Union(16, typeof(OxDb.SharedGame.Spells.Messages.OnStopCast))]
    [Union(17, typeof(OxDb.SharedGame.Spells.Messages.OnUpdateEffect))]
    [Union(18, typeof(OxDb.SharedGame.RpgLevels.Messages.NewRpgLevel))]
    [Union(19, typeof(OxDb.SharedGame.Rewards.Messages.OnAddQuantityReward))]
    [Union(20, typeof(OxDb.SharedGame.Quests.Messages.GetQuests))]
    [Union(21, typeof(OxDb.SharedGame.Quests.Messages.OnGetQuests))]
    [Union(22, typeof(OxDb.SharedGame.Players.Messages.AddPlayer))]
    [Union(23, typeof(OxDb.SharedGame.Players.Messages.OnFinishLoadPlayer))]
    [Union(24, typeof(OxDb.SharedGame.Players.Messages.SaveDirty))]
    [Union(25, typeof(OxDb.SharedGame.Pings.Messages.Ping))]
    [Union(26, typeof(OxDb.SharedGame.Networking.Messages.ConnMessageCounts))]
    [Union(27, typeof(OxDb.SharedGame.Movement.Messages.OnAddToGrid))]
    [Union(28, typeof(OxDb.SharedGame.Movement.Messages.OnUpdatePos))]
    [Union(29, typeof(OxDb.SharedGame.Movement.Messages.UpdatePos))]
    [Union(30, typeof(OxDb.SharedGame.MapServer.Messages.MapObjectCounts))]
    [Union(31, typeof(OxDb.SharedGame.MapServer.Messages.ServerMessageCounts))]
    [Union(32, typeof(OxDb.SharedGame.MapObjects.Messages.DespawnObject))]
    [Union(33, typeof(OxDb.SharedGame.MapObjects.Messages.GetMapObjectStatus))]
    [Union(34, typeof(OxDb.SharedGame.MapObjects.Messages.GetSpawnedObject))]
    [Union(35, typeof(OxDb.SharedGame.MapObjects.Messages.OnGetMapObjectStatus))]
    [Union(36, typeof(OxDb.SharedGame.MapObjects.Messages.OnSpawn))]
    [Union(37, typeof(OxDb.SharedGame.MapObjects.Messages.SendSpawn))]
    [Union(38, typeof(OxDb.SharedGame.MapMessages.Interfaces.InfrequentMessageEnvelope))]
    [Union(39, typeof(OxDb.SharedGame.Loot.Messages.ClearLoot))]
    [Union(40, typeof(OxDb.SharedGame.Loot.Messages.LootCorpse))]
    [Union(41, typeof(OxDb.SharedGame.Loot.Messages.SendRewards))]
    [Union(42, typeof(OxDb.SharedGame.Loot.Messages.SkillLootCorpse))]
    [Union(43, typeof(OxDb.SharedGame.Errors.Messages.ErrorMessage))]
    [Union(44, typeof(OxDb.SharedGame.Combat.Messages.Died))]
    [Union(45, typeof(OxDb.SharedGame.Combat.Messages.InterruptCast))]
    [Union(46, typeof(OxDb.SharedGame.Chat.Messages.OnChatMessage))]
    [Union(47, typeof(OxDb.SharedGame.Chat.Messages.SendChatMessage))]
    [Union(48, typeof(OxDb.SharedGame.Achievements.Messages.OnUpdateAchievement))]
    public interface IMapApiMessage : IMapMessage, IClientEvent
    {
    }



    public class MapApiMessageEnvelope
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public IMapApiMessage ApiMessage { get; set; }
    }
}


