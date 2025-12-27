using MessagePack;
namespace Genrpg.Shared.Utils
{
    /// <summary>
    /// Use this to return an error message and a "valid or not" bool, along with the data originally sent in.
    /// </summary>
    public class ValidityResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}


