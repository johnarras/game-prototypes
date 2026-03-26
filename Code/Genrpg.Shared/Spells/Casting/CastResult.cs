namespace Genrpg.Shared.Spells.Casting
{
    public class CastResult
    {
        public string Message = "";

        public void AddMessage(string txt)
        {
            Message += txt + "\n";
        }
    }
}


