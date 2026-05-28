using OxDb.SharedCore.Interfaces;
using System;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Users.Loaders
{
    public interface ISharedUserDataLoader : ISetupDictionaryItem<Type>, IInitializable
    {
        System.Threading.Tasks.Task CreateDefaultData(string userId);
    }
}


