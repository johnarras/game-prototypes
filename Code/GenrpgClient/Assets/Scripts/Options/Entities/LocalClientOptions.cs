
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;

public class ClientFlags
{
    public const int ChatActive = 1 << 0;
    public const int IsFullScreen = 1 << 1;
}
/// <summary>
/// This is used on the client to store things that must be loaded instantly when the game starts.
/// </summary>
public class LocalClientOptions : IStringId
{
    public string Id { get; set; }
    public int ScreenWidth { get; set; } = 1920;
    public int ScreenHeight { get; set; } = 1080;
    public int Flags { get; set; }
    public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
    public void AddFlags(int flagBits) { Flags |= flagBits; }
    public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

    public SmallIdFloatCollection AudioVolumes { get; set; } = new SmallIdFloatCollection();

    public float GetVolume(EAudioCategories category)
    {
        return AudioVolumes.Get((int)category);
    }

    public void SetVolume(EAudioCategories category, float volume)
    {
        AudioVolumes.Set((int)category, volume);
    }
}