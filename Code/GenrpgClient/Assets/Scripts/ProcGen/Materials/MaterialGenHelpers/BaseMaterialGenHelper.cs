using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.GameObjects;
using Assets.Scripts.ProcGen.Materials.Constants;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.ProcGen.Services;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials.MaterialGenHelpers
{
    public abstract class BaseMaterialGenHelper : IMaterialGenHelper
    {
        protected IClientAppService _appService = null;
        protected IClientEntityService _clientEntityService = null;
        protected ILogService _logService = null;
        protected ILineGenService _lineGenService = null;
        protected INoiseService _noiseService = null;
        protected ICrawlerMapService _mapService = null;
        protected IMaterialGenUtilsService _materialGenUtilsService = null;

        public abstract EMaterialGenTypes HelperKey { get; }
        public abstract Awaitable<Texture2D> GenerateTexture(MaterialGenState state);
    }
}
