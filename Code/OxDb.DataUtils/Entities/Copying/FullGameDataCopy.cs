using OxDb.SharedCore.GameSettings.Interfaces;

namespace OxDb.DataUtils.Entities.Copying
{
    public class FullGameDataCopy
    {
        public List<IGameSettings> Data { get; set; } = new List<IGameSettings>();
    }
}


