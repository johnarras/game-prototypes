namespace OxDb.ServerCore.MainServer
{
    public interface IServer
    {
        Task Init(object data, CancellationToken serverToken);
        Task Run();
    }
}


