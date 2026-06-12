
using OxDb.SharedCore.Interfaces;
using System;

namespace OxDb.SharedGame.MapServer.Entities
{
    public interface IMapMessageHandler : ISetupDictionaryItem<Type>
    {
        System.Threading.Tasks.Task Process(MapMessagePackage package);
    }
}


