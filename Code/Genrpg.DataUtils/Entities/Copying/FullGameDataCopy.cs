using Genrpg.Shared.GameSettings.Interfaces;

namespace Genrpg.DataUtils.Entities.Copying
{
    public class FullGameDataCopy
    {
        public List<IGameSettings> Data { get; set; } = new List<IGameSettings>();
    }
}


