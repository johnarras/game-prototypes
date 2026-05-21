using OxDb.SharedCore.Client.Interfaces;

namespace Assets.Scripts.Audio.ClientEvents
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
