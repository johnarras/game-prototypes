using MessagePack;
using Genrpg.Shared.GameSettings.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.GameSettings.WebApi.UpdateGameSettings
{
    public sealed class UpdateGameSettingsResponse : IWebResponse
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<ITopLevelSettings> NewSettings { get; set; } = new List<ITopLevelSettings>();
        public GameDataOverrideList DataOverrides { get; set; } = null;
    }
}


