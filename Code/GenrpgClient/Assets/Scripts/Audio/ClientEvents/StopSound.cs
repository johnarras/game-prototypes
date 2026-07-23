using OxDb.SharedCore.Client.Interfaces;

namespace OxDb.Client.Audio.ClientEvents
{
    public class StopSound : IClientEvent
    {
        public string SoundName { get; set; }

        public StopSound(string soundName)
        {
            SoundName = soundName;
        }
    }
}
