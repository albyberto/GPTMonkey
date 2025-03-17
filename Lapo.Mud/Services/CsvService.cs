using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Lapo.Mud.Services;

public class CsvService
{
    readonly string _path;

    public CsvService(string path)
    {
        var baseDirectory = AppContext.BaseDirectory;
        path = Path.Combine(baseDirectory, $"{path}.csv");

        if (!File.Exists(path)) File.Create(path).Dispose();

        _path = path;
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
        }
    }
}