using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Utils;
using System.Net.Sockets;
using System.Threading;

namespace Genrpg.Shared.Networking.Entities.TCP
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
