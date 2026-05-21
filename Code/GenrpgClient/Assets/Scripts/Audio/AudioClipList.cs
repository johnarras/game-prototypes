using Assets.Scripts.Audio.Constants;
using Assets.Scripts.Core;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // Needed

public class FullAudioSource
{
    public PlayAudioData PlayData;
    public AudioSource Source;
}

[Serializable]
public class WeightedAudioClip : IWeightedItem
{
    [field: SerializeField]
    public double Weight { get; set; }

    public AudioClip Clip;
}

public class AudioClipList : BaseBehaviour
{

    protected IClientRandom _rand = null;

    public float Volume = AudioConstants.MaxVolume;
    public bool Is3D = false;
    public float TtlSeconds = AssetConstants.DefaultTtl;
    public List<WeightedAudioClip> AudioClips;
    public bool IsActiveMusic { get; set; }


    protected DateTime _unloadTime { get; set; } = DateTime.UtcNow.AddSeconds(AssetConstants.DefaultTtl);

    // Not set in editor because it might be different depending on how we do overrides and such
    public string Name { get; set; }

    protected List<FullAudioSource> _sources = new List<FullAudioSource>();

    private bool _isValid = false;
    public bool IsValid()
    {
        if (_isValid)
        {
            return true;
        }

        if (AudioClips == null || AudioClips.Count < 1)
        {
            return false;
        }

        for (int c = 0; c < AudioClips.Count; c++)
        {
            if (AudioClips[c] == null)
            {
                _isValid = false;
                return false;
            }
        }
        _isValid = true;
        return true;
    }

    private void UpdateUnloadTime()
    {
        _unloadTime = DateTime.UtcNow.AddSeconds(TtlSeconds);
    }

    public void UpdateVolume(Dictionary<EAudioCategories, AudioChannel> channels)
    {
        List<FullAudioSource> sources = _sources.ToList();

        foreach (FullAudioSource source in sources)
        {
            float currVolume = channels[source.PlayData.Category].Volume;

            if (currVolume == 0)
            {
                StopAll();
                continue;
            }
            else
            {
                source.Source.volume = source.PlayData.Volume * Volume * currVolume;
            }
        }
    }

    public AudioSource Play(PlayAudioData playData, int index = -1)
    {
        UpdateUnloadTime();
        if (!_isValid || playData == null)
        {
            return null;
        }

        AudioClip clip = null;
        if (index < 0 || index >= AudioClips.Count)
        {
            WeightedAudioClip weightedClip = RandUtils.GetRandomElement(AudioClips, _rand.Rand);
            if (weightedClip != null)
            {
                clip = weightedClip.Clip;
            }
        }
        else
        {
            WeightedAudioClip weightedClip = AudioClips[index];
            clip = weightedClip.Clip;
        }

        if (playData.Parent == null)
        {
            playData.Parent = entity;
        }

        if (clip == null)
        {
            return null;
        }

        AudioSource source = playData.Parent.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = playData.Looping;
        source.volume = playData.Volume * Volume;

        if (playData.VarianceScale > 0)
        {
            source.volume *= (1 + RandUtils.DeltaRange(playData.VarianceScale, _rand.Rand));
            source.pitch *= (1 + RandUtils.DeltaRange(playData.VarianceScale, _rand.Rand));
        }

        if (playData.MusicData != null || !Is3D)
        {
            source.spatialBlend = 0;
        }
        else if (Is3D)
        {
            source.spatialBlend = 1;
            source.minDistance = 10;
            source.maxDistance = 50;
        }

        if (!source.loop)
        {
            source.PlayOneShot(source.clip, source.volume);
        }
        else
        {
            source.volume = playData.Volume;
            source.Play();
        }
        _sources.Add(new FullAudioSource()
        {
            PlayData = playData,
            Source = source,
        });
        return source;
    }

    public void StopSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        FullAudioSource fullSource = _sources.FirstOrDefault(x => x.Source == source);

        if (fullSource == null)
        {
            return;
        }

        _sources.Remove(fullSource);
        _clientEntityService.Destroy(fullSource.Source);
    }

    public void StopAll(float fadeTime = 0.0f)
    {
        List<FullAudioSource> sources = new List<FullAudioSource>(_sources);
        foreach (FullAudioSource source in sources)
        {
            StopSource(source.Source);
        }
    }

    public bool CanUnloadAudio()
    {
        // If we have sources, see if any were deleted or ended and remove them.
        if (_sources.Count > 0)
        {
            for (int s = 0; s < _sources.Count; s++)
            {
                FullAudioSource fullSource = _sources[s];
                AudioSource source = fullSource.Source;

                if (source == null)
                {
                    _sources.RemoveAt(s);
                    s--;
                }

                if (!source.isPlaying)
                {
                    source.clip = null;
                    _clientEntityService.Destroy(source);
                    fullSource.Source = null;
                    _sources.Remove(fullSource);
                    s--;
                }
            }
            if (_sources.Count < 1)
            {
                UpdateUnloadTime();
            }
            return false;
        }
        else if (!IsActiveMusic && _unloadTime < DateTime.UtcNow)
        {
            return true;
        }
        return false;
    }
}

