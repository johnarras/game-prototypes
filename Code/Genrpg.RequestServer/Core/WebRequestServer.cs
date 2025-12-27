
using Genrpg.RequestServer.RequestHandlers;
using Genrpg.RequestServer.Services.AccountAuth;
using Genrpg.RequestServer.Services.GameAuth;
using Genrpg.RequestServer.Services.GameClient;
using Genrpg.RequestServer.Services.NoUsers;
using Genrpg.RequestServer.Setup;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.ServerShared.DataStores;
using Genrpg.ServerShared.MainServer;
using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.Crypto.Entities;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages;
using System.Text;

namespace Genrpg.RequestServer.Core
{
    /// <summary>
    /// This is a minimal amount of webdev used to get us into code that can be used elsewhere easier.
    /// </summary>
    public class WebRequestServer : BaseServer<WebContext, WebsiteSetupService, IQueueMessageHandler>
    {
        protected IGameClientWebService _gameClientWebService { get; private set; }
        protected IAccountAuthWebService _accountAuthWebService { get; private set; }
        protected IGameAuthWebService _gameAuthWebService { get; private set; }
        protected ICryptoService _cryptoService { get; private set; }
        protected ICharmService _charmService { get; private set; }
        protected INoUserWebService _noUserWebService { get; private set; }
        protected IFullRepositoryService _repositoryService { get; private set; }
        protected ITextSerializer _textSerializer { get; private set; }
        protected IBinarySerializer _binarySerializer { get; private set; }
        private CancellationTokenSource _serverSource = new CancellationTokenSource();
        protected CancellationToken _token => _serverSource.Token;

        public WebRequestServer()
        {
            _serverSource = new CancellationTokenSource();

            Init(_serverSource.Token).Wait();
        }

        protected WebContext SetupContext()
        {
            return new WebContext(_config, _context.loc, _repositoryService, _binarySerializer);
        }

        protected string _serverInstanceId = CloudServerNames.Login + HashUtils.NewUUId().ToString().ToLowerInvariant();
        protected override string GetServerId(object data)
        {
            return _serverInstanceId;
        }

        public async Task<string> HandleUserClient(string postData)
        {
            WebContext context = SetupContext();
            await _gameClientWebService.HandleUserClientRequest(context, postData, _token);
            return PackageResponses(context);
        }

        public async Task<string> HandleNoUser(string postData)
        {
            WebContext context = SetupContext();
            await _noUserWebService.HandleNoUserRequest(context, postData, _token);
            return PackageResponses(context);
        }

        public async Task<string> HandleAccountAuth(string postData)
        {
            WebContext context = SetupContext();
            await _accountAuthWebService.HandleAccountAuthRequest(context, postData, _token);
            return PackageResponses(context);
        }


        public async Task<string> HandleGameAuth(string postData)
        {
            WebContext context = SetupContext();
            await _gameAuthWebService.HandleGameAuthRequest(context, postData, _token);
            return PackageResponses(context);
        }

        private string PackageResponses(WebContext context)
        {
            string txt = _textSerializer.SerializeToString(new WebServerResponseSet() { Responses = context.GetResponseList() });

            context.Dispose();

            return txt;
        }

        public async Task<string> HandleTxList(string address)
        {
            MyRandom rand = new MyRandom();
            EthereumTransactionList normalList = await _cryptoService.GetTransactionsFromWallet(address, false);

            EthereumTransactionList internalList = await _cryptoService.GetTransactionsFromWallet(address, true);

            List<EthereumTransaction> allTransactions = new List<EthereumTransaction>(normalList.result);
            allTransactions.AddRange(internalList.result);

            StringBuilder retval = new StringBuilder();
            retval.Append("EXAMPLE CONVERTING TRANSACTIONS INTO STAT BONUSES: NOT FINAL TUNING\n\n");

            foreach (EthereumTransaction trans in allTransactions)
            {
                retval.Append("TX: " + trans.hash + "\n");

                List<PlayerCharmBonusList> list = _charmService.CalcBonuses(trans.hash);

                foreach (PlayerCharmBonusList blist in list)
                {

                    List<string> bonusTexts = _charmService.PrintBonuses(blist);

                    foreach (string btext in bonusTexts)
                    {
                        retval.AppendLine("    " + btext);
                    }
                }
            }

            return retval.ToString();
        }
    }
}


