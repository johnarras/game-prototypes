using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Tasks.Services;

namespace Genrpg.Shared.Networking.Entities.TCP
{
    public class ConnectTcpConn : BaseTcpConn
    {
        const int MaxConnectTries = 3;
        string _host;
        int _port;

        public ConnectTcpConn(string host, long port,
            MapApiMessageHandler handler, 
            ILogService logService,
            ISerializer serializer,
            ITaskService taskService,
            CancellationToken token, object extraData) : base(handler, logService, serializer, taskService, token, extraData)
        {
            _host = host;
            _port = (int)port;

            _taskService.ForgetTask(ConnectToServer(token), false);
        }

        protected async Task ConnectToServer(CancellationToken token)
        {
            TcpClient client = new TcpClient();
            for (int times = 0; times < MaxConnectTries; times++)
            {
                try
                {
                    using (Task connectTask = client.ConnectAsync(_host, _port))
                    {
                        connectTask.Wait(2000);

                        if (connectTask.IsCompleted && !connectTask.IsCanceled)
                        {
                            InitTcpClient(client);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Shutdown(e, "TcpClient could not connect " + _host + ": " + _port);
                }
            }
            await Task.CompletedTask;
        }
    }
}
