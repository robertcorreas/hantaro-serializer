using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HantaroSerializer
{
    public class DataContractSerializeOptions
    {
        public DataContractSerializeOptions()
        {
            KnownTypes = null;
            MaxItensInObjectGraph = Int32.MaxValue;
            IgnoreDataExtensionObjects = false;
            PreserveObjectReferences = true;
            DataContractSurrogate = null;
            DataContractResolver = null;
        }

        public List<Type> KnownTypes { get; set; }
        public int MaxItensInObjectGraph { get; set; }
        public bool IgnoreDataExtensionObjects { get; set; }
        public bool PreserveObjectReferences { get; set; }
        public IDataContractSurrogate DataContractSurrogate { get; set; }
        public DataContractResolver DataContractResolver { get; set; }
    }
}