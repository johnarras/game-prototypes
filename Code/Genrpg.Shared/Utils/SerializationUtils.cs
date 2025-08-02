using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Genrpg.Shared.Utils
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
        byte[] SerializeToBytes(object obj);
        T Deserialize<T>(string txt) where T : class;
        T Deserialize<T>(byte[] bytes, int length = 0) where T : class;
        T MakeCopy<T>(T t) where T : class;
    }

    public interface ITextSerializer : IInjectable, ISerializer, IExplicitInject
    {
        string PrettyPrint(object obj);
        object DeserializeWithType(string txt, Type t);
        object DeserializeWithType(byte[] bytes, Type t);
        TOutput ConvertType<TInput, TOutput>(TInput input) where TInput : class where TOutput : class;
    }

    public interface IBinarySerializer : IInjectable, ISerializer
    {

    }

    [MessagePackObject]
    public class NewtonsoftTextSerializer : ITextSerializer
    {
        private ILogService _logService = null!;

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
            return JsonConvert.SerializeObject(obj, _prettyPrintSettings);
        }

        public string SerializeToString(object obj)
        {
            return JsonConvert.SerializeObject(obj, _baseSettings);
        }

        public byte[] SerializeToBytes(object obj)
        {
            return Encoding.UTF8.GetBytes(SerializeToString(obj));
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

        public object DeserializeWithType(byte[] bytes, Type t)
        {
            return DeserializeWithType(Encoding.UTF8.GetString(bytes), t);  
        }

        public T Deserialize<T>(string txt) where T : class
        {
            return (T)DeserializeWithType(txt, typeof(T));
        }

        public T Deserialize<T>(byte[] bytes, int length = 0) where T : class
        {
            if (length == 0)
            {
                length = bytes.Length;
            }

            return Deserialize<T>(Encoding.ASCII.GetString(bytes, 0, length));
        }

        public T DeserializeFromBytes<T>(byte[] bytes, Type t) where T : class
        {
            return (T)DeserializeWithType(bytes, t);
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
    }


    [MessagePackObject]
    public class MessagePackBinarySerializer : IBinarySerializer
    {
        private ILogService _logService = null!;

        public string SerializeToString(object obj)
        {
            return Encoding.ASCII.GetString(SerializeToBytes(obj)); 
        }

        public byte[] SerializeToBytes(object obj)
        {
            return MessagePackSerializer.Serialize(obj);
        }

        public T Deserialize<T>(string txt) where T : class
        {
            return Deserialize<T>(Encoding.UTF8.GetBytes(txt));
        }

        public T Deserialize<T>(byte[] bytes, int length = 0) where T : class
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }

        public T MakeCopy<T>(T t) where T : class
        {
            return  Deserialize<T>(SerializeToBytes(t));
        }
    }
}
