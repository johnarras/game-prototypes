using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedGame.Tasks.Services;
using System.Net.Sockets;
using System.Threading;

namespace OxDb.SharedGame.Networking.Entities.TCP
{
    public class AcceptTcpConn : BaseTcpConn
    {

        public AcceptTcpConn(TcpClient client,
            MapApiMessageHandler messageHandler,
            ILogService logService,
            ISerializer serializer,
            ITaskService taskService,
            CancellationToken token, ServerConnectionState connState) : base(messageHandler, logService, serializer, taskService, token, connState)
        {
            InitTcpClient(client);
        }
    }
}


