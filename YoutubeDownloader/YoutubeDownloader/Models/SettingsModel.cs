using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
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

        public string Mp3DestinationPath { get; set; }


        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
        public XmlSchema GetSchema()
        {
            return null;
        }

        public static SettingsModel GetDefault()
        {
            return new SettingsModel
            {
                Mp3DestinationPath = ""
            };
        }
    }

    

}
