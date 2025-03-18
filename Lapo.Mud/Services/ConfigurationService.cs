using Lapo.Mud.Components.Pages;
using Lapo.Mud.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Lapo.Mud.Services;

public class ConfigurationService
{
    readonly string _path;
    readonly string _section;

    public ConfigurationService(IOptions<ConfigurationOptions> options, ILogger<ConfigurationService> logger)
    {
        var path = Path.Combine(AppContext.BaseDirectory, $"{options.Value.Path}.json");
        if (!File.Exists(path)) throw new FileNotFoundException("Configuration file not found.", path);

        _path = path;
        _section = options.Value.Section;
        
        logger.LogInformation($"{nameof(ConfigurationService)} initialized with path '{_path}' and section '{_section}'.");
    }

    public async Task<T?> ReadAsync<T>(string key)
    {
        try
        {
            var json = await File.ReadAllTextAsync(_path);
            var jsonObj = JObject.Parse(json);
            var sectionPath = _section.Split(':');
            var keyPath = key.Split(':');

            JToken targetSection = jsonObj;

            // Naviga fino alla sezione specificata in _section
            foreach (var section in sectionPath)
            {
                if (targetSection[section] is not JObject obj)
                {
                    Console.WriteLine($"Section '{section}' not found in JSON.");
                    return default;
                }
                targetSection = obj;
            }

            // Naviga fino alla chiave specificata in key
            foreach (var part in keyPath[..^1])
            {
                if (targetSection[part] is not JObject obj)
                {
                    Console.WriteLine($"Key part '{part}' not found in JSON.");
                    return default;
                }
                targetSection = obj;
            }

            var token = targetSection[keyPath[^1]];

            if (token is null)
            {
                Console.WriteLine($"Key '{key}' not found in JSON.");
                return default;
            }

            // Se il tipo richiesto è una lista, assicuriamoci che il token sia un array
            if (typeof(T) != typeof(List<Configuration.TableConfig>) || token is JArray) return token.ToObject<T>(new());

            Console.WriteLine($"Expected JArray but found a different structure for key '{key}'.");
            return default;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading JSON key '{key}': {ex.Message}");
            return default;
        }
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