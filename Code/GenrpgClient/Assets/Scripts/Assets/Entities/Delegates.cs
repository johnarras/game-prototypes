using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Assets.Entities
{
    public delegate void OnDownloadHandler(object obj, object data, CancellationToken token);

    public delegate void SpriteListDelegate(object[] sprites);

}
