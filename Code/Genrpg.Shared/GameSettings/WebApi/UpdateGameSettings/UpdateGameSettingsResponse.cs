using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genrpg.Shared.GameSettings.WebApi.UpdateGameSettings
{
    public sealed class UpdateGameSettingsResponse : IWebResponse
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<ITopLevelSettings> NewSettings { get; set; } = new List<ITopLevelSettings>();
        public GameDataOverrideList DataOverrides { get; set; } = null;
    }
}


