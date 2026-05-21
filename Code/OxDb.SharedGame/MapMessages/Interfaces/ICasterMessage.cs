using OxDb.SharedGame.MapMessages.Interfaces;

public interface ICasterMessage : IMapApiMessage
{
    string CasterId { get; set; }
}

