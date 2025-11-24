using Assets.Scripts.Assets.Services;
using Assets.Scripts.Audio.Constants;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Options.Services;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Tokens;
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
public class UnityAudioService : BaseBehaviour, IAudioService, IGameTokenService, IInjectOnLoad<IAudioService>, IInitOnResolve
{

    protected IClientOptionsService _clientOptionsService = null;

    public List<MusicChannel> MusicChannels;

    Dictionary<EAudioCategories, float> _volumes = new Dictionary<EAudioCategories, float>();

    public async Task Initialize(CancellationToken token)
    {
        foreach (EAudioCategories category in Enum.GetValues(typeof(EAudioCategories)))
        {
            _volumes[category] = _clientOptionsService.GetOptions().GetVolume(category);
        }
        await Task.CompletedTask;
    }

    private Dictionary<string, AudioClipList> _audioCache = new Dictionary<string, AudioClipList>();

    protected CancellationToken _token;

    public void SetGameToken(CancellationToken token)
    {
        _token = token;
    }

    public override void Init()
    {
        base.Init();
        AddUpdate(AudioUpdate, UpdateTypes.Regular);
        if (MusicChannels == null)
        {
            MusicChannels = new List<MusicChannel>();
        }
    }

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
        volume = MathUtils.Clamp(AudioConstants.MinVolume, volume, AudioConstants.MaxVolume);
        _clientOptionsService.GetOptions().SetVolume(category, volume);
        _volumes[category] = volume;
        UpdateVolumes();
    }

    public float GetVolume(EAudioCategories category)
    {
        return _volumes[category];
    }

    private void UpdateVolumes()
    {
        foreach (AudioClipList acl in _audioCache.Values)
        {
            acl.UpdateVolume(_volumes);
        }
    }

    public void PlaySound(string soundName, object parent = null, float volume = AudioConstants.MaxVolume)
    {
        if (_volumes[EAudioCategories.Sound] <= AudioConstants.MinVolume)
        {
            return;
        }

        PlayAudioData playData = new PlayAudioData()
        {
            audioName = soundName,
            volume = volume * _volumes[EAudioCategories.Sound],
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

        if (_volumes[playData.category] <= AudioConstants.MinVolume)
        {
            return;
        }

        if (_audioCache.ContainsKey(name))
        {
            AudioClipList cont = _audioCache[name];
            PlayLoadedAudio(cont, playData);
            return;
        }

        _assetService.LoadAsset(AssetCategoryNames.Audio, playData.audioName, OnDownloadAudio, playData, entity, _token);
    }

    private void OnDownloadAudio(object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        AudioClipList cont = go.GetComponent<AudioClipList>();
        if (cont == null || !cont.IsValid())
        {
            _clientEntityService.Destroy(go);
            return;
        }

        PlayAudioData playData = data as PlayAudioData;
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
            MusicChannel categoryCont = GetMusicChannel(playData.category);
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

    protected MusicChannel GetMusicChannel(EAudioCategories cat)
    {
        return MusicChannels.FirstOrDefault(x => x.category == cat);
    }



    /// <summary>
    /// Keep list of default Ids for channels to be read in from a music region.
    /// </summary>
    private Dictionary<EAudioCategories, long> _channelIds = new Dictionary<EAudioCategories, long>();

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
        MusicChannel cont = GetMusicChannel(cat);
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

        MusicChannel catCont = GetMusicChannel(musicCont.playData.category);

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
            float newRandTime = MathUtils.FloatRange(randTime / 2, randTime * 3 / 2, _rand);
            catCont.curr.NextRandomizeTime = DateTime.UtcNow.AddSeconds(newRandTime);
        }
    }

    int fadeFrames = 50;
    List<CurrentMusic> removeList = null;
    private MusicChannel cont = null;
    private CurrentMusic prevMusic = null;
    private void UpdateMusic()
    {
        for (int m = 0; m < MusicChannels.Count; m++)
        {
            cont = MusicChannels[m];
            removeList = null;
            if (cont.curr != null)
            {
                if (_volumes[cont.category] <= AudioConstants.MinVolume)
                {
                    if (removeList == null)
                    {
                        removeList = new List<CurrentMusic>();
                    }
                    removeList.Add(cont.curr);
                }

                FadeSourceTo(cont.curr.source, _volumes[cont.category], fadeFrames);
                FadeSourceTo(cont.curr.prevSource, AudioConstants.MinVolume, fadeFrames);
            }
            for (int mp = 0; mp < cont.prevList.Count; mp++)
            {
                prevMusic = cont.prevList[mp];
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

            if (cont.curr != null &&
                (removeList == null || !removeList.Contains(cont.curr)) &&
                cont.curr.GetRandomIzeSeconds() > 0 &&
                DateTime.UtcNow > cont.curr.NextRandomizeTime)
            {
                cont.ChooseNewRandomSound(_rand);
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

