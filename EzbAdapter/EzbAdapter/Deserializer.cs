using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using EzbAdapter.Contracts;
using EzbAdapter.Contracts.Xml.Xml2CSharp;

namespace EzbAdapter
{
    public class Deserializer
    {
        public static XmlGenericData Deserialize(Stream content, out List<ErrorMessage> errorObjects)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XmlGenericData));
            var obj = (XmlGenericData)serializer.Deserialize(content);
            errorObjects = new List<ErrorMessage>();
            return obj;
        }
    }
}