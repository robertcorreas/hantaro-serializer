using System.Runtime.Serialization;

namespace HantaroSerializer
{
    internal static class DataContractSerializeFactory
    {
        public static DataContractSerializer Create<T>(DataContractSerializeOptions options)
        {
            return new DataContractSerializer(typeof(T), options.KnownTypes, options.MaxItensInObjectGraph,options.IgnoreDataExtensionObjects,options.PreserveObjectReferences, options.DataContractSurrogate, options.DataContractResolver);
        }
    }
}