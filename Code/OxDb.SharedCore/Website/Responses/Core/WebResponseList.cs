using OxDb.SharedCore.Website.Responses.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedCore.Website.Responses.Core
{

    public interface IWebContext : IDisposable
    {
        List<IWebResponse> GetResponseList();
        void AddResponse(IWebResponse response);
        void AddFront(IWebResponse response);
        void ClearResponses();
        void AddResponseRange(List<IWebResponse> responses);
        void ShowError(string errorMessage);
    }

    public class WebResponseList
    {
        private List<IWebResponse> _responses = new List<IWebResponse>();
        public List<IWebResponse> GetResponses() { return _responses.ToList(); }
        public void AddResponse(IWebResponse response) { _responses.Add(response); }
        public void AddFront(IWebResponse response) { _responses.Insert(0, response); }

        public void Clear() { _responses.Clear(); }
        public void AddRange(List<IWebResponse> list) { _responses.AddRange(list); }
    }

}


