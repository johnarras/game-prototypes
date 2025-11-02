using Assets.Scripts.Assets.Services;
using Genrpg.Shared.Interfaces;


namespace Assets.Scripts.Interfaces
{
    public interface IAudioService : IInitializable, IAssetSubsystem
    {
        void PlaySound(string name, object parent = null, float volume = 1.0f);
        void PlayMusic(IMusicRegion region);
        void StopAllAudio();

        void SetSoundActive(bool val);
        bool IsSoundActive();

        void SetMusicActive(bool val);
        bool IsMusicActive();
    }
}
