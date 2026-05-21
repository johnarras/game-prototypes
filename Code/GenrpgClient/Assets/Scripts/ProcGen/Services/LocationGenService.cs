using OxDb.SharedCore.Core.Entities;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.ProcGen.Settings.Locations;
using System.Threading;
using System.Threading.Tasks;

public interface ILocationGenService : IInitializable
{
    Location Generate(IGameState gs, int locationId, int zoneId);
}



public class LocationGenService : ILocationGenService
{

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public virtual Location Generate(IGameState gs, int locationId, int zoneId)
    {
        return null;
    }
}


