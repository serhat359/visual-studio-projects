using System.Text.Json;

namespace BackupHomeFolder;

class Settings
{
    private const string settingFileName = "settings.json";

    public static void Set(AppSetting setting)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        string jsonString = JsonSerializer.Serialize(setting, options);

        File.WriteAllText(settingFileName, jsonString);
    }

    public static AppSetting Get()
    {
        try
        {
            string jsonString = File.ReadAllText(settingFileName);

            return JsonSerializer.Deserialize<AppSetting>(jsonString);
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
