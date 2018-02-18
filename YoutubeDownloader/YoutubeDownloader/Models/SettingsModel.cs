using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

namespace YoutubeDownloader
{
    public sealed class SettingsModel : BindableBase, IXmlSerializable
    {
        public SettingsModel()
        {

        }

        private string _mp3DestinationDirectory;
        public string Mp3DestinationDirectory
        {
            get { return _mp3DestinationDirectory; }
            set { SetProperty(ref _mp3DestinationDirectory, value); }
        }

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
