using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Website.Interfaces
{
    public interface IAccountAuthRequest : IWebRequest
    {
        long AccountProductId { get; set; }
        string ReferrerId { get; set; }
        string DeviceId { get; set; }
        string Password { get; set; }
    }
}
