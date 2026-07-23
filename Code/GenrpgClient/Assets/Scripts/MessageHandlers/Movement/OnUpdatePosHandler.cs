using OxDb.Client.Pathfinding.Utils;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Movement.Messages;
using OxDb.SharedGame.Pathfinding.Services;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.ResultHandlers.TypedHandlers
{
    public class OnUpdatePosHandler : BaseClientMapMessageHandler<OnUpdatePos>
    {

        private IPathfindingService _pathfindingService = null;
        private IClientPathfindingUtils _pathfindingUtils;
        private IPlayerManager _playerManager;

        protected override async ValueTask InnerProcess(OnUpdatePos pos, CancellationToken token)
        {
            if (pos.ObjId == _playerManager.GetUnitId())
            {
                return;

            }

            if (_objectManager.GetGridItem(pos.ObjId, out ClientMapObjectGridItem gridItem))
            {
                if (gridItem.Controller != null)
                {
                    gridItem.Controller.LastPosUpdate = DateTime.UtcNow;
                }
            }

            if (_objectManager.GetMapObject(pos.ObjId, out MapObject obj))
            {
                float oldFX = obj.FinalX;
                float oldFZ = obj.FinalZ;
                string oldTarget = obj.TargetId;
                float oldSpeed = obj.Speed;

                float currX = obj.X;
                float currZ = obj.Z;

                obj.FinalX = pos.GetX();
                obj.FinalZ = pos.GetZ();
                obj.Speed = pos.GetSpeed();
                obj.Moving = true;
                obj.TargetId = pos.TargetId;

                double distOffset = MathUtil.LPNorm(2, currX - pos.GetX(), currZ - pos.GetZ());

                if (distOffset > 2 * obj.Speed)
                {
                    obj.Speed *= 2;
                }

                if (obj is Unit unit)
                {

                    if (oldFX != obj.FinalX || oldFZ != obj.FinalZ || oldSpeed != obj.Speed ||
                        oldTarget != obj.TargetId)
                    {
                        if (!string.IsNullOrEmpty(obj.TargetId))
                        {
                            if (_objectManager.GetMapObject(obj.TargetId, out MapObject mapObject))
                            {
                                obj.FinalX = mapObject.X;
                                obj.FinalZ = mapObject.Z;
                            }
                        }


                        _pathfindingService.UpdatePath(unit, (int)obj.FinalX, (int)obj.FinalZ, OnUpdatePath);
                    }

                    if (unit.HasFlag(UnitFlags.ProxyCharacter))
                    {
                        if (_objectManager.GetController(pos.ObjId, out UnitController unitController))
                        {
                            unitController.SetInputValues(pos.GetKeysDown(), pos.GetRot());
                        }
                    }
                }
            }
            await Task.CompletedTask;
        }

        private void OnUpdatePath(Unit unit)
        {
        }
    }
}


