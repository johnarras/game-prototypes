using OxDb.Client.Audio.Constants;
using OxDb.SharedCore.Client.Interfaces;
using UnityEngine;

namespace OxDb.Client.Audio.ClientEvents
{
    public class PlaySound : IClientEvent
    {
        public string SoundName { get; set; }
        public float Volume { get; set; } = AudioConstants.MaxVolume;
        public GameObject Parent { get; set; }
        public float VarianceScale { get; set; } = 1.0f;
        public bool Looping { get; set; }


        public PlaySound(string soundName, float varianceScale = AudioConstants.DefaultVariance, GameObject parent = null,
            float volume = AudioConstants.MaxVolume,
            bool looping = false)
        {
            SoundName = soundName;
            Volume = volume;
            Parent = parent;
            VarianceScale = varianceScale;
            Looping = looping;
        }
    }
}
