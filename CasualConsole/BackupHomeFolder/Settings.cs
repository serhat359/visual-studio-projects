using System.IO;
using Newtonsoft.Json;

namespace BackupHomeFolder
{
    class Settings
    {
        private const string settingFileName = "settings.json";

        public static void Set(AppSetting setting)
        {
            string jsonString = JsonConvert.SerializeObject(setting, Formatting.Indented);

            File.WriteAllText(settingFileName, jsonString);
        }

        public static AppSetting Get()
        {
            try
            {
                string jsonString = File.ReadAllText(settingFileName);

                return JsonConvert.DeserializeObject<AppSetting>(jsonString);
            }
            catch (FileNotFoundException)
            {
                return new AppSetting();
            }
        }
    }

    class AppSetting
    {
        public string SourceFolder { get; set; }

        public string DestinationFolder { get; set; }
    }
}
