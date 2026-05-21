using OxDb.SharedGame.MapMessages.Interfaces;

// This is just to mark certain messages as commands so we can restrict the API
// client can use.
public interface IPlayerCommand : IMapApiMessage
{

}


