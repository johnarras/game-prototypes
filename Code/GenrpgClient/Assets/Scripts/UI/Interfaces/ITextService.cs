using Assets.Scripts.UI.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.UI.Interfaces;

namespace Assets.Scripts.UI.Interfaces
{
    public interface ITextService : IInjectable
    {
        string HighlightText(string text, string color = TextColors.ColorYellow);
        string HighlightText(char c, string color = TextColors.ColorYellow);
        string GetLinkUnderMouse(IText text);
    }
}


