using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Attributes.WebApi
{
    public class AddDebuffPlayCountResponse : IWebResponse
    {
        public int DebuffDaysAdded { get; set; }
    }
}
