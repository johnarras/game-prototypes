using OxDb.SharedCore.Interfaces;
using System;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Users.Loaders
{
    public interface ISharedUserDataLoader : ISetupDictionaryItem<Type>, IInitializable
    {
        Task CreateDefaultData(string userId);
    }
}


