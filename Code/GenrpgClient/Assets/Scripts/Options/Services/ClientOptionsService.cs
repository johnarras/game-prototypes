using Assets.Scripts.Audio.Constants;
using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Options.Services
{
    public interface IClientOptionsService : IInitializable, IExplicitInject, IClientQuitCleanup
    {
        LocalClientOptions GetOptions();
        void SaveOptions();
    }

    public class ClientOptionsService : IClientOptionsService
    {
        protected ILogService _logService = null;
        protected IClientAppService _clientAppService = null;
        protected ITextSerializer _textSerializer = null;

        protected LocalClientOptions _options = null;
        protected ClientRepositoryCollection<LocalClientOptions> _repo = null;
        protected string OptionsFileName = "LocalOptions";

        public ClientOptionsService(ILogService logService,
        IClientAppService clientAppService,
        ITextSerializer textSerializer)
        {
            _logService = logService;
            _clientAppService = clientAppService;
            _textSerializer = textSerializer;
        }


        public async Task Initialize(CancellationToken token)
        {
            _repo = new ClientRepositoryCollection<LocalClientOptions>(_logService, _clientAppService, _textSerializer);
            _options = GetOptions();
            await Task.CompletedTask;
        }

        public LocalClientOptions GetOptions()
        {
            if (_options == null)
            {
                _options = _repo.Load(OptionsFileName).GetAwaiter().GetResult();
                if (_options == null)
                {
                    _options = new LocalClientOptions()
                    {
                        Id = OptionsFileName,
                    };

                    foreach (EAudioCategories cat in Enum.GetValues(typeof(EAudioCategories)))
                    {
                        _options.SetVolume(cat, AudioConstants.MaxVolume);
                    }

                    // Do this here rather than in constructor because protobuf will ignore zeroes
                    SaveOptions();
                }
            }
            return _options;
        }

        public void SaveOptions()
        {
            _repo.Save(GetOptions()).Wait();
        }

        public void OnQuit()
        {
            if (!_clientAppService.IsFullScreen())
            {
                _options.ScreenWidth = _clientAppService.ScreenWidth;
                _options.ScreenHeight = _clientAppService.ScreenHeight;
            }
            SaveOptions();
        }
    }
}
