using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Website.Messages.Error
{
    public class ErrorResponse : IWebResponse
    {
        public string Error { get; set; }
    }
}


