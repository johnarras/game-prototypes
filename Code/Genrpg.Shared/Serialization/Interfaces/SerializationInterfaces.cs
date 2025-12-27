using CommunityToolkit.HighPerformance.Buffers;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.Serialization.Interfaces
{
    /// <summary>
    /// This class is used to serialize and deserialize all kinds of data.
    /// IT is used for 
    /// 1. Server data storage I/O
    /// 2. Cache I/O
    /// 3. Asynchronous Client/Server communications
    /// 4. Editor I/O
    /// 5. Sending commands to all instances in a role.
    /// 6. Realtime Client/Server communication
    /// 7. Client device I/O
    /// </summary>
    /// 

    public interface ISerializer
    {
        string SerializeToString(object obj);
        T Deserialize<T>(string txt) where T : class;
        T Deserialize<T>(ReadOnlyMemory<byte> byteSpan, int length = 0) where T : class;
        T MakeCopy<T>(T t) where T : class;
        void BinarySerialize(object obj, ArrayPoolBufferWriter<byte> writerYouMustDispose);
        ArrayPoolBufferWriter<byte> GetBuffer();
        void ReturnBuffer(ArrayPoolBufferWriter<byte> buffer);
    }


    public interface ITextSerializer : IInjectable, ISerializer
    {
        string PrettyPrint(object obj);
        object DeserializeWithType(string txt, Type t);
        TOutput ConvertType<TInput, TOutput>(TInput input) where TInput : class where TOutput : class;
    }

    public interface IBinarySerializer : IInjectable, ISerializer
    {
    }


}


