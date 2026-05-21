using Newtonsoft.Json;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.GameSettings.PlayerData;
using OxDb.SharedCore.Website.Responses.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedCore.GameSettings.WebApi.UpdateGameSettings
{
    public sealed class UpdateGameSettingsResponse : IWebResponse
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<ITopLevelSettings> NewSettings { get; set; } = new List<ITopLevelSettings>();
        public ABList AB { get; set; } = null;
    }
}


