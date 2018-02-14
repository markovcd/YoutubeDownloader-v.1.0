using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

namespace YoutubeDownloader.Utilities
{
    public sealed class SettingsSingleton
    {
        public static readonly string FileName = "settings.xml";

        public static string FilePath { get { return Path.Combine(FileHelper.GetApplicationFolder(), FileName); } }

        private static SettingsSingleton _instance = new SettingsSingleton();

        public static SettingsSingleton Instance { get { return _instance; } }

        public SettingsModel Model { get; private set; }

        private SettingsSingleton()
        {
            Deserialize();
        }

        public void Deserialize()
        {
            Model = Deserialize(FilePath);
        }

        public void Serialize()
        {
            Serialize(FilePath, Model);
        }

        public static SettingsModel Deserialize(string filePath)
        {
            var serializer = new XmlSerializer(typeof(SettingsModel));

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                return serializer.Deserialize(fileStream) as SettingsModel;
            }
        }

        public static void Serialize(string filePath, SettingsModel settings)
        {
            var serializer = new XmlSerializer(typeof(SettingsModel));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(fileStream, settings);
            }

        }
    }
}
