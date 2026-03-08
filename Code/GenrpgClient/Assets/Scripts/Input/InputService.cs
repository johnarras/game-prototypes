using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Input.Interfaces;
using Assets.Scripts.UI.ClientEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

internal class CustomInputAction
{
    public Key Key;
    public InputAction Action;
    public Action<InputAction.CallbackContext> PerformCallback;
    public Action<InputAction.CallbackContext> CancelCallback;

    public bool IsPressed()
    {
        if (Action != null)
        {
            return Action.phase == InputActionPhase.Performed;
        }
        return false;
    }
}

public interface IInputService : IInitializable, IClientResetCleanup
{
    bool MouseClickNow(int index);
    float GetDeltaTime();
    bool MouseIsDown(int mouseIndex);
    Vector3 MousePosition();
    bool ModifierIsActive(string keyCommand);
    void SetDisabled(bool isDisabled);
    bool WasPressedThisFrame(char key, bool dispatchClickKeys = false);
    bool WasPressedThisFrame(Key key, bool dispatchClickKeys = false);
    bool IsPressed(char key);
    bool IsPressed(Key k);
    bool ContinueKeyIsDown();
    float GetAxis(string mouseAxisName);
    Key FromChar(char c);
    bool EditingText();
}

public class InputService : IInputService
{

    private ICameraController _cameraController = null;
    private IPlayerManager _playerManager = null;
    private IMapGenData _md = null;
    private IClientUpdateService _updateService = null;
    private IDispatcher _dispatcher = null;
    private IClientGameState _gs = null;
    private IClientEntityService _clientEntityService = null;

    List<IKeyboardSubsystem> _keyboardSystems = new List<IKeyboardSubsystem>();

    private bool _didSetupInputMap = false;

    public async Task Initialize(CancellationToken token)
    {
        _updateService.AddUpdate(this, InputUpdate, UpdateTypes.Regular, token);
        _keyboardSystems = _gs.loc.GetVals<IKeyboardSubsystem>();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Pressed this frame
    /// </summary>
    /// <param name="c"></param>
    /// <param name="dispatchClickKeys"></param>
    /// <returns></returns>
    public bool WasPressedThisFrame(char c, bool dispatchClickKeys = false)
    {
        return WasPressedThisFrame(Keyboard.current.FindKeyOnCurrentKeyboardLayout(c.ToString())?.keyCode ?? Key.None, dispatchClickKeys);
    }

    public bool WasPressedThisFrame(Key k, bool dispatchClickKeys = false)
    {
        if (k == Key.None)
        {
            return false;
        }
        try
        {

            bool wasPressed = Keyboard.current[k]?.wasPressedThisFrame ?? false;

            if (wasPressed && dispatchClickKeys)
            {
                _dispatcher.Dispatch(new ClickKey() { Key = k });
            }
            return wasPressed;
        }
        catch (Exception ee)
        {
            Debug.Log("EXC: " + ee.Message + " " + k);
        }
        return false;
    }

    /// <summary>
    /// Is down in general
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool IsPressed(Key c)
    {
        if (c == Key.None)
        {
            return false;
        }
        return Keyboard.current[c]?.isPressed ?? false;
    }

    public bool IsPressed(char c)
    {
        return IsPressed(Keyboard.current.FindKeyOnCurrentKeyboardLayout(c.ToString())?.keyCode ?? Key.None);
    }

    private bool _isDisabled = false;
    public void SetDisabled(bool isDisabled)
    {
        _isDisabled = isDisabled;
    }


    public float GetDeltaTime() { return Time.deltaTime; }


    public bool MouseClickNow(int index)
    {
        if (index < 0 || index > 2)
        {
            return false;
        }

        return Mouse.current.IsPressed(index);
    }

    public bool MouseIsDown(int index)
    {
        if (index < 0 || index > 2)
        {
            return false;
        }

        if (index == 0)
        {
            return Mouse.current.leftButton.isPressed;
        }
        else if (index == 1)
        {
            return Mouse.current.rightButton.isPressed;
        }
        {
            return false;
        }
    }

    public Vector3 MousePosition()
    {
        Vector2 pos = Mouse.current.position.ReadValue();
        return new Vector3(pos.x, pos.y, 0);
    }

    public bool ModifierIsActive(string keyCommand)
    {
        if (keyCommand == KeyComm.ShiftName)
        {
            return IsPressed(Key.LeftShift) || IsPressed(Key.RightShift);
        }
        else if (keyCommand == KeyComm.CtrlName)
        {
            return IsPressed(Key.LeftCtrl) || IsPressed(Key.RightCtrl);
        }
        else if (keyCommand == KeyComm.AltName)
        {
            return IsPressed(Key.LeftAlt) || IsPressed(Key.RightAlt);
        }
        return false;
    }

    int mouseLayerMask = 0;

    private List<CustomInputAction> customActions = new List<CustomInputAction>();

    private void SetupInputMap()
    {
        if (_didSetupInputMap)
        {
            return;
        }

        foreach (Key key in Enum.GetValues(typeof(Key)))
        {

            string keyName = Enum.GetName(typeof(Key), key);
            CustomInputAction customAction = new CustomInputAction()
            {
                Key = key,
            };
            InputAction ia = new InputAction(keyName, type: InputActionType.Button);
            customAction.Action = ia;
            if (keyName.IndexOf("Digit") == 0)
            {
                keyName = keyName.Replace("Digit", "");
            }
            ia.AddBinding("<Keyboard>/" + keyName);
            customActions.Add(customAction);

            Action<InputAction.CallbackContext> performAction = (InputAction.CallbackContext context) =>
            {
                OnKeyPress(customAction.Key);
            };

            Action<InputAction.CallbackContext> cancelAction = (InputAction.CallbackContext context) =>
            {
                OnKeyRelease(customAction.Key);
            };

            ia.performed += performAction;
            ia.canceled += cancelAction;
            customAction.PerformCallback = performAction;
            customAction.CancelCallback = cancelAction;
            ia.Enable();

        }

        _didSetupInputMap = true;
    }


    private void OnKeyPress(Key key)
    {
        foreach (IKeyboardSubsystem subsystem in _keyboardSystems)
        {
            subsystem.OnKeyPress(key);
        }
    }

    private void OnKeyRelease(Key key)
    {
        foreach (IKeyboardSubsystem subsystem in _keyboardSystems)
        {
            subsystem.OnKeyRelease(key);
        }
    }

    private void InputUpdate()
    {
        if (_isDisabled)
        {
            return;
        }

        SetupInputMap();

        if (!WasPressedThisFrame(Key.Escape))
        {
            if (_md.GeneratingMap)
            {
                return;
            }
        }

        GetMapMouseHit();
    }

    RaycastHit hit;
    Ray ray;
    GameObject hitObject = null;
    InteractableObject interactObject = null;
    bool didHitObject = false;
    Camera mainCam = null;
    float hitObjectDistance = 0;
    GameObject playerObject = null;
    float errorDistance = 1000000;
    private void GetMapMouseHit()
    {
        if (mainCam == null)
        {
            if (_cameraController == null)
            {
                return;
            }
            mainCam = _cameraController.GetMainCamera();
        }

        if (mouseLayerMask == 0)
        {
            mouseLayerMask = LayerUtils.GetMask(new string[] { LayerNames.Default, LayerNames.ObjectLayer, LayerNames.UnitLayer });
        }

        if (playerObject == null)
        {
            playerObject = _playerManager.GetPlayerGameObject();
        }

        ray = mainCam.ScreenPointToRay(MousePosition());

        didHitObject = Physics.Raycast(ray, out hit, MapConstants.MaxMouseRaycastDistance, mouseLayerMask);

        if (didHitObject && hit.transform != null)
        {

            hitObject = hit.transform.gameObject;

            if (playerObject != null)
            {
                hitObjectDistance = Vector3.Distance(hit.transform.position, playerObject.transform.position);
            }
            else
            {
                hitObjectDistance = errorDistance;
            }

            // This causes garbage in the editor, but not in the built game. 0.6k per frame.
            InteractableObject newInteractObject = hitObject.GetComponent<InteractableObject>();

            // Add this for cases where the collider is nested in the prefab and 
            // the interactable object component is added to the root object.
            if (newInteractObject == null && hitObject.transform.parent != null)
            {
                newInteractObject = _clientEntityService.FindInParents<InteractableObject>(hitObject);
            }


            if (interactObject != null && newInteractObject != interactObject)
            {
                interactObject.MouseExit();
            }
            if (newInteractObject != null && newInteractObject != interactObject)
            {
                newInteractObject.MouseEnter();
            }

            if (newInteractObject != null)
            {
                if (MouseClickNow(0))
                {
                    newInteractObject.LeftMouseClick(hitObjectDistance);
                }
                else if (MouseClickNow(1))
                {
                    newInteractObject.RightMouseClick(hitObjectDistance);
                }
            }


            interactObject = newInteractObject;
        }
        else
        {
            hitObject = null;
            hitObjectDistance = errorDistance;
            if (interactObject != null)
            {
                interactObject.MouseExit();
            }
        }
    }

    public bool ContinueKeyIsDown()
    {
        return WasPressedThisFrame(Key.Escape) || WasPressedThisFrame(Key.Space) || WasPressedThisFrame(Key.Enter);
    }

    public float GetAxis(string mouseAxisName)
    {
        return Mouse.current.scroll.ReadValue().y;
    }

    public Key FromChar(char c)
    {
        return Keyboard.current.FindKeyOnCurrentKeyboardLayout(c.ToString())?.keyCode ?? Key.None;
    }

    public async Task OnReset(CancellationToken token)
    {
        if (customActions != null)
        {
            foreach (CustomInputAction action in customActions)
            {
                action.Action.performed -= action.PerformCallback;
                action.Action.canceled -= action.CancelCallback;
            }
        }
        await Task.CompletedTask;
    }

    public bool EditingText()
    {
        return _clientEntityService.GetComponent<GInputField>(EventSystem.current.currentSelectedGameObject) != null;
    }
}

