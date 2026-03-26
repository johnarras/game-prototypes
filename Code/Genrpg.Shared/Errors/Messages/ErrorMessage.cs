using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Errors.Messages
{
    [MessagePackObject]
    public sealed class ErrorMessage : BaseMapApiMessage
    {
        [Key(0)] public string ErrorText { get; set; }

        public ErrorMessage(string txt)
        {
            ErrorText = txt;
        }
    }
}


