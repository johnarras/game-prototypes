using Genrpg.DataUtils.Constants;
using Genrpg.DataUtils.Entities.Core;

namespace Genrpg.DataUtils.Interfaces
{
    public interface IUICanvas
    {
        void Add(object elem, double x, double y);
        void Remove(object elem);
        bool Contains(object elem);
    }
}


