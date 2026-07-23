using System;
using System.Collections.Generic;

namespace OxDb.Client.Dungeons.Audio
{

    [Serializable]
    public class AmbientSound
    {
        public string SoundName;
        public float MinSecondsBetweenProc = 10;
        public float ProcsPerSecond = 0.1f;
    }


    public class AmbientSoundList : BaseBehaviour
    {
        public List<AmbientSound> Sounds = new List<AmbientSound>();
    }
}
