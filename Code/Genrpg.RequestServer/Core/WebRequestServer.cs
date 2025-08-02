
using Genrpg.Shared.Utils;
using Genrpg.ServerShared.MainServer;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.Shared.Crypto.Entities;
using System.Text;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.RequestServer.Setup;
using Genrpg.RequestServer.Services.NoUsers;
using Genrpg.Shared.Website.Messages;
using Genrpg.RequestServer.RequestHandlers;
using System.Formats.Tar;
using Genrpg.RequestServer.Services.GameAuth;
using Genrpg.RequestServer.Services.GameClient;
using Genrpg.RequestServer.Services.AccountAuth;

namespace Genrpg.RequestServer.Core
{
    /// <summary>
    /// This is a minimal amount of webdev used to get us into code that can be used elsewhere easier.
    /// </summary>
    public class WebRequestServer : BaseServer<WebContext, WebsiteSetupService, IWebRequestHandler>
    {
        protected IGameClientWebService _gameClientWebService { get; private set; }
        protected IAccountAuthWebService _accountAuthWebService { get; private set; }
        protected IGameAuthWebService _gameAuthWebService { get; private set; }
        protected ICryptoService _cryptoService { get; private set; }
        protected ICharmService _charmService { get; private set; }
        protected INoUserWebService _noUserWebService { get; private set; }
        protected IRepositoryService _repositoryService { get; private set; }
        protected ITextSerializer _serializer { get; private set; } 
        private CancellationTokenSource _serverSource = new CancellationTokenSource();
        protected CancellationToken _token => _serverSource.Token;

        public WebRequestServer()
        {
            _serverSource = new CancellationTokenSource();

            Init(null, null, _serverSource.Token).Wait();
            _gameClientWebService = _context.loc.Get<IGameClientWebService>();
            _accountAuthWebService = _context.loc.Get<IAccountAuthWebService>();
            _gameAuthWebService = _context.loc.Get<IGameAuthWebService>();
            _cryptoService = _context.loc.Get<ICryptoService>();
            _charmService = _context.loc.Get<ICharmService>();
            _noUserWebService = _context.loc.Get<INoUserWebService>();
            _serializer = _context.loc.Get<ITextSerializer>();
        }

        protected WebContext SetupContext()
        {
            return new WebContext(_config, _context.loc);
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
            return _serializer.SerializeToString(new WebServerResponseSet() { Responses = context.Responses.GetResponses() });
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
