using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Assets.Entities
{
    public class AtlasSpriteDownload
    {
        public string atlasName;
        public string spriteName;
        public OnDownloadHandler finalHandler;
        public object data;
    }

}
