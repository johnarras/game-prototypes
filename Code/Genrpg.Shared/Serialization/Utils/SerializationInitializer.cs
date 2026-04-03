using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Serialization.Utils
{

    public class InterfaceTypeList
    {
        public Type InterfaceType { get; set; }
        public List<Type> ChildInterfaces { get; set; } = new List<Type>();
        public List<Type> DerivedTypes { get; set; } = new List<Type>();
    }


    class MapApiTypeSettings
    {
        public Type InterfaceType { get; set; }
        public Type BaseClassType { get; set; }
        // 0 = toplevel, N+1 is subinterface. Recommend keeping it linear and to 1-2 levels tops.
        // Used to make the most frequent messagepack messages only use one byte for message type.
        // This is like the idea behind Huffman encoding. More bytes for less frequent symbols/messages.
        public int Order { get; set; }
        public string FilePath { get; set; }
        public List<string> FullMessageTypeNames { get; set; } = new List<string>();
        public string InterfaceTypeName => InterfaceType.Name;
        public string BaseTypeName => BaseClassType.Name;

        public string InterfaceDeclarationInCode => "public interface " + InterfaceTypeName;
    }



    public class LinePrefixPair
    {
        public string Prefix { get; set; }
        public string Suffix { get; set; }

    }

    public static class SerializationInitializer
    {
        public static void Init(string dirName, IReflectionService reflectionService)
        {
            MessagePackInitializer mpi = new MessagePackInitializer();  
            mpi.Init(dirName, reflectionService);   
            SystemTextJsonInitializer sji = new SystemTextJsonInitializer();
            sji.Init(dirName, reflectionService);
        }
    }
}