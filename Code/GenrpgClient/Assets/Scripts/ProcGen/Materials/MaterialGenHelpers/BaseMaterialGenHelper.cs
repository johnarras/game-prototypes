using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.Client.GameObjects;
using OxDb.Client.ProcGen.Materials.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.ProcGen.Services;
using UnityEngine;

namespace OxDb.Client.ProcGen.Materials.MaterialGenHelpers
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


        protected Texture2D CreateTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBAFloat, true, false, true);
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }
    }
}
