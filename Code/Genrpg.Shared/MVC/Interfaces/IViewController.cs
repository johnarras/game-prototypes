using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.MVC.Interfaces
{

    public interface IViewController
    {
        IView GetView();
        CancellationToken GetToken();
    }

    public interface IViewController<TModel, TView> : IViewController where TView : IView
    {
        Task Init(TModel model, TView view, CancellationToken token);
        TModel GetModel();
    }
}
