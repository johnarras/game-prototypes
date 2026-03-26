using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.ProcGen.Settings.Names
{

    public class NameValue : IIdName
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
    }


    public class KeyValue
    {
        public string Key { get; set; }
        public string Val { get; set; }
    }



    public class NameIdValue : IIdName
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
        public long Val { get; set; }
    }

}


