using CommunityToolkit.HighPerformance.Buffers;
using MessagePack;
using OxDb.SharedCore.Serialization.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace OxDb.SharedCore.Serialization.Services
{
    public class MessagePackBinarySerializer : IBinarySerializer
    {
        public string SerializeToString(object obj)
        {
            ArrayPoolBufferWriter<byte> writer = GetBuffer();

            BinarySerialize(obj, writer);

            string txt = Encoding.ASCII.GetString(writer.WrittenSpan.ToArray());

            ReturnBuffer(writer);

            return txt;
        }

        public T Deserialize<T>(string txt) where T : class
        {
            return Deserialize<T>(Encoding.UTF8.GetBytes(txt));
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> bytes, int length = 0) where T : class
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }

        public T MakeCopy<T>(T t) where T : class
        {
            ArrayPoolBufferWriter<byte> writer = GetBuffer();
            BinarySerialize(t, writer);
            T obj = Deserialize<T>(writer.WrittenMemory);
            ReturnBuffer(writer);
            return obj;
        }

        public void BinarySerialize(object obj, ArrayPoolBufferWriter<byte> writerYouMustDispose)
        {
            writerYouMustDispose.Clear();
            MessagePackSerializer.Serialize(writerYouMustDispose, obj);
        }

        private ConcurrentQueue<ArrayPoolBufferWriter<byte>> _bufferPool = new ConcurrentQueue<ArrayPoolBufferWriter<byte>>();

        public ArrayPoolBufferWriter<byte> GetBuffer()
        {
            if (_bufferPool.TryDequeue(out ArrayPoolBufferWriter<byte> buffer))
            {
                return buffer;
            }
            return new ArrayPoolBufferWriter<byte>();
        }

        public void ReturnBuffer(ArrayPoolBufferWriter<byte> buffer)
        {
            buffer.Clear();
            // Do not dispose. Maybe dispose if the buffer is too big?
            if (buffer.Capacity > 1024)
            {
                buffer.Dispose();
            }
            else
            {
                _bufferPool.Enqueue(buffer);
            }
        }
    }
}


