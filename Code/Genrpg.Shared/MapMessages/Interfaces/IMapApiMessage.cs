using Genrpg.Shared.Serialization.Attributes;
using MessagePack;
using Newtonsoft.Json;

namespace Genrpg.Shared.MapMessages.Interfaces
{

    [MessagePackInterface]
    [Union(0 ,typeof(Genrpg.Shared.WhoList.Messages.GetWhoList))]
    [Union(1 ,typeof(Genrpg.Shared.WhoList.Messages.OnGetWhoList))]
    [Union(2 ,typeof(Genrpg.Shared.Trades.Messages.OnCompleteTrade))]
    [Union(3 ,typeof(Genrpg.Shared.Targets.Messages.OnSetTarget))]
    [Union(4 ,typeof(Genrpg.Shared.Targets.Messages.OnTargetIsDead))]
    [Union(5 ,typeof(Genrpg.Shared.Targets.Messages.SetTarget))]
    [Union(6 ,typeof(Genrpg.Shared.Stats.Messages.Regen))]
    [Union(7 ,typeof(Genrpg.Shared.Stats.Messages.StatUpd))]
    [Union(8 ,typeof(Genrpg.Shared.Spells.Settings.Effects.ActiveSpellEffect))]
    [Union(9 ,typeof(Genrpg.Shared.Spells.Messages.CastingSpell))]
    [Union(10 ,typeof(Genrpg.Shared.Spells.Messages.CastSpell))]
    [Union(11 ,typeof(Genrpg.Shared.Spells.Messages.CombatText))]
    [Union(12 ,typeof(Genrpg.Shared.Spells.Messages.FX))]
    [Union(13 ,typeof(Genrpg.Shared.Spells.Messages.OnAddEffect))]
    [Union(14 ,typeof(Genrpg.Shared.Spells.Messages.OnRemoveEffect))]
    [Union(15 ,typeof(Genrpg.Shared.Spells.Messages.OnStartCast))]
    [Union(16 ,typeof(Genrpg.Shared.Spells.Messages.OnStopCast))]
    [Union(17 ,typeof(Genrpg.Shared.Spells.Messages.OnUpdateEffect))]
    [Union(18 ,typeof(Genrpg.Shared.RpgLevels.Messages.NewRpgLevel))]
    [Union(19 ,typeof(Genrpg.Shared.Rewards.Messages.OnAddQuantityReward))]
    [Union(20 ,typeof(Genrpg.Shared.Quests.Messages.GetQuests))]
    [Union(21 ,typeof(Genrpg.Shared.Quests.Messages.OnGetQuests))]
    [Union(22 ,typeof(Genrpg.Shared.Players.Messages.AddPlayer))]
    [Union(23 ,typeof(Genrpg.Shared.Players.Messages.OnFinishLoadPlayer))]
    [Union(24 ,typeof(Genrpg.Shared.Players.Messages.SaveDirty))]
    [Union(25 ,typeof(Genrpg.Shared.Pings.Messages.Ping))]
    [Union(26 ,typeof(Genrpg.Shared.Networking.Messages.ConnMessageCounts))]
    [Union(27 ,typeof(Genrpg.Shared.Movement.Messages.OnAddToGrid))]
    [Union(28 ,typeof(Genrpg.Shared.Movement.Messages.OnUpdatePos))]
    [Union(29 ,typeof(Genrpg.Shared.Movement.Messages.UpdatePos))]
    [Union(30 ,typeof(Genrpg.Shared.MapServer.Messages.MapObjectCounts))]
    [Union(31 ,typeof(Genrpg.Shared.MapServer.Messages.ServerMessageCounts))]
    [Union(32 ,typeof(Genrpg.Shared.MapObjects.Messages.DespawnObject))]
    [Union(33 ,typeof(Genrpg.Shared.MapObjects.Messages.GetMapObjectStatus))]
    [Union(34 ,typeof(Genrpg.Shared.MapObjects.Messages.GetSpawnedObject))]
    [Union(35 ,typeof(Genrpg.Shared.MapObjects.Messages.OnGetMapObjectStatus))]
    [Union(36 ,typeof(Genrpg.Shared.MapObjects.Messages.OnSpawn))]
    [Union(37 ,typeof(Genrpg.Shared.MapObjects.Messages.SendSpawn))]
    [Union(38 ,typeof(Genrpg.Shared.MapMessages.Interfaces.InfrequentMessageEnvelope))]
    [Union(39 ,typeof(Genrpg.Shared.Loot.Messages.ClearLoot))]
    [Union(40 ,typeof(Genrpg.Shared.Loot.Messages.LootCorpse))]
    [Union(41 ,typeof(Genrpg.Shared.Loot.Messages.SendRewards))]
    [Union(42 ,typeof(Genrpg.Shared.Loot.Messages.SkillLootCorpse))]
    [Union(43 ,typeof(Genrpg.Shared.Errors.Messages.ErrorMessage))]
    [Union(44 ,typeof(Genrpg.Shared.Combat.Messages.Died))]
    [Union(45 ,typeof(Genrpg.Shared.Combat.Messages.InterruptCast))]
    [Union(46 ,typeof(Genrpg.Shared.Chat.Messages.OnChatMessage))]
    [Union(47 ,typeof(Genrpg.Shared.Chat.Messages.SendChatMessage))]
    [Union(48 ,typeof(Genrpg.Shared.Achievements.Messages.OnUpdateAchievement))]
    public interface IMapApiMessage : IMapMessage
    {
    }



    public class MapApiMessageEnvelope
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public IMapApiMessage ApiMessage { get; set; }
    }
}


