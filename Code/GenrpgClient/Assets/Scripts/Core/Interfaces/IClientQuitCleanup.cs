using Genrpg.Shared.Interfaces;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IClientQuitCleanup : IInjectable
    {
        void OnQuit();
    }
}
