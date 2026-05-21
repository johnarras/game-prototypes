using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading;

public class SpawnLoadData
{
    public MapObject Obj;
    public OnSpawn Spawn;
    public bool FixedPosition;
    public CancellationToken Token;
}

