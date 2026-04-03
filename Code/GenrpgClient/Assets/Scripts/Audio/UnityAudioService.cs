using Assets.Scripts.Assets.Services;
using Assets.Scripts.Audio.Constants;
using Assets.Scripts.Core;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Options.Services;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IAudioService : IInitializable, IAssetSubsystem
{
    void PlaySound(string name, object parent = null, float volume = 1.0f);
    void PlayMusic(IMusicRegion region);
    void StopAllAudio();

    void SetVolume(EAudioCategories category, float volume);
    float GetVolume(EAudioCategories category);
}
public class UnityAudioService : IAudioService
{

    private IClientOptionsService _clientOptionsService = null;
    private IClientUpdateService _updateService = null;
    private IAssetService _assetService = null;
    private IClientEntityService _clientEntityService = null;
    private ISingletonContainer _singletons = null;
    private IClientRandom _rand = null;

    private Dictionary<EAudioCategories, AudioChannel> _channels = new Dictionary<EAudioCategories, AudioChannel>();

    private GameObject _audioParent = null;

    private CancellationToken _token;
    public async Task Initialize(CancellationToken token)
    {
        _token = token;
        _audioParent = _singletons.GetAssetParent<AudioClip>();
        foreach (EAudioCategories category in Enum.GetValues(typeof(EAudioCategories)))
        {
            _channels[category] = new AudioChannel()
            {
                Category = category,
                Volume = _clientOptionsService.GetOptions().GetVolume(category),
                Looping = category == EAudioCategories.Music,
            };
        }

        _updateService.AddUpdate(this, AudioUpdate, UpdateTypes.Regular, _token);
        await Task.CompletedTask;
    }

    private Dictionary<string, AudioClipList> _audioCache = new Dictionary<string, AudioClipList>();

    void AudioUpdate()
    {
        UpdateMusic();
    }

    public void StopAllAudio()
    {
        foreach (AudioClipList cont in _audioCache.Values)
        {
            cont.StopAll();
        }
    }

    public void SetVolume(EAudioCategories category, float volume)
    {
        volume = MathUtil.Clamp(AudioConstants.MinVolume, volume, AudioConstants.MaxVolume);
        _clientOptionsService.GetOptions().SetVolume(category, volume);
        _channels[category].Volume = volume;
        UpdateVolumes();
    }

    public float GetVolume(EAudioCategories category)
    {
        return _channels[category].Volume;
    }

    private void UpdateVolumes()
    {
        foreach (AudioClipList acl in _audioCache.Values)
        {
            acl.UpdateVolume(_channels);
        }
    }

    public void PlaySound(string soundName, object parent = null, float volume = AudioConstants.MaxVolume)
    {
        if (_channels[EAudioCategories.Sound].Volume <= AudioConstants.MinVolume)
        {
            return;
        }

        PlayAudioData playData = new PlayAudioData()
        {
            audioName = soundName,
            volume = volume * _channels[EAudioCategories.Sound].Volume,
            parent = parent as GameObject,
            category = EAudioCategories.Sound,
            looping = false,
        };
        PlayAudio(playData);
    }

    protected void PlayAudio(PlayAudioData playData)
    {
        if (playData == null)
        {
            return;
        }

        if (_channels[playData.category].Volume <= AudioConstants.MinVolume)
        {
            return;
        }

        if (_audioCache.ContainsKey(playData.audioName))
        {
            AudioClipList cont = _audioCache[playData.audioName];
            PlayLoadedAudio(cont, playData);
            return;
        }

        _assetService.LoadAsset(AssetCategoryNames.Audio, playData.audioName, OnDownloadAudio, _audioParent, _token, playData);
    }

    private void OnDownloadAudio(GameObject go, PlayAudioData playData, CancellationToken token)
    {
        AudioClipList cont = go.GetComponent<AudioClipList>();
        if (cont == null || !cont.IsValid())
        {
            _clientEntityService.Destroy(go);
            return;
        }

        if (playData == null || string.IsNullOrEmpty(playData.audioName))
        {
            _clientEntityService.Destroy(go);
            return;
        }

        if (_audioCache.ContainsKey(playData.audioName))
        {
            _clientEntityService.Destroy(go);
            cont = _audioCache[playData.audioName];

        }
        else
        {
            _audioCache[playData.audioName] = cont;
            cont.Name = playData.audioName;
        }
        PlayLoadedAudio(cont, playData);
    }


    protected void PlayLoadedAudio(AudioClipList clipList, PlayAudioData playData)
    {
        if (clipList == null || playData == null)
        {
            return;
        }

        AudioSource source = clipList.Play(playData);

        if (playData.musicData != null)
        {
            AudioChannel categoryCont = GetMusicChannel(playData.category);
            CurrentMusic musicCont = new CurrentMusic()
            {
                playData = playData,
                clipList = clipList,
                source = source,
            };
            clipList.IsActiveMusic = true;

            SetNewMusic(musicCont);
        }
    }

    #region Music

    protected AudioChannel GetMusicChannel(EAudioCategories cat)
    {
        return _channels.Values.FirstOrDefault(x => x.Category == cat);
    }





    public void PlayMusic(IMusicRegion region)
    {
        return;
        //if (MusicChannels == null)
        //{
        //    return;
        //}

        //_channelIds[AudioCategory.Music] = AudioConstants.MaxVolume;
        //_channelIds[AudioCategory.Ambient] = AudioConstants.MinVolume;

        //if (region != null)
        //{
        //    _channelIds[AudioCategory.Music] = region.MusicTypeId;
        //    _channelIds[AudioCategory.Ambient] = region.AmbientMusicTypeId;
        //}

        //for (int i = 0; i < MusicChannels.Count; i++)
        //{
        //    if (i == (int)AudioCategory.Ambient)
        //    {
        //        continue;
        //    }


        //    MusicChannel ch = MusicChannels[i];
        //    long musicId = 0;
        //    if(_channelIds.ContainsKey(ch.category))
        //    {
        //        musicId = _channelIds[ch.category];
        //    }
        //    else
        //    {
        //        continue;
        //    }

        //    MusicType mtype = _gameData.Get<MusicTypeSettings>(_gs.ch)?.Get(musicId);

        //    string musicName = "";
        //    if (mtype != null)
        //    {
        //        musicName = mtype.Art;
        //    }
        //    else
        //    {
        //        musicName = "IntroMusic";
        //    }

        //    if (ch.curr != null && ch.curr.playData != null &&
        //        ch.curr.playData.audioName == musicName)
        //    {
        //        continue;
        //    }

        //    FadeOutMusic(ch.category);

        //    PlayAudioData playData = new PlayAudioData()
        //    {
        //        musicData=mtype,
        //        audioName = musicName,
        //        volume = ch.Volume,
        //        category = ch.category,
        //        looping = ch.Looping,
        //        parent = entity,

        //    };


        //    PlayAudio(playData);
        //}
    }



    private void FadeOutMusic(EAudioCategories cat)
    {
        AudioChannel cont = GetMusicChannel(cat);
        if (cont == null)
        {
            return;
        }

        if (cont.curr != null)
        {
            cont.prevList.Add(cont.curr);
            cont.curr = null;
        }
    }

    private void SetNewMusic(CurrentMusic musicCont)
    {
        if (musicCont == null || musicCont.clipList == null || musicCont.source == null ||
            musicCont.playData == null ||
            string.IsNullOrEmpty(musicCont.playData.audioName))
        {
            return;
        }

        AudioChannel catCont = GetMusicChannel(musicCont.playData.category);

        if (catCont == null)
        {
            return;
        }

        if (catCont.curr != null && catCont.curr.clipList.IsEqual(musicCont.clipList))
        {
            musicCont.clipList.StopSource(musicCont.source);
            return;
        }

        CurrentMusic currentFadingOutMusic = null;

        foreach (CurrentMusic music in catCont.prevList)
        {
            if (music.clipList == musicCont.clipList)
            {
                currentFadingOutMusic = music;
                break;
            }
        }

        CurrentMusic newMusic = null;

        if (currentFadingOutMusic != null)
        {
            musicCont.clipList.StopSource(musicCont.source);
            catCont.prevList.Remove(currentFadingOutMusic);
            newMusic = currentFadingOutMusic;

        }
        else
        {
            newMusic = musicCont;
        }

        if (catCont.curr != null)
        {
            catCont.prevList.Add(catCont.curr);
        }
        catCont.curr = newMusic;
        if (catCont.curr != null && catCont.curr.GetRandomIzeSeconds() > 0)
        {
            float randTime = catCont.curr.GetRandomIzeSeconds();
            float newRandTime = RandUtils.FloatRange(randTime / 2, randTime * 3 / 2, _rand);
            catCont.curr.NextRandomizeTime = DateTime.UtcNow.AddSeconds(newRandTime);
        }
    }

    int fadeFrames = 50;
    List<CurrentMusic> removeList = null;
    private AudioChannel cont = null;
    private CurrentMusic prevMusic = null;
    private void UpdateMusic()
    {
        foreach (AudioChannel channel in _channels.Values)
        {
            removeList = null;
            if (channel.curr != null)
            {
                if (channel.Volume <= AudioConstants.MinVolume)
                {
                    if (removeList == null)
                    {
                        removeList = new List<CurrentMusic>();
                    }
                    removeList.Add(channel.curr);
                }

                FadeSourceTo(channel.curr.source, channel.Volume, fadeFrames);
                FadeSourceTo(channel.curr.prevSource, AudioConstants.MinVolume, fadeFrames);
            }
            for (int mp = 0; mp < channel.prevList.Count; mp++)
            {
                prevMusic = channel.prevList[mp];
                float volume = FadeSourceTo(prevMusic.source, AudioConstants.MinVolume, fadeFrames);
                FadeSourceTo(prevMusic.prevSource, AudioConstants.MinVolume, fadeFrames);
                if (volume <= 0)
                {
                    if (removeList == null)
                    {
                        removeList = new List<CurrentMusic>();
                    }

                    removeList.Add(prevMusic);
                }
            }

            if (channel.curr != null &&
                (removeList == null || !removeList.Contains(channel.curr)) &&
                channel.curr.GetRandomIzeSeconds() > 0 &&
                DateTime.UtcNow > channel.curr.NextRandomizeTime)
            {
                channel.ChooseNewRandomSound(_rand);
            }
        }

        if (removeList != null)
        {
            foreach (CurrentMusic item in removeList)
            {
                item.StopAll();
                cont.prevList.Remove(item);
            }
        }
    }

    private float FadeSourceTo(AudioSource source, float targetVolume, int fadeFrames)
    {
        if (source == null)
        {
            return AudioConstants.MinVolume;
        }

        if (source.volume == targetVolume)
        {
            return source.volume;
        }

        float deltaIncrement = AudioConstants.MaxVolume;
        if (fadeFrames > 0)
        {
            deltaIncrement = AudioConstants.MaxVolume / (fadeFrames + 1);
        }
        if (source.volume > targetVolume)
        {
            source.volume -= deltaIncrement;
            if (source.volume < AudioConstants.MinVolume)
            {
                source.volume = AudioConstants.MinVolume;
            }
        }
        else if (source.volume < targetVolume)
        {
            source.volume += deltaIncrement;
            if (source.volume > targetVolume)
            {
                source.volume = targetVolume;
            }
        }
        return source.volume;
    }

    #endregion

    #region Cleanup


    private List<AudioClipList> _removeList = null;
    public async Awaitable UpdateAssets(CancellationToken token)
    {
        foreach (AudioClipList cont in _audioCache.Values)
        {
            if (cont.CanUnloadAudio())
            {
                if (_removeList == null)
                {
                    _removeList = new List<AudioClipList>();
                }

                _removeList.Add(cont);
            }
        }

        if (_removeList != null && _removeList.Count > 0)
        {
            foreach (AudioClipList cont in _removeList)
            {
                if (cont == null)
                {
                    continue;
                }
                _audioCache.Remove(cont.Name);
                _clientEntityService.Destroy(cont.gameObject);
            }
        }
        else
        {
            _removeList = null;
        }
        await Task.CompletedTask;
    }

    #endregion

}



