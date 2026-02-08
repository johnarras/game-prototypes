
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Setup
{
    public class ServerSetup
    {
        public async Task<GS> SetupFromConfig<GS, TSetupService>(object currentObject, string serverId, CancellationToken token, IServerConfig serverConfigIn,
            string envOverride)
            where GS : ServerGameState
            where TSetupService : SetupService
        {

            try
            {
                if (string.IsNullOrEmpty(serverId))
                {
                    throw new Exception("Missing ServerId in setup code!");
                }

                IServerConfig config = serverConfigIn;

                if (config == null)
                {
                    config = await new ConfigSetup().SetupServerConfig(token, serverId, envOverride);
                }

                GS gs = (GS)Activator.CreateInstance(typeof(GS), new object[] { config });
                TSetupService setupService = (TSetupService)Activator.CreateInstance(typeof(TSetupService));
                await setupService.SetupGame(gs, new List<object> { currentObject }, token);

                return gs;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
    }
}


