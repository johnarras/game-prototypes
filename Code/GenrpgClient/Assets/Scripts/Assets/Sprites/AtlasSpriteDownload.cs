using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Assets.Entities
{
    public class AtlasSpriteDownload
    {
        public string AtlasName;
        public string SpriteName;
        public OnDownloadHandler FinalHandler;
        public GImage TargetImage;
    }

}
