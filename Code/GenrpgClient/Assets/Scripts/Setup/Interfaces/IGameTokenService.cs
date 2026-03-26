using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Assets.Scripts.Setup.Interfaces
{
    public interface IGameTokenService
    {
        void SetGameToken(CancellationToken token);
    }
}
