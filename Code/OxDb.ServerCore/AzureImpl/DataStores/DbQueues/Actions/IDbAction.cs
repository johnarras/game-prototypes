namespace OxDb.ServerCore.AzureImpl.DataStores.DbQueues.Actions
{
    public interface IDbAction
    {
        Task<bool> Execute();
    }
}


