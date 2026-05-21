namespace OxDb.SharedCore.DataStores.Indexes
{
    public class IndexConfig
    {
        public string MemberName { get; set; }
        public bool Ascending { get; set; } = true;
        public bool Unique { get; set; } = false;
        public bool CompoundContinue { get; set; } = false;
    }
}


