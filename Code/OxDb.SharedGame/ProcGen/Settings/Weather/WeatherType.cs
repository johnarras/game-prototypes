using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils.Data;

namespace OxDb.SharedGame.ProcGen.Settings.Weather
{
    public class WeatherType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public MyColorF AmbientColor { get; set; }
        public MyColorF LightColor { get; set; }
        public float LightScale { get; set; }
        public MyColorF SkyColor { get; set; }
        public float FogScale { get; set; }
        public MyColorF FogColor { get; set; }
        public float FogDistance { get; set; }
        public float CloudScale { get; set; }
        public float CloudSpeed { get; set; }
        public MyColorF CloudColor { get; set; }

        public float WindScale { get; set; }

        public float PrecipScale { get; set; }
        public bool IsCold { get; set; }

        public float ParticleScale { get; set; }
        public string Particles { get; set; }

        public WeatherType()
        {
            AmbientColor = new MyColorF();
            LightColor = new MyColorF();
            LightScale = 1.0f;
            SkyColor = new MyColorF();
            FogScale = 0f;
            FogColor = new MyColorF();
            CloudScale = 0f;
            CloudSpeed = 0f;
            WindScale = 1.0f;
            PrecipScale = 1.0f;
            Particles = "";
        }

    }
}


