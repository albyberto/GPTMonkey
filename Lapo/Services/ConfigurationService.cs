using Newtonsoft.Json.Linq;

namespace Lapo.Services;

public class ConfigurationService
{
    readonly string _path;
    readonly string _section;

    public ConfigurationService(string path, string section)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Configuration file not found.", path);

        _path = path;
        _section = section;
    }

    public async Task UpsertAsync<T>(string key, T value)
    {
        var json = await File.ReadAllTextAsync(_path);
        var jsonObj = JObject.Parse(json);
        var sectionPath = _section.Split(':');
        var keyPath = key.Split(':');

        JToken targetSection = jsonObj;
        foreach (var section in sectionPath)
        {
            if (targetSection[section] is not JObject obj)
            {
                obj = new();
                targetSection[section] = obj;
            }

            targetSection = obj;
        }

        foreach (var part in keyPath[..^1])
        {
            if (targetSection[part] is not JObject obj)
            {
                obj = new();
                targetSection[part] = obj;
            }

            targetSection = obj;
        }

        targetSection[keyPath[^1]] = JToken.FromObject(value);
        await File.WriteAllTextAsync(_path, jsonObj.ToString());
    }

    public async Task RemoveAsync(string key)
    {
        var json = await File.ReadAllTextAsync(_path);
        var jsonObj = JObject.Parse(json);
        var sectionPath = _section.Split(':');
        var keyPath = key.Split(':');

        JToken targetSection = jsonObj;
        foreach (var section in sectionPath)
        {
            if (targetSection[section] is not JObject obj) return;
            targetSection = obj;
        }

        foreach (var part in keyPath[..^1])
        {
            if (targetSection[part] is not JObject obj) return;
            targetSection = obj;
        }

        if (targetSection is not JObject parentSection) return;
        parentSection.Remove(keyPath[^1]);
        await File.WriteAllTextAsync(_path, jsonObj.ToString());
    }
}