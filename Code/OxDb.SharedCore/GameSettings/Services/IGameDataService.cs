using OxDb.SharedCore.Interfaces;
using System.Threading.Tasks;

namespace OxDb.SharedCore.GameSettings.Services
{
    public interface IGameDataService : IInjectable
    {
        Task<IGameData> LoadGameData();
    }
}


