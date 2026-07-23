using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace OxDb.Client.UI.Overrides
{
    [UxmlElement]
    public partial class GImage : Image
    {
        private UILifecycleHelper _lifecycle;
        public CancellationToken DestroyToken => _lifecycle?.Token ?? CancellationToken.None;
        public GImage() => _lifecycle = new UILifecycleHelper(this);

        public void SetImageData(Texture2D texture)
        {
            // This is the equivalent of a RawImage
            this.image = texture;
            this.sprite = null; // ClearFullCell sprite to ensure the texture shows
        }

        public void SetImageData(Sprite sprite)
        {
            // This is the equivalent of a standard Image
            this.sprite = sprite;
            this.image = null; // ClearFullCell texture to ensure the sprite shows
        }


    }
}
