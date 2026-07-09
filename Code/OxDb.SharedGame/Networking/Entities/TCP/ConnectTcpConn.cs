using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Tasks.Services;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Networking.Entities.TCP
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
                            base.InitTcpClient(client);
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


