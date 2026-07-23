using OxDb.Client.UI.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.UI.Interfaces;

namespace OxDb.Client.UI.Interfaces
{
    public interface ITextService : IInjectable
    {
        string HighlightText(string text, string color = TextColors.ColorYellow);
        string HighlightText(char c, string color = TextColors.ColorYellow);
        string GetLinkUnderMouse(IText text);
    }
}


