using OxDb.SharedCore.Utils;
using System;
using System.Text;
using System.Threading;
using UnityEngine.UIElements;

namespace OxDb.Client.UI.Overrides
{
    public class UILifecycleHelper
    {
        private CancellationTokenSource _cts;
        private readonly VisualElement _element;

        public string ClassName => _className;
        private string _className;

        public CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public UILifecycleHelper(VisualElement element)
        {
            Type t = element.GetType();

            _element = element;
            string[] nameWords = StrUtils.SplitOnCapitalLetters(t.Name).Split(' ');

            StringBuilder sb = new StringBuilder();

            foreach (string word in nameWords)
            {
                sb.Append(word.ToLower() + "-");
            }
            sb.Append("base");

            _className = sb.ToString();

            _element.AddToClassList(_className);

            _element.RegisterCallback<AttachToPanelEvent>(OnAttach);
            _element.RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            _cts = new CancellationTokenSource();
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}