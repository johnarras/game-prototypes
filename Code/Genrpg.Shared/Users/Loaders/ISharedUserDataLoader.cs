using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Users.Loaders
{
    public interface ISharedUserDataLoader : ISetupDictionaryItem<Type>, IInitializable
    {
        Task CreateDefaultData(string userId);
    }
}
