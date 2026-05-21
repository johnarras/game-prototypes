using OxDb.SharedCore.Website.Interfaces;

namespace OxDb.SharedCore.GameSettings.WebApi.UpdateGameSettings
{
    public class UpdateGameSettingsRequest : IClientUserRequest
    {
        public string CharId { get; set; }
    }
}


