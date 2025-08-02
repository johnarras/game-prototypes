using Genrpg.Shared.UI.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Interfaces
{
    public interface IScreen
    {
        long ScreenId { get; }
        Task StartOpen(object data, CancellationToken token);
        void StartClose();
        void ErrorClose(string txt);
        void OnInfoChanged();
        bool BlockMouse();
        string GetName();
        CancellationToken GetToken();
    }
}
