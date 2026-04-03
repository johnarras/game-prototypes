using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Interfaces;

namespace Assets.Scripts.UI.Interfaces
{
    public interface ITextService : IInjectable
    {
        string HighlightText(string text, string color = TextColors.ColorYellow);
        string HighlightText(char c, string color = TextColors.ColorYellow);
        string GetLinkUnderMouse(IText text);
    }
}


