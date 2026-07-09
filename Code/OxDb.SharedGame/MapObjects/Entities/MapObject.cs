using Newtonsoft.Json;
using OxDb.SharedCore.DataStores.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings.PlayerData;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedCore.Serialization.Attributes;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Errors.Messages;
using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.MapObjects.Interfaces;
using OxDb.SharedGame.MapObjects.MapObjectAddons.Entities;
using OxDb.SharedGame.Networking.Interfaces;
using OxDb.SharedGame.Pathfinding.Entities;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Spells.Interfaces;
using OxDb.SharedGame.Units.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedGame.MapObjects.Entities
{
    [MessagePackIgnoreType]
    public class MapObject : IMapObject, IDisposable, IUnitDataLookup, IRandomContainer
    {
        public string Id { get; set; }
        public string GetId() { return Id; }
        public string Name { get; set; }
        public IRandom Rand { get; private set; } = new MyRandom();
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Rot { get; set; }
        public float Speed { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string Client { get; set; } = VersionConstants.MinVersion.ToString();
        public Version ClientVersion { get; set; } = VersionConstants.MinVersion;
        public string ClientPlatform { get; set; }

        public long ZoneId { get; set; }
        public string LocationId { get; set; }
        public string LocationPlaceId { get; set; }

        public long PrevZoneId { get; set; }
        public long FactionTypeId { get; set; }
        public long AddonBits { get; set; }

        public DateTime LastGridChange { get; set; }

        public float FinalRot { get; set; }

        public float FinalX { get; set; } = -1;

        public float FinalZ { get; set; } = -1;

        public long Level { get; set; }

        [JsonIgnore] public WaypointList Waypoints { get; private set; } = new WaypointList();

        [JsonIgnore] public List<IDisplayEffect> Effects { get; set; } = new List<IDisplayEffect>();

        public SmallIndexBitList StatusEffects { get; set; } = new SmallIndexBitList();

        public ABList AB { get; set; } = new ABList();

        protected int _idHash { get; set; } = -1;
        public int GetIdHash()
        {
            if (_idHash < 0)
            {
                _idHash = StrUtils.GetPrefixIdHash(Id);
            }
            return _idHash;
        }

        public virtual void SendError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                AddMessage(new ErrorMessage(error));
            }
        }

        public void AddEffect(IDisplayEffect effect)
        {
            if (effect.EntityTypeId == EntityTypes.StatusEffect)
            {
                StatusEffects.SetBitIndex(effect.EntityId);
            }
        }
        public virtual void Dispose()
        {
            _messageCache.Clear();
            OnActionMessage = null;
            Waypoints.Clear();
            Effects.Clear();
        }

        public void RemoveEffect(IDisplayEffect effect)
        {
            if (!Effects.Contains(effect))
            {
                return;
            }

            lock (this)
            {
                Effects.Remove(effect);
            }

            if (effect.EntityTypeId == EntityTypes.StatusEffect)
            {
                if (!Effects.Any(x => x.EntityTypeId == EntityTypes.StatusEffect && x.EntityId == effect.EntityId))
                {
                    StatusEffects.RemoveBitIndex(effect.EntityId);
                }
            }
        }

        public void RemoveStatusBit(long statusBitId)
        {
            StatusEffects.RemoveBitIndex(statusBitId);
            if (Effects.Any(x => x.EntityTypeId == EntityTypes.StatusEffect && x.EntityId == statusBitId))
            {
                lock (this)
                {
                    Effects = Effects.Where(x => x.EntityTypeId != EntityTypes.StatusEffect || x.EntityId != statusBitId).ToList();
                }
            }
        }

        public float GetNextXPos()
        {
            if (Waypoints != null && Waypoints.Waypoints.Count > 0)
            {
                return Waypoints.Waypoints[0].X;
            }
            return FinalX;
        }

        public float GetNextZPos()
        {
            if (Waypoints != null && Waypoints.Waypoints.Count > 0)
            {
                return Waypoints.Waypoints[0].Z;
            }
            return FinalZ;
        }

        public bool Moving { get; set; }

        public string TargetId { get; set; }

        [JsonIgnore] public object OnActionLock { get; set; } = new object();

        public IMapApiMessage OnActionMessage { get; set; }

        public IMapApiMessage ActionMessage { get; set; }

        public IMapSpawn Spawn { get; set; }

        private bool _isDeleted { get; set; }

        public List<UnitRole> Roles { get; set; } = new List<UnitRole>();


        public TAddon GetAddon<TAddon>() where TAddon : IMapObjectAddon
        {
            return (TAddon)Spawn?.GetAddons()?.FirstOrDefault(x => x.GetType() == typeof(TAddon));
        }

        public List<IMapObjectAddon> GetAddons()
        {
            return Spawn?.GetAddons() ?? new List<IMapObjectAddon>();
        }

        public bool HasAddon(long addonTypeId)
        {
            return (AddonBits & (long)(1 << (int)addonTypeId)) != 0;
        }

        public bool HasTarget()
        {
            return !string.IsNullOrEmpty(TargetId);
        }

        public bool IsDeleted()
        {
            return _isDeleted;
        }

        public void SetDeleted(bool val)
        {
            _isDeleted = val;
        }

        protected ConcurrentDictionary<Type, object> _messageCache = new ConcurrentDictionary<Type, object>();
        public virtual T GetCachedMessage<T>(bool unCancel) where T : IMapApiMessage, new()
        {
            if (_messageCache.TryGetValue(typeof(T), out object message))
            {
                T tcurr = (T)message;
                if (unCancel)
                {
                    tcurr.SetCancelled(false);
                }
                return tcurr;
            }
            T t = new T();
            _messageCache.TryAdd(typeof(T), t);
            return t;
        }

        public virtual bool IsPlayer() { return false; }
        public virtual bool IsUnit() { return false; }
        public virtual bool IsGroundObject() { return false; }

        // This exists here, but it is only set up for players for now

        protected IConnection _conn = null;
        public virtual void AddMessage(IMapApiMessage message)
        {
            IConnection conn = _conn;
            if (conn != null)
            {
                conn.AddMessage(message);
            }
        }

        public Point3F GetPos()
        {
            return new Point3F(X, Y, Z);
        }

        public void CopyDataToMapObjectFromMapSpawn(IMapSpawn spawn)
        {
            Id = spawn.ObjId;
            X = spawn.X;
            Z = spawn.Z;
            Rot = spawn.Rot;
            EntityTypeId = spawn.EntityTypeId;
            EntityId = spawn.EntityId;
            ZoneId = spawn.ZoneId;
            LocationId = spawn.LocationId;
            LocationPlaceId = spawn.LocationPlaceId;
            Speed = 0;
            Moving = false;
            FactionTypeId = spawn.FactionTypeId;
            AddonBits = spawn.GetAddonBits();
            if (!string.IsNullOrEmpty(spawn.Name))
            {
                Name = spawn.Name;
            }
        }

        public float DistanceTo(MapObject other)
        {
            float dx = X - other.X;
            float dz = Z - other.Z;

            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        protected Dictionary<Type, IUnitData> _dataDict = new Dictionary<Type, IUnitData>();

        virtual protected bool AlwaysCreateMissingData() { return true; }


        public ValueTask<T> GetAsync<T>() where T : class, IUnitData, new()
        {
            return new ValueTask<T>(Get<T>());
        }

        public ValueTask<IFilteredObject> GetFilteredObject()
        {
            return new ValueTask<IFilteredObject>(this);
        }

        public virtual T Get<T>() where T : class, IUnitData, new()
        {
            Type currType = typeof(T);

            if (_dataDict.ContainsKey(currType))
            {
                return (T)_dataDict[currType];
            }

            if (!IsPlayer() || AlwaysCreateMissingData())
            {
                T t = (T)Activator.CreateInstance(typeof(T));
                t.Id = Id;
                Set(t);
                return t;
            }

            return default;
        }


        public virtual void Set(IUnitData obj)
        {
            if (obj == null)
            {
                return;
            }
            IUnitData obj2 = obj.Unpack();
            _dataDict[obj2.GetType()] = obj2;
        }

        public virtual List<ITopLevelUnitData> GetTopLevelData() { return new List<ITopLevelUnitData>(); }

        public virtual List<IUnitData> GetAllData() { return new List<IUnitData>(); }

        public void AddResponse(IWebResponse response)
        {
        }
    }
}


