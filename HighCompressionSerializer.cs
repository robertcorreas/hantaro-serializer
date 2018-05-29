using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;

namespace HantaroSerializer
{
    public class HighCompressionSerializer
    {
        private readonly SerializeType _type;

        #region Construtores

        public HighCompressionSerializer(SerializeType type)
        {
            _type = type;
        }

        #endregion

        public void Serialize<T>(string filePath, T data, DataContractSerializeOptions options = null)
        {
            try
            {
                if (_type == SerializeType.XmlDataContractSerialize)
                {
                    if (options == null)
                    {
                        throw new ArgumentException("DataContract Serializer needs a DataContractSerialzieOptions!");
                    }

                    SerializeWithDataContractInternal(data, filePath, options);
                }
                else if (_type == SerializeType.XmlSerialize)
                {
                    SerializeWithXmlSerializerInternal(data, filePath);
                }

                if (_type == SerializeType.XmlDataContractSerialize)
                {
                    var tempFilePathCompressed = GetTempFilePathCompressed(filePath);
                    FileCompressorDecompressor.CompressFileLZMA(filePath, tempFilePathCompressed);
                    File.Delete(filePath);
                    File.Move(tempFilePathCompressed, filePath);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Arquivo inválido - Serializador - Salvar", e);
            }
        }

        public void Deserialize<T>(string filePath, out T newData, DataContractSerializeOptions options = null)
        {
            if (_type == SerializeType.XmlDataContractSerialize)
            {
                if (options == null)
                {
                    throw new ArgumentException("DataContract Serializer needs a DataContractSerialzieOptions!");
                }

                DeserializeXmlDataContractSerializerInternal(filePath, out newData, options);
            }
            else if (_type == SerializeType.XmlSerialize)
            {
                DeserializeXmlSerializerInternal(filePath, out newData);
            }
            else
            {
                newData = default(T);
            }
        }

        private string GetTempFilePathCompressed(string tempFilePath)
        {
            var id = Guid.NewGuid();
            return tempFilePath + id;
        }

        private void SerializeWithDataContractInternal<T>(T data, string tempFilePath, DataContractSerializeOptions options)
        {
            var dcs = DataContractSerializeFactory.Create<T>(options);
            using (var file = File.Create(tempFilePath))
            using (var gzipFile = new GZipStream(file, CompressionMode.Compress))
            using (var writer = XmlWriter.Create(gzipFile, new XmlWriterSettings { Indent = true }))
            {
                dcs.WriteObject(writer, data);
            }
        }

        private void SerializeWithXmlSerializerInternal<T>(T data, string tempFilePath)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var file = File.Create(tempFilePath))
            using (var gzipFile = new GZipStream(file, CompressionMode.Compress))
            using (var writer = new StreamWriter(gzipFile))
            {
                serializer.Serialize(writer, data);
            }
        }

        private void DeserializeXmlDataContractSerializerInternal<T>(string filePath, out T newData, DataContractSerializeOptions options)
        {
            var decompressFilePath = filePath + Guid.NewGuid();

            FileCompressorDecompressor.DecompressFileLZMA(filePath, decompressFilePath);

            var dcs = DataContractSerializeFactory.Create<T>(options);
            try
            {
                using (var gzipStream = new GZipStream(File.OpenRead(decompressFilePath), CompressionMode.Decompress))
                using (var reader = XmlReader.Create(gzipStream))
                {
                    newData = (T)dcs.ReadObject(reader);
                }

                File.Delete(decompressFilePath);
            }
            catch (Exception e)
            {
                newData = default(T);
                throw new ArgumentException("Arquivo inválido - Serializador - Abrir", e);
            }
        }

        private void DeserializeXmlSerializerInternal<T>(string filePath, out T newData)
        {
            if (TryOpenCompressedFile(filePath, out newData))
            {
                return;
            }
            if (TryOpenNotCompressedFile(filePath, out newData))
            {
                return;
            }
        }

        private bool TryOpenCompressedFile<T>(string filePath, out T newData)
        {
            try
            {
                var deserializer = new XmlSerializer(typeof(T));
                using (var gzipStream = new GZipStream(File.OpenRead(filePath), CompressionMode.Decompress))
                using (var reader = new StreamReader(gzipStream))
                {
                    newData = (T)deserializer.Deserialize(reader);
                }

                return true;
            }
            catch (Exception)
            {
                newData = default(T);
                return false;
            }
        }

        private bool TryOpenNotCompressedFile<T>(string filePath, out T newData)
        {
            try
            {
                var deserializer = new XmlSerializer(typeof(T));
                using (var textReader = new StreamReader(filePath))
                {
                    newData = (T)deserializer.Deserialize(textReader);
                }
                return true;

            }
            catch (Exception)
            {
                newData = default(T);
                return false;
            }
        }
    }
}