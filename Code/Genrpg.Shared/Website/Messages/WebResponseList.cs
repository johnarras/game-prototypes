using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Website.Messages
{
    public class WebResponseList
    {
        private List<IWebResponse> _responses = new List<IWebResponse>();
        public List<IWebResponse> GetResponses() { return _responses.ToList(); }
        public void AddResponse(IWebResponse response) { _responses.Add(response); }

        public void Clear() { _responses.Clear(); }
        public void AddRange(IEnumerable<IWebResponse> list) { _responses.AddRange(list); } 
    }

}
