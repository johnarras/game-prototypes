using MessagePack;
namespace Genrpg.Shared.Spells.Casting
{
    // MessagePackIgnore  
    public class CastResult
    {
        public string Message = "";

        public void AddMessage(string txt)
        {
            Message += txt + "\n";
        }
    }
}
