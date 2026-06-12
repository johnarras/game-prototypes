using OxDb.SharedCore.Interfaces;
using System;

namespace OxDb.SharedGame.Users.Loaders
{
    public interface ISharedUserDataLoader : ISetupDictionaryItem<Type>, IInitializable
    {
        System.Threading.Tasks.Task CreateDefaultData(string userId);
    }
}


