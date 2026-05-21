namespace OxDb.SharedCore.Utils
{
    public class ReadOnlyString
    {
        private string _val;

        public ReadOnlyString(string val)
        {
            _val = val;
        }

        public string GetString()
        {
            return _val;
        }
    }
}
