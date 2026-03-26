using Genrpg.Shared.MapMessages.Interfaces;

public interface ICasterMessage : IMapApiMessage
{
    string CasterId { get; set; }
}

