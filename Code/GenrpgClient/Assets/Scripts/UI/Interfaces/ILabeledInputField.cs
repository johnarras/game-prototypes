namespace Genrpg.Shared.UI.Interfaces
{
    public interface ILabeledInputField
    {
        void SetLabel(string text);
        void SetPlaceholder(string text);
        void SetInputText(string text);
        string GetInputText();
    }
}


