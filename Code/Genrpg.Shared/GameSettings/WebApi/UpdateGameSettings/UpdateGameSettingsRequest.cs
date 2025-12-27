using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.GameSettings.WebApi.UpdateGameSettings
{
    public class UpdateGameSettingsRequest : IClientUserRequest
    {
        public string CharId { get; set; }
    }
}


