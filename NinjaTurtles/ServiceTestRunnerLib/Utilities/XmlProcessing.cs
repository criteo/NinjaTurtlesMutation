using System.IO;
using System.Xml.Serialization;

namespace NinjaTurtles.ServiceTestRunnerLib.Utilities
{
    internal static class XmlProcessing
    {
        public static string SerializeToXml<T>(T data)
        {
            var sw = new StringWriter();
            var serializer = new XmlSerializer(data.GetType());
            serializer.Serialize(sw, data);
            var xmlData = sw.ToString();
            sw.Close();
            return xmlData;
        }

        public static T DeserializeFromXml<T>(string xmlData)
        {
            var deserializer = new XmlSerializer(typeof(T));
            var sr = new StringReader(xmlData);
            var data = (T)deserializer.Deserialize(sr);
            sr.Close();
            return data;
        }
    }
}
