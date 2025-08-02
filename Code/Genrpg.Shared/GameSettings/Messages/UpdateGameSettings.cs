using MessagePack;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.GameSettings.Messages
{
    [MessagePackObject]
    public sealed class UpdateGameSettings : BaseInfrequenMapApiMessage
    {
    }
}
