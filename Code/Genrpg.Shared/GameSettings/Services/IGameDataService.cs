using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Services
{
    public interface IGameDataService : IInjectable
    {
        Task<IGameData> LoadGameData();
    }
}


