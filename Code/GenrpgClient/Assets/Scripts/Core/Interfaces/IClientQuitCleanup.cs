using OxDb.SharedCore.Interfaces;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IClientQuitCleanup : IInjectable
    {
        void OnQuit();
    }
}


