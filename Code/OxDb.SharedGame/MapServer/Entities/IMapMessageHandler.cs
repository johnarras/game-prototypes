
using OxDb.SharedCore.Interfaces;
using System;
using System.Threading.Tasks;

namespace OxDb.SharedGame.MapServer.Entities
{
    public interface IMapMessageHandler : ISetupDictionaryItem<Type>
    {
        Task Process(MapMessagePackage package);
    }
}


