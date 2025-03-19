using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Lapo.Mud.Options;
using Microsoft.Extensions.Options;

namespace Lapo.Mud.Services;

public class CsvService
{
    readonly ILogger<CsvService> _logger;
    readonly string _path;

    public CsvService(IOptions<CsvOptions> options, ILogger<CsvService> logger)
    {
        _logger = logger;
        var path = Path.Combine(AppContext.BaseDirectory, $"{options.Value.Path}.csv");

        if (!File.Exists(path)) File.Create(path);

        _path = path;
        
        _logger.LogInformation($"CsvService initialized with path '{_path}'.");
    }

    public void Write(Dictionary<string, List<DataRow>> diff)
    {
        foreach (var (key, data) in diff.OrderBy(x => x.Key))
        {
            if (data.Count == 0) return;

            using var writer = new StreamWriter(_path, append: true);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            // Write table name
            // csv.WriteField(key);
            // csv.NextRecord();

            // Write header
            foreach (DataColumn column in data.First().Table.Columns) csv.WriteField(column.ColumnName);
            csv.NextRecord();

            // Write rows
            var rows = data.Select(row => row.ItemArray);
            foreach (var row in rows)
            {
                foreach (var item in row) csv.WriteField(item);
                csv.NextRecord();
            }
            
            _logger.LogInformation($"Wrote {data.Count} rows for table '{key}'.");
        }
    }

    public void Backup()
    {
        var backupPath = Path.Combine(AppContext.BaseDirectory, $"{Path.GetFileNameWithoutExtension(_path)}_backup_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(_path)}");
        File.Copy(_path, backupPath, overwrite: true);
        _logger.LogInformation($"Backup created at '{backupPath}'.");
    }
    
    public void Delete()
    {
        File.Delete(_path);
        _logger.LogInformation($"Deleted file '{_path}'.");
    }
}