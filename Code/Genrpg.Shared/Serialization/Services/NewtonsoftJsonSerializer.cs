using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Genrpg.Shared.Serialization.Interfaces;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace Genrpg.Shared.Serialization.Services
{
    public class NewtonsoftTextSerializer : ITextSerializer
    {
        private JsonSerializerSettings _baseSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            Formatting = Formatting.None,
        };


        private JsonSerializerSettings _prettyPrintSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            Formatting = Formatting.Indented,
        };

        public string PrettyPrint(object obj)
        {
            return SerializeInternal(obj, _prettyPrintSettings);
        }

        public string SerializeToString(object obj)
        {
            return SerializeInternal(obj, _baseSettings);
        }

        private string SerializeInternal(object obj, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(obj, settings);
        }

        public object DeserializeWithType(string txt, Type t)
        {
            int newIndex = 0;
            while (newIndex < txt.Length && txt[newIndex] != '{' && txt[newIndex] != '[')
            {
                newIndex++;
            }
            if (newIndex > 0)
            {
                txt = txt.Substring(newIndex);
            }
            return JsonConvert.DeserializeObject(txt, t, _baseSettings);
        }

        public T Deserialize<T>(string txt) where T : class
        {
            return JsonConvert.DeserializeObject<T>(txt, _baseSettings);
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> bytes, int length = 0) where T : class
        {
            return Deserialize<T>(Encoding.ASCII.GetString(bytes.ToArray(), 0, length));
        }

        public T MakeCopy<T>(T t) where T : class
        {
            return (T)DeserializeWithType(SerializeToString(t), t.GetType());
        }

        public TOutput ConvertType<TInput, TOutput>(TInput input) where TInput : class where TOutput : class
        {
            string txt = SerializeToString(input);
            return Deserialize<TOutput>(txt);
        }

        public void BinarySerialize(object obj, ArrayPoolBufferWriter<byte> writerYouMustDispose)
        {
            writerYouMustDispose.Clear();

            string txt = SerializeToString(obj);
            byte[] bytes = Encoding.UTF8.GetBytes(txt);

            ReadOnlySpan<byte> tempSpan = new ReadOnlySpan<byte>(bytes);

            writerYouMustDispose.Write(tempSpan);

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
            buffer.Dispose();
            _bufferPool.Enqueue(buffer);
        }
    }

}
