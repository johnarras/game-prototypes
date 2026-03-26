using Genrpg.Shared.MapMessages.Interfaces;


public interface ITargetMessage : IMapApiMessage
{
    string TargetId { get; set; }
}


