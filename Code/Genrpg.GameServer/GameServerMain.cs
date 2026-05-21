using OxDb.InstanceServer.Setup;
using OxDb.MapServer.MainServer;
using OxDb.MonsterServer.Setup;
using OxDb.PlayerServer.Setup;
using OxDb.ServerCore.MainServer;
using OxDb.ServerCore.Setup;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.Utils;
using System.Diagnostics;

namespace Genrpg.GameServer
{
    /// <summary>
    /// This server exists to allow devs to spin up an entire stack on a custom MessagingEnv
    /// so they can develop locally in a sandbox. Really, the microservices may end up being
    /// stateless, and the map instance servers should all be spun up separately.
    /// </summary>
    public class GameServerMain
    {
        static async Task Main(string[] args)
        {
            DotNetServiceConfiguration.SetupServiceInstances(null, GameComponentNames.GameServer);

            await new GameServer().RunGame();
        }
    }

    public class GameServer
    {
        private List<IBaseServer> _servers = new List<IBaseServer>();
        private CancellationTokenSource _serverTokenSource = new CancellationTokenSource();
        public async Task RunGame()
        {
            try
            {
                InstanceServerMain instanceServer = new InstanceServerMain();
                ServerInitArgs basicArgs = new ServerInitArgs()
                {
                    Token = _serverTokenSource.Token,
                };
                await instanceServer.Init(basicArgs);
                _servers.Add(instanceServer);

                PlayerServerMain playerServer = new PlayerServerMain();
                await playerServer.Init(basicArgs);
                _servers.Add(playerServer);

                MonsterServerMain monsterServer = new MonsterServerMain();
                await monsterServer.Init(basicArgs);
                _servers.Add(monsterServer);

                int serverCount = 2;

                for (int i = 0; i < serverCount; i++)
                {
                    InitMapServerData initServerData = new InitMapServerData()
                    {
                        MapServerCount = serverCount,
                        MapServerIndex = i,
                        MapServerName = HashUtils.NewGuid(),
                        StartPort = 4000 + 100 * i,
                        MapIds = new List<string>(),
                    };

                    MapServerMain mapServer = new MapServerMain();

                    ServerInitArgs mapArgs = new ServerInitArgs()
                    {
                        Token = _serverTokenSource.Token,
                        Data = initServerData,
                    };

                    await mapServer.Init(mapArgs);

                    _servers.Add(mapServer);

                }

                while (true)
                {
                    await Task.Delay(2000, _serverTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("GameServerInitException: " + ex.Message + " " + ex.StackTrace);
            }
        }
    }
}



