namespace OxDb.SharedCore.DataStores.Entities
{
    public class RepoSaveArgs
    {
        public bool Verbose { get; set; }
        public object Args { get; set; }
        public string OverrideId { get; set; }
        public bool Encrypt { get; set; }
    }
}
