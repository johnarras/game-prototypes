using OxDb.SharedCore.Website.Responses.Interfaces;

namespace OxDb.SharedGame.Attributes.WebApi
{
    public class AddDebuffPlayCountResponse : IWebResponse
    {
        public int DebuffDaysAdded { get; set; }
    }
}
