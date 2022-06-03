using System;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RevitTestApp.Services
{
    internal interface ISettingsStorageService
    {
        Settings Read();
        void Store(Settings settings);
    }


    internal class InFileStorageService : ISettingsStorageService
    {
        readonly string configsFileName;
        readonly string appFolder;

        public InFileStorageService()
        {
            appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RevitTestApp");
            configsFileName = "app_configs.json";
        }

        public string Fullname => Path.Combine(appFolder, configsFileName);

        public Settings Read()
        {
            try
            {
                JObject jo = JObject.Parse(File.ReadAllText(Fullname));
                return jo.ToObject<Settings>();
            }
            catch (Exception)
            {
                return new Settings();
            }
        }

        public void Store(Settings settings)
        {
            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);

            File.WriteAllText(Path.Combine(appFolder, configsFileName), JsonConvert.SerializeObject(settings));
        }
    }

    [DataContract]
    internal class Settings
    {
        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public string ParameterName { get; set; } = "";
    }
}