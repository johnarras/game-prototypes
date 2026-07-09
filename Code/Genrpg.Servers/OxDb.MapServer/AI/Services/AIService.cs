using OxDb.MapServer.AI.Constants;
using OxDb.MapServer.Combat.Messages;
using OxDb.MapServer.MapMessaging.Interfaces;
using OxDb.MapServer.Maps;
using OxDb.MapServer.Units.Services;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.AI.Settings;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Pathfinding.Entities;
using OxDb.SharedGame.Pathfinding.Services;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Spells.PlayerData.Spells;
using OxDb.SharedGame.Targets.Messages;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.MapServer.AI.Services
{
    public interface IAIService : IInjectable
    {
        bool Update(Unit unit);
        void TargetMove(Unit unit, string targetUnitId);
        void EndCombat(Unit unit, string killedUnitId, bool clearAllAttackers);
        void BringFriends(Unit unit, string targetId);
        long GetCastTimes();
        long GetUpdateTimes();
    }

    public class AIService : IAIService
    {

        protected IMapMessageService _messageService = null;
        protected IMapObjectManager _objectManager = null;
        protected IPathfindingService _pathfindingService = null;
        protected IGameData _gameData = null;
        protected ILogService _logService = null;
        protected IServerUnitService _unitService = null;

        public long _updateTimes = 0;
        public long _castTimes = 0;

        public long GetUpdateTimes()
        {
            return _updateTimes;
        }

        public long GetCastTimes()
        {
            return _castTimes;
        }

        public virtual bool Update(Unit unit)
        {
            if (!_unitService.IsOkUnit(unit, false))
            {
                _objectManager.RemoveObject(unit.Rand, unit.Id, UnitConstants.CorpseDespawnSeconds);
                return false;
            }

            unit.LastUpdateTime = DateTime.UtcNow;
            _updateTimes++;
            float ux = unit.X;
            float uz = unit.Z;
            float fx = unit.FinalX;
            float fz = unit.FinalZ;
            float spd = unit.Speed;
            float rot = unit.Rot;

            if (unit.HasFlag(UnitFlags.Evading))
            {
                KeepMoving(unit);
            }
            else if (unit.HasTarget())
            {
                KeepMoving(unit);
                UpdateCombat(unit);
            }
            else
            {
                ScanForTargets(unit);

                if (unit.HasTarget() || unit.Moving)
                {
                    KeepMoving(unit);
                }
                else
                {
                    IdleWander(unit);
                }
            }

            UpdateAfterAIStep(unit);
            return true;
        }

        protected void IdleWander(Unit unit)
        {
            if (!unit.Moving && !unit.HasFlag(UnitFlags.Evading) &&
                !unit.GetAddons().Any() &&
                !unit.HasTarget() &&
                unit.Rand.NextDouble() < _gameData.Get<AISettings>(unit).IdleWanderChance &&
                unit.Spawn != null)
            {
                unit.ClearAttackers(_logService);

                float wanderRange = AIConstants.IdleWanderRange;

                float targetx = unit.Spawn.X + RandUtils.DeltaRange(wanderRange, unit.Rand);
                float targetz = unit.Spawn.Z + RandUtils.DeltaRange(wanderRange, unit.Rand);

                LocationMove(unit, targetx, targetz, RandUtils.FloatRange(0.2f, 0.3f, unit.Rand));
            }
        }

        protected void UpdateCombat(Unit unit)
        {
            if (!_objectManager.GetUnit(unit.TargetId, out Unit target) || target.HasFlag(UnitFlags.IsDead))
            {
                SetTarget(unit, "");
                EndCombat(unit, unit.TargetId, false);
                return;
            }

            SpellData spellData = unit.Get<SpellData>();
            // This does not require an await for monsters
            IReadOnlyList<Spell> spells = spellData.GetData();

            if (spells.Count < 1)
            {
                KeepMoving(unit);
                return;
            }

            _castTimes++;
            Spell spell = spells.FirstOrDefault();

            CastSpell castSpell = new CastSpell()
            {
                SpellId = spell.IdKey,
                TargetId = unit.TargetId,
            };

            _messageService.SendMessage(unit, castSpell);

            KeepMoving(unit);
        }

        protected void ScanForTargets(Unit unit)
        {
            if (unit.HasFlag(UnitFlags.Evading))
            {
                return;
            }

            List<Unit> nearbyUnits = _objectManager.GetTypedObjectsNear<Unit>(unit.X, unit.Z, unit, _gameData.Get<AISettings>(unit).EnemyScanDistance,
                true);

            nearbyUnits = nearbyUnits.Where(x => x.FactionTypeId != unit.FactionTypeId && !x.HasFlag(UnitFlags.IsDead | UnitFlags.Evading)).ToList();

            if (nearbyUnits.Count > 0)
            {
                string newTargetId = nearbyUnits[unit.Rand.Next() % nearbyUnits.Count].Id;
                TargetMove(unit, newTargetId);
                BringFriends(unit, newTargetId); // When it finds a target, it brings friends.
            }
        }

        public void BringFriends(Unit bringer, string targetId)
        {
            if (!_objectManager.GetUnit(targetId, out Unit targetUnit) || targetUnit.HasFlag(UnitFlags.IsDead | UnitFlags.Evading))
            {
                return;
            }

            BringFriends bringAFriend = new BringFriends()
            {
                BringerFactionId = bringer.FactionTypeId,
                BringerId = bringer.Id,
                TargetFactionId = targetUnit.FactionTypeId,
                TargetId = targetUnit.Id,
            };

            _messageService.SendMessageNear(targetUnit, bringAFriend, _gameData.Get<AISettings>(bringer).BringAFriendRadius, false);
        }

        public void LocationMove(Unit unit, float x, float z, float speedMult)
        {
            unit.Speed = unit.BaseSpeed * speedMult;
            unit.Moving = true;
            unit.FinalX = x;
            unit.FinalZ = z;
        }

        public void TargetMove(Unit unit, string targetUnitId)
        {
            if (unit.HasFlag(UnitFlags.Evading))
            {
                return;
            }

            if (!_objectManager.GetUnit(targetUnitId, out Unit targetUnit))
            {
                return;
            }

            float speedMult = 1.0f;
            if (unit.FactionTypeId != targetUnit.FactionTypeId)
            {
                speedMult = UnitConstants.CombatSpeedMult;
            }

            SetTarget(unit, targetUnit.Id);

            float targX = targetUnit.X;
            float targZ = targetUnit.Z;

            float dx = unit.X - targetUnit.X;
            float dz = unit.Z - targetUnit.Z;

            float dist = (float)Math.Sqrt(dx * dx + dz * dz);

            LocationMove(unit, targX, targZ, speedMult);

            StartCombat(unit, targetUnit);
        }
        public void EndCombat(Unit unit, string killedUnitId, bool isLeashing)
        {
            string oldTargetId = unit.TargetId;
            SetTarget(unit, null);
            if (!string.IsNullOrEmpty(killedUnitId))
            {
                unit.RemoveAttacker(killedUnitId);
            }

            if (isLeashing)
            {
                unit.AddFlag(UnitFlags.Evading);
                unit.ClearAttackers(_logService);
                LocationMove(unit, unit.CombatStartX, unit.CombatStartZ, UnitConstants.EvadeSpeedMult);
                return;
            }

            ScanForTargets(unit);
            if (!unit.HasTarget() || unit.TargetId == oldTargetId || unit.TargetId == killedUnitId)
            {
                SetTarget(unit, null);

                if (!(unit is Character ch))
                {
                    unit.AddFlag(UnitFlags.Evading);
                    LocationMove(unit, unit.CombatStartX, unit.CombatStartZ, UnitConstants.EvadeSpeedMult);
                    return;
                }
            }
        }

        public void SetTarget(Unit unit, string targetId)
        {
            if (unit.TargetId == targetId)
            {
                return;
            }

            if (targetId != null)
            {
                if (unit.HasFlag(UnitFlags.IsDead | UnitFlags.Evading))
                {
                    return;
                }
                if (!_objectManager.GetUnit(targetId, out Unit target))
                {
                    return;
                }

                if (target.HasFlag(UnitFlags.IsDead | UnitFlags.Evading))
                {
                    return;
                }
            }

            unit.TargetId = targetId;

            OnSetTarget onSet = unit.GetCachedMessage<OnSetTarget>(true);
            onSet.CasterId = unit.Id;
            onSet.TargetId = targetId;

            _messageService.SendMessageNear(unit, onSet);
        }

        public void StartCombat(Unit attacker, Unit victim)
        {
            if (!attacker.HasFlag(UnitFlags.DidStartCombat))
            {
                attacker.CombatStartX = attacker.X;
                attacker.CombatStartZ = attacker.Z;
                attacker.CombatStartRot = attacker.Rot;
                attacker.AddFlag(UnitFlags.DidStartCombat);
            }
            if (attacker.HasFlag(UnitFlags.Evading))
            {
                attacker.RemoveFlag(UnitFlags.Evading);
            }
        }

        public void KeepMoving(Unit unit)
        {
            if (!unit.Moving || unit.Speed < 0.01f)
            {
                if (!unit.HasTarget())
                {
                    // If it's trying to evade/return home, let it continue trying to move
                    if (unit.HasFlag(UnitFlags.Evading))
                    {
                        // Re-trigger movement toward home if it somehow stopped prematurely
                        LocationMove(unit, unit.CombatStartX, unit.CombatStartZ, UnitConstants.EvadeSpeedMult);
                    }
                    return;
                }
            }
            if (unit.HasTarget() && !unit.HasFlag(UnitFlags.Evading))
            {
                if (_objectManager.GetUnit(unit.TargetId, out Unit target))
                {
                    unit.FinalX = target.X;
                    unit.FinalZ = target.Z;
                }

                float ddx = unit.FinalX - unit.CombatStartX;
                float ddz = unit.FinalZ - unit.CombatStartZ;

                double combatDist = Math.Sqrt(ddx * ddx + ddz * ddz);

                if (combatDist >= _gameData.Get<AISettings>(unit).LeashDistance)
                {
                    EndCombat(unit, "", true);
                    return;
                }
            }

            float finalDx = unit.X - unit.FinalX;
            float finalDz = unit.Z - unit.FinalZ;

            float distToGo = (float)Math.Sqrt(finalDx * finalDx + finalDz * finalDz);

            if (!unit.Moving)
            {
                unit.RemoveFlag(UnitFlags.Evading);
                if (unit.HasTarget() && distToGo > AIConstants.CloseToTargetDistance)
                {
                    TargetMove(unit, unit.TargetId);
                }
                return;
            }

            unit.Speed = Math.Max(unit.Speed, 0.1f);

            float distGone = unit.Speed * _gameData.Get<AISettings>(unit).UpdateSeconds;

            float oldSpeed = unit.Speed;

            float pctMove = distGone / distToGo;
            if (pctMove >= 1.0f || distToGo < AIConstants.CloseToTargetDistance)
            {
                SetUnitAtFinalLocation(unit);
            }
            else
            {
                int nextWpIndex = -1;
                int closestWpIndex = -1;
                float closestWpDist = 10000;
                for (int index = 0; index < unit.Waypoints.Waypoints.Count; index++)
                {
                    Waypoint wp = unit.Waypoints.Waypoints[index];
                    float dx = wp.Z - unit.X;
                    float dz = wp.Z - unit.Z;

                    double distToNext = Math.Sqrt(dx * dx + dz * dz);

                    if (distToNext < closestWpDist)
                    {
                        distToNext = closestWpDist;
                        closestWpIndex = index;
                    }
                    else if (closestWpIndex >= 0) // Found closest index
                    {
                        nextWpIndex = index;
                        break;
                    }
                }

                if (nextWpIndex < 0 || nextWpIndex >= unit.Waypoints.Waypoints.Count - 1)
                {
                    nextWpIndex = unit.Waypoints.Waypoints.Count - 1;
                }

                for (int i = 0; i < nextWpIndex; i++)
                {
                    unit.Waypoints.RemoveWaypointAt(0);
                }

                float nextXPos = unit.GetNextXPos();
                float nextZPos = unit.GetNextZPos();

                float oldX = unit.X;
                float oldZ = unit.Z;

                float nx = unit.X + (nextXPos - unit.X) * pctMove;
                float nz = unit.Z + (nextZPos - unit.Z) * pctMove;
                unit.X = nx;
                unit.Z = nz;

                float finaldx = unit.X - unit.FinalX;
                float finaldz = unit.Z - unit.FinalZ;

                double finalDist = Math.Sqrt(finaldx * finaldx + finaldz * finaldz);

                if (finalDist > distToGo || finalDist < AIConstants.CloseToTargetDistance)
                {
                    SetUnitAtFinalLocation(unit);
                }
            }
        }

        private void SetUnitAtFinalLocation(Unit unit)
        {
            unit.X = unit.FinalX;
            unit.Z = unit.FinalZ;
            unit.Speed = 0;
            unit.Moving = false;
            unit.Waypoints.Clear();
            if (unit.HasFlag(UnitFlags.Evading))
            {
                unit.RemoveFlag(UnitFlags.Evading | UnitFlags.DidStartCombat);
            }
        }

        private void UpdateAfterAIStep(Unit unit)
        {
            _pathfindingService.UpdatePath(unit, (int)unit.FinalX, (int)unit.FinalZ, OnUpdatePath);
        }

        private void OnUpdatePath(Unit unit)
        {
            UnitUtils.TurnTowardNextPosition(unit);
            _objectManager.UpdatePosition(unit, 0);
        }
    }
}


