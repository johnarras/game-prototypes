using Genrpg.DataUtils.Constants;
using Genrpg.DataUtils.Entities.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.DataUtils.Interfaces
{
    public interface ISmallPopup : IWindowBase
    {
        void StartClose();
    }

    public class StubPopup : ISmallPopup    
    {
        public void Add(object elem, double x, double y) { }

        public bool Contains(object elem)
        {

            return false;
        }

        public void Remove(object elem) { }

        public async Task<ISmallPopup> ShowBlockingDialog(string text, double width = 0, double height = 0)
        {
            await Task.CompletedTask;
            return new StubPopup();
        }

        public void ShowDataWindow(EditorGameState gs, object data, string action)
        {
        }

        public Task<EContentDialogResult> ShowMessageBox(string content, string title = null, bool showCancelButton = false)
        {
            return Task.FromResult(EContentDialogResult.Primary);
        }

        public void StartClose() { }
    }
}
