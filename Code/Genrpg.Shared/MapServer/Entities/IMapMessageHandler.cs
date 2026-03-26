
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Threading.Tasks;

namespace Genrpg.Shared.MapServer.Entities
{
    public interface IMapMessageHandler : ISetupDictionaryItem<Type>
    {
        Task Process(IRandom rand, MapMessagePackage package);
    }
}


