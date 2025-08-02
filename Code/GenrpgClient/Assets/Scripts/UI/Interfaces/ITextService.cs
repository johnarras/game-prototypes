using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Interfaces
{
    public interface ITextService : IInjectable
    {
        string HighlightText(string text, string color = TextColors.ColorYellow);
        string HighlightText(char c, string color = TextColors.ColorYellow);
        string GetLinkUnderMouse(IText text);
    }
}
