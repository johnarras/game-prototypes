using UnityEngine.InputSystem;

namespace Assets.Scripts.Input.Interfaces
{
    public interface IKeyboardSubsystem
    {
        void OnKeyPress(Key key);
        void OnKeyRelease(Key key);
    }
}
