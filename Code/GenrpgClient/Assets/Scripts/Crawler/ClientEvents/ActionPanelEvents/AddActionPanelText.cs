
using OxDb.SharedCore.Client.Interfaces;
using System;

namespace OxDb.Client.Crawler.ClientEvents.ActionPanelEvents
{
    public class AddActionPanelText : IClientEvent
    {
        public string Text { get; set; }
        public Action OnClickAction { get; set; }


        public AddActionPanelText(string text, Action onClickAction = null)
        {
            Text = text;
            OnClickAction = onClickAction;
        }
    }
}


