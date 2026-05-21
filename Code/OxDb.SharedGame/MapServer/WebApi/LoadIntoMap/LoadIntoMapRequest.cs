using OxDb.SharedCore.Website.Interfaces;

namespace OxDb.SharedGame.MapServer.WebApi.LoadIntoMap
{
    public class LoadIntoMapRequest : IClientUserRequest
    {
        public string Env { get; set; }
        public string MapId { get; set; }
        public string InstanceId { get; set; }
        public string CharId { get; set; }
        public bool GenerateMap { get; set; }
        public string WorldDataEnv { get; set; }
    }
}


