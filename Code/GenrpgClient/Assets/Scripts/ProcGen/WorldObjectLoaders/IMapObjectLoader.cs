
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading;
using UnityEngine;

public interface IMapObjectLoader : ISetupDictionaryItem<long>
{
    Awaitable Load(OnSpawn message, MapObject loadedObject, CancellationToken token);
}

