using OxDb.SharedCore.Serialization.Interfaces;

namespace OxDb.SharedCore.Interfaces
{
    public interface IComplexCopy
    {
        void DeepCopyFrom(IComplexCopy from, ISerializer serializer);
        object GetDeepCopyData();
    }
}


