
using Genrpg.Shared.UI.Interfaces;

public class GRawImage : UnityEngine.UI.RawImage, IRawImage
{
    protected override void OnDestroy()
    {
        texture = null;
        base.OnDestroy();
    }
}

