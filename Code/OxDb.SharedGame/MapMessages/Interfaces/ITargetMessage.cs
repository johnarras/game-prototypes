using OxDb.SharedGame.MapMessages.Interfaces;


public interface ITargetMessage : IMapApiMessage
{
    string TargetId { get; set; }
}


