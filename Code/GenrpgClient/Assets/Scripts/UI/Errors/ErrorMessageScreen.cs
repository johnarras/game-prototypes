using OxDb.SharedCore.Website.Responses.Errors;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.UI.Screens
{
    public abstract class ErrorMessageScreen : BaseScreen
    {
        public GText ErrorText;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            ErrorResponse errorResponse = data as ErrorResponse;

            if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Error))
                ShowError(errorResponse.Error);
            await Task.CompletedTask;
        }

        public virtual void ShowError(string errorMessage)
        {
            _uiService.SetText(ErrorText, errorMessage);
        }

    }
}


