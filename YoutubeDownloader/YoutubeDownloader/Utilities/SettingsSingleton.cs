using System;
using System.Xml.Serialization;
using System.IO;

namespace YoutubeDownloader
{
    sealed class SettingsSingleton : BaseViewModel
    {
        public string FilePath => Path.Combine(FileHelper.GetApplicationFolder(), Consts.SettingsFilename);

        private static SettingsSingleton _instance;
        public static SettingsSingleton Instance => _instance ?? (_instance = new SettingsSingleton());

        public SettingsModel Model { get; private set; }

        private SettingsSingleton()
        {
            Deserialize();
        }

        ~SettingsSingleton()
        {
            Serialize();
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

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    return serializer.Deserialize(fileStream) as SettingsModel;
                }
            }
            catch (Exception)
            {
                return SettingsModel.GetDefault();
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
