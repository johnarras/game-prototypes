using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.WorldCanvas.Interfaces
{
    public interface IDynamicUIItem
    {
        bool FrameUpdateIsComplete(float deltaTime);
    }
}
