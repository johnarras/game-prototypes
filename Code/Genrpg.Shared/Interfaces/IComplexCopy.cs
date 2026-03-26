using Genrpg.Shared.Serialization.Interfaces;

namespace Genrpg.Shared.Interfaces
{
    public interface IComplexCopy
    {
        void DeepCopyFrom(IComplexCopy from, ISerializer serializer);
        object GetDeepCopyData();
    }
}


