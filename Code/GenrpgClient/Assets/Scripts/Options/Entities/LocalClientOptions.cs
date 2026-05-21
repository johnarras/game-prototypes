
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils.Data;


public class ClientDefaultOptions
{
    public const int ScreenWidth = 1920;
    public const int ScreenHeight = 1080;
}

public class ClientFlags
{
    public const int ChatActive = 1 << 0;
    public const int IsFullScreen = 1 << 1;
    public const int ClassicMovement = 1 << 2;
}
/// <summary>
/// This is used on the client to store things that must be loaded instantly when the game starts.
/// </summary>
public class LocalClientOptions : IStringId
{
    public string Id { get; set; }
    public int ScreenWidth { get; set; } = ClientDefaultOptions.ScreenWidth;
    public int ScreenHeight { get; set; } = ClientDefaultOptions.ScreenHeight;
    public int Flags { get; set; }
    public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
    public void AddFlags(int flagBits) { Flags |= flagBits; }
    public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

    public SmallIdFloatCollection AudioVolumes { get; set; } = new SmallIdFloatCollection();

    public float GetVolume(EAudioCategories category)
    {
        return AudioVolumes[(int)category];
    }

    public void SetVolume(EAudioCategories category, float volume)
    {
        AudioVolumes[(int)category] = volume;
    }
}

