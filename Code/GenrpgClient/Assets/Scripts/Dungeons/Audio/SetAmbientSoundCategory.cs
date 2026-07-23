using OxDb.SharedCore.Client.Interfaces;

namespace OxDb.Client.Dungeons.Audio
{
    public class SetAmbientSoundCategory : IClientEvent
    {
        public string CategoryName;

        public SetAmbientSoundCategory(string categoryName)
        {
            CategoryName = categoryName;
        }
    }
}
