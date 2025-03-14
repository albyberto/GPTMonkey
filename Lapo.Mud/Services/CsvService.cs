using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Lapo.Services;

public class CsvService
{
    readonly string _path;

    public CsvService(string path)
    {
        path = $"{path}.csv";

        if (!File.Exists(path)) throw new FileNotFoundException("Report file not found.", path);

        _path = path;
    }

    public void Write(List<dynamic> records)
    {
        if (records.Count == 0) return;

        using var writer = new StreamWriter(_path);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        var firstRecord = (IDictionary<string, object>)records[0];

        foreach (var header in firstRecord.Keys) csv.WriteField(header);
        csv.NextRecord();

        foreach (var recordDict in records.Select(record => (IDictionary<string, object>)record))
        {
            foreach (var value in recordDict.Values) csv.WriteField(value);
            csv.NextRecord();
        }
    }
}