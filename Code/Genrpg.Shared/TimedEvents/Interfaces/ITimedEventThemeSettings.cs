using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.TimedEvents.Interfaces
{
    public interface ITimedEventThemeSettings
    {
        ITimedEventTheme GetTheme(long themeId);
    }
}
