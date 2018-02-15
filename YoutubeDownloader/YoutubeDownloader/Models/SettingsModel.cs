using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

namespace YoutubeDownloader
{
    public sealed class SettingsModel : IXmlSerializable
    {
        public SettingsModel()
        {

        }

        public string Mp3DestinationDirectory { get; set; }


        public void ReadXml(XmlReader reader)
        {
            Mp3DestinationDirectory = reader.ReadString();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(Mp3DestinationDirectory);
        }
        public XmlSchema GetSchema()
        {
            return null;
        }

        public static SettingsModel GetDefault()
        {
            return new SettingsModel
            {
                Mp3DestinationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Consts.DefaultDirectoryName)
            };
        }
    }

    

}
