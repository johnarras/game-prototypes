using UnityEngine.InputSystem;

namespace OxDb.Client.Input.Interfaces
{
    public interface IKeyboardSubsystem
    {
        void OnKeyPress(Key key);
        void OnKeyRelease(Key key);
    }
}


