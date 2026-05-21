using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Errors.Messages
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


