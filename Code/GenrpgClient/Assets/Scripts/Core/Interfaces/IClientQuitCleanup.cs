using OxDb.SharedCore.Interfaces;

namespace OxDb.Client.Core.Interfaces
{
    public interface IClientQuitCleanup : IInjectable
    {
        void OnQuit();
    }
}


