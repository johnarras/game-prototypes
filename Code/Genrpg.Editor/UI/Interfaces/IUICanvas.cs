
namespace Genrpg.Editor.UI.Interfaces
{
    public interface IUICanvas
    {
        void Add(object elem, double x, double y);
        void Remove(object elem);
        bool Contains(object elem);
    }
}
