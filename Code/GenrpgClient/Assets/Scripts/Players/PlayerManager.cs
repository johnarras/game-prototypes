using Assets.Scripts.ClientEvents.DataUpdates;
using Assets.Scripts.Core;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Input;
using Assets.Scripts.Input.Interfaces;
using Assets.Scripts.Setup.Interfaces;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;


public interface IPlayerManager : IInitializable, IMapTokenService, IKeyboardSubsystem
{
    void SetUnit(UnitController unitController);
    bool TryGetUnit(out Unit unit);
    GameObject GetPlayerGameObject();
    string GetUnitId();
    bool Exists();
    void SetCurrentTarget(string targetId);
    void TargetNext();
    void SetKeyPercent(string commandName, float percent);
    void MoveAboveObstacles();
    void SetPlayerObject(GameObject entity);
    string[] MoveInputsToCheck();
}

public class PlayerManager : IPlayerManager
{
    private Unit _unit;
    private UnitController _unitController;
    private GameObject _entity;
    private IClientRandom _rand;
    private CancellationToken _mapToken;

    private IClientMapObjectManager _mapObjectManager = null;
    private IRealtimeNetworkService _networkService = null;
    private IClientEntityService _clientEntityService = null;
    private IInputService _inputService = null;
    private IClientUpdateService _updateService = null;
    private IDispatcher _dispatcher = null;
    private IClientGameState _gs = null;

    public async Task Initialize(CancellationToken token)
    {
        _updateService.AddUpdate(this, OnFrameTick, UpdateTypes.Regular, token);
        _dispatcher.AddListener<OnNewGameData>(OnNewGameDataHandler, token);
        await Task.CompletedTask;
    }

    public void SetPlayerObject(GameObject entity)
    {
        _entity = entity;
    }

    public void SetUnit(UnitController unitController)
    {
        _unitController = unitController;
        if (_unitController != null)
        {
            _entity = _unitController.gameObject;
            _unit = _unitController.GetUnit();
            ClientUnitUtils.UpdateMapPosition(_unitController, _unit);
        }
        else
        {
            _clientEntityService.Destroy(_entity);
            _entity = null;
            _unit = null;
        }
    }

    public void SetMapToken(CancellationToken mapToken)
    {
        _mapToken = mapToken;
    }

    public void SetKeyPercent(string keyName, float percent)
    {
        _unitController?.SetKeyPercent(keyName, percent);
    }


    public bool TryGetUnit(out Unit unit)
    {
        unit = _unit;
        return unit != null;
    }

    public string GetUnitId()
    {
        return _unit?.Id ?? null;
    }

    public GameObject GetPlayerGameObject()
    {
        return _entity;
    }

    public UnitController GetController()
    {
        return _unitController;
    }

    public bool Exists()
    {
        return _unit != null && _unitController != null;
    }

    public void SetCurrentTarget(string unitId)
    {
        if (_unit == null || _unitController == null)
        {
            return;
        }

        _mapObjectManager.GetUnit(unitId, out Unit finalUnit);

        if (finalUnit != null)
        {
            _lastUnitsTabbed.Add(finalUnit);
            _unit.TargetId = finalUnit.Id;
            int sx = (int)(_unitController.entity.transform.position.x);
            int sz = (int)(_unitController.entity.transform.position.z);
            int ex = (int)(finalUnit.X);
            int ez = (int)(finalUnit.Z);
            //WaypointList list = _pathfindingService.CalcPath(_unit, _rand, sx, sz, ex, ez, true);

            //_clientPathfindingUtils.ShowPath(list, _mapToken);
        }
        else
        {
            _unit.TargetId = null;
        }

        _networkService.SendMapMessage(new SetTarget() { TargetId = finalUnit.Id });
    }

    private List<Unit> _lastUnitsTabbed = new List<Unit>();
    public void TargetNext()
    {
        int dist = 20;
        int rad = dist + 10;

        while (_lastUnitsTabbed.Count > 2)
        {
            _lastUnitsTabbed.RemoveAt(0);
        }

        if (_unit != null)
        {
            Vector3 newPos = _unitController.entity.transform.position + _unitController.entity.transform.forward * dist;
            List<Unit> units = _mapObjectManager.GetTypedObjectsNear<Unit>(newPos.x, newPos.z, rad);
            if (units.Count < 1)
            {
                return;
            }

            _lastUnitsTabbed = _lastUnitsTabbed.Where(X => X != null && X != _unit).ToList();

            float minDistToPlayer = 1000000;
            float cx = newPos.x;
            float cz = newPos.z;
            foreach (Unit obj in units)
            {
                if (obj == _unit)
                {
                    continue;
                }
                float dx = obj.X - cx;
                float dz = obj.Z - cz;
                float currDist = (float)Math.Sqrt(dx * dx + dz * dz);
                if (currDist < minDistToPlayer)
                {
                    minDistToPlayer = currDist;
                }
            }


            List<Unit> closeUnits = new List<Unit>();
            foreach (Unit obj in units)
            {

                if (obj == _unit)
                {
                    continue;
                }
                float dx = obj.X - cx;
                float dz = obj.Z - cz;
                float currDist = (float)Math.Sqrt(dx * dx + dz * dz);

                if (currDist < minDistToPlayer + 10)
                {
                    closeUnits.Add(obj);
                }
            }

            List<Unit> nontabbedUnits = new List<Unit>();
            foreach (Unit obj in closeUnits)
            {
                if (!_lastUnitsTabbed.Contains(obj))
                {
                    nontabbedUnits.Add(obj);
                }
            }


            List<Unit> finalUnits = (closeUnits.Count > 0 ? closeUnits : nontabbedUnits);


            if (nontabbedUnits.Count < 1)
            {
                _lastUnitsTabbed.Clear();
            }

            if (finalUnits.Count < 1)
            {
                return;
            }

            finalUnits = finalUnits.Where(x => !x.HasFlag(UnitFlags.IsDead)).ToList();


            if (finalUnits.Count < 1)
            {
                return;
            }

            int unitPos = _rand.Next() % finalUnits.Count;

            Unit finalUnit = finalUnits[unitPos];

            if (finalUnit == _unit)
            {
                return;
            }
            SetCurrentTarget(finalUnit.Id);
        }
    }
    public void MoveAboveObstacles()
    {
        if (_entity == null)
        {
            return;
        }
        RaycastHit[] hits = Physics.RaycastAll(_entity.transform.position + Vector3.up * 500, Vector3.down);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.name.IndexOf("Water") >= 0)
            {
                if (hit.transform.position.y > _entity.transform.position.y)
                {
                    _entity.transform.position = new Vector3(_entity.transform.position.x, hit.transform.position.y + 1,
                        _entity.transform.position.z);
                }
            }
        }
        _entity.transform.position += new Vector3(0, 10, 0);
    }


    private string[] _moveInputsToCheck = new string[]
    {
        KeyComm.Forward,
        KeyComm.Backward,
        KeyComm.TurnLeft,
        KeyComm.TurnRight,
        KeyComm.StrafeLeft,
        KeyComm.StrafeRight,
        KeyComm.Jump
    };

    public string[] MoveInputsToCheck()
    {
        return _moveInputsToCheck;
    }

    public void OnKeyPress(Key key)
    {
        InputContainer cont = _movementInputs.FirstOrDefault(x => x.Key == key);
        if (cont == null)
        {
            return;
        }
        cont.IsPressed = true;
    }

    public void OnKeyRelease(Key key)
    {
        InputContainer cont = _movementInputs.FirstOrDefault(x => x.Key == key);
        if (cont == null)
        {
            return;
        }
        cont.IsPressed = false;
    }

    private void OnFrameTick()
    {

        if (!Exists())
        {
            return;
        }
        if (_inputService.EditingText())
        {
            return;
        }

        UpdateMovementInputs();
    }


    private Dictionary<Key, bool> _pressedKeys = new Dictionary<Key, bool>();
    private void UpdateMovementInputs()
    {
        foreach (InputContainer cont in _movementInputs)
        {
            SetKeyPercent(cont.Command.KeyCommand, cont.IsPressed ? 1 : 0);
        }
        if (_inputService.MouseIsDown(0) && _inputService.MouseIsDown(1))
        {
            SetKeyPercent(KeyComm.Forward, 1.0f);
        }
    }

    private List<InputContainer> _movementInputs = new List<InputContainer>();
    private void OnNewGameDataHandler(OnNewGameData loaded)
    {

        KeyCommData inputData = _gs.ch.Get<KeyCommData>();

        IReadOnlyList<KeyComm> inputList = inputData.GetData();

        _movementInputs.Clear();

        foreach (string moveInput in MoveInputsToCheck())
        {
            KeyComm kc = inputList.FirstOrDefault(x => x.KeyCommand == moveInput);

            if (kc == null || string.IsNullOrWhiteSpace(kc.KeyCommand) || string.IsNullOrEmpty(kc.KeyPress))
            {
                continue;
            }

            InputContainer cont = new InputContainer()
            {
                Command = kc,
                MouseButton = -1,
                Key = _inputService.FromChar(kc.KeyPress[0]),
                IsPressed = false
            };

            _movementInputs.Add(cont);
        }
    }
}

