using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Networking.Entities;
using Genrpg.Shared.Networking.Entities.TCP;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.MapServer.Networking.Listeners
{


    public class BaseTcpListener : IListener
    {
        protected TcpListener _server = null;
        protected List<BaseTcpConn> _connections = new List<BaseTcpConn>();

        protected object _connLock = new object();

        protected string _host = null;
        protected int _port = 0;
        protected CancellationToken _token;
        protected ILogService _logService = null;
        protected ISerializer _serializer = null;
        protected ITaskService _taskService = null;
        protected Action<ServerConnectionState> _addConnectionHandler;
        protected MapApiMessageHandler _messageHandler;

        public virtual void Dispose()
        {
            _server.Stop();
            foreach (IConnection conn in _connections)
            {
                conn.ForceClose();
            }
        }

        public BaseTcpListener (string host, int port,
            ILogService logService,
            ISerializer serializer,
            ITaskService taskService,
            Action<ServerConnectionState> addConnection, 
            MapApiMessageHandler receiveMessages,
            CancellationToken token)
        {
            _logService = logService;
            _serializer = serializer;
            _taskService = taskService;
            _addConnectionHandler = addConnection;
            _messageHandler = receiveMessages;
            _port = port;
            _host = host;
            _token = token;
            _taskService.ForgetTask(RunListener(_token), true);
        }

        private async Task AddClient(TcpClient client, CancellationToken token)
        {
            ServerConnectionState connState = new ServerConnectionState();
            IConnection conn = CreateTCPConnection(client, connState, _logService, _serializer, _taskService);
            connState.conn = conn;
            _addConnectionHandler(connState);
            await Task.CompletedTask;
        }

        protected IConnection CreateTCPConnection(TcpClient client, ServerConnectionState connState, ILogService logService, ISerializer serializer, ITaskService taskService)
        {
            return new AcceptTcpConn(client, 
                _messageHandler,
            logService,
            serializer,
            taskService,
            _token,
            connState);
        }

        private async Task RunListener(CancellationToken token)
        {
            bool listenerIsActive = false;
            try
            {
                while (true)
                {
                    if (!listenerIsActive)
                    {
                        _logService.Info("Create listen socket " + _host + " " + _port);
                        IPAddress localAddr = IPAddress.Parse(_host);
                        _server = new TcpListener(localAddr, _port);
                        _server.Start();
                        listenerIsActive = true;
                    }

                    TcpClient client = await _server.AcceptTcpClientAsync(_token);
                    _logService.Info("Accepted client on " + _host + " " + _port);
                    _taskService.ForgetTask(AddClient(client, token), false);
                }
            }
            catch (SocketException e)
            {
                Trace.WriteLine("SocketException: {0}", e.Message);
                _server.Stop();
                listenerIsActive = false;
            }
            catch (OperationCanceledException ce)
            {
                _logService.Info("Shutdown listen socket " + ce.Message);
                _server.Stop();
            }
            catch (Exception e)
            {
                _logService.Exception(e, "BaseTcpListener.Listen");
            }
        }
    }
}

