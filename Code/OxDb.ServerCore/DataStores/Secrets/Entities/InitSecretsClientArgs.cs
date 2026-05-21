namespace OxDb.ServerCore.DataStores.Secrets.Entities
{
    public class InitSecretsClientArgs
    {
        public string Env { get; set; }
        public string ServerName { get; set; }
        public string VaultPrefix { get; set; }
    }
}
