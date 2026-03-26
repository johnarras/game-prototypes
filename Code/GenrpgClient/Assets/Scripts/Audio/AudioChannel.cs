using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;

[Serializable]
public class AudioChannel
{
    public float Volume;
    public bool Looping;
    public EAudioCategories Category;
    public CurrentMusic curr = null;
    public long ChannelId;
    public List<CurrentMusic> prevList = new List<CurrentMusic>();

    public void ChooseNewRandomSound(IRandom rand)
    {

        if (curr == null || curr.clipList == null ||
             curr.clipList.Clips == null || curr.clipList.Clips.Count < 2)
        {
            curr.NextRandomizeTime = DateTime.UtcNow.AddMinutes(1);
            return;
        }

        float randTime = curr.GetRandomIzeSeconds();
        float nextRandTime = RandUtils.FloatRange(randTime / 2, randTime * 3 / 2, rand);
        curr.NextRandomizeTime = DateTime.UtcNow.AddSeconds(nextRandTime);


        curr.clipList.StopSource(curr.prevSource);
        curr.prevSource = curr.source;
        int skipIndex = -1;
        if (curr.source != null)
        {
            for (int i = 0; i < curr.clipList.Clips.Count; i++)
            {
                if (curr.clipList.Clips[i] == curr.source.clip)
                {
                    skipIndex = i;
                    break;
                }
            }
        }

        int newIndex = rand.Next() % curr.clipList.Clips.Count - 1;
        if (newIndex >= skipIndex)
        {
            newIndex++;
        }

        curr.source = curr.clipList.Play(curr.playData, newIndex);
    }
}

