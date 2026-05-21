using MessagePack;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Interfaces;
using OxDb.SharedGame.MapObjects.MapObjectAddons.Entities;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Spells.Interfaces;
using OxDb.SharedGame.Spells.Settings.Effects;
using OxDb.SharedGame.Stats.Messages;
using OxDb.SharedGame.Units.Entities;
using System;
using System.Collections.Generic;

namespace OxDb.SharedGame.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class OnSpawn : BaseMapApiMessage, IMapSpawn
    {
        [Key(0)] public string ObjId { get; set; }
        [Key(1)] public DateTime LastSpawnTime { get; set; }
        [Key(2)] public long EntityTypeId { get; set; }
        [Key(3)] public long EntityId { get; set; }
        [Key(4)] public float X { get; set; }
        [Key(5)] public float Z { get; set; }
        [Key(6)] public long ZoneId { get; set; }
        [Key(7)] public string LocationId { get; set; }
        [Key(8)] public string LocationPlaceId { get; set; }
        [Key(9)] public int SpawnSeconds { get; set; }
        [Key(10)] public string Name { get; set; }
        [Key(11)] public float Y { get; set; }
        [Key(12)] public short Rot { get; set; }
        [Key(13)] public float Speed { get; set; }
        [Key(14)] public long FactionTypeId { get; set; }
        [Key(15)] public bool IsPlayer { get; set; }
        [Key(16)] public string TargetId { get; set; }
        [Key(17)] public int TempFlags { get; set; }
        [Key(18)] public long Level { get; set; }
        [Key(19)] public int OverrideZonePercent { get; set; }
        [Key(20)] public AttackerInfo FirstAttacker { get; set; }
        [Key(21)] public List<RewardList> Loot { get; set; }
        [Key(22)] public List<RewardList> SkillLoot { get; set; }
        [Key(23)] public List<DisplayEffect> Effects { get; set; }
        [Key(24)] public List<FullStat> Stats { get; set; }
        [Key(25)] public long AddonBits { get; set; }
        public long GetAddonBits() { return AddonBits; }

        // Do not send Addons on spawn.
        public List<IMapObjectAddon> GetAddons() { return new List<IMapObjectAddon>(); }

        public OnSpawn()
        {

        }

        public OnSpawn(MapObject wo, ITextSerializer serializer)
        {
            CopyDataFromMapObjectToMapSpawn(wo, serializer);
        }

        private void CopyDataFromMapObjectToMapSpawn(IMapObject obj, ITextSerializer serializer)
        {
            ObjId = obj.Id;

            Name = obj.Name;
            EntityTypeId = obj.EntityTypeId;
            EntityId = obj.EntityId;
            X = obj.X;
            Y = obj.Y;
            Z = obj.Z;
            Rot = (short)obj.Rot;
            Speed = obj.Speed;
            ZoneId = obj.ZoneId;
            LocationId = obj.LocationId;
            LocationPlaceId = obj.LocationPlaceId;
            AddonBits = obj.AddonBits;

            if (obj is Unit unit)
            {
                FactionTypeId = unit.FactionTypeId;
                Stats = unit.Stats.GetSnapshot();
                TargetId = unit.TargetId;
                FirstAttacker = unit.GetFirstAttacker();
                Loot = unit.Loot;
                SkillLoot = unit.SkillLoot;
                TempFlags = unit.GetFlags();

                Effects = new List<DisplayEffect>();
                foreach (IDisplayEffect eff in unit.Effects)
                {
                    Effects.Add(serializer.ConvertType<IDisplayEffect, DisplayEffect>(eff));
                }

                Level = unit.Level;
                if (obj is Character ch)
                {
                    IsPlayer = true;
                    Name = ch.Name;
                    EntityTypeId = EntityTypes.ProxyCharacter;
                }
            }
        }

        public string GetName()
        {
            return Name;
        }

        public bool IsDirty()
        {
            return false;
        }

        public void SetDirty(bool val)
        {

        }
    }
}


