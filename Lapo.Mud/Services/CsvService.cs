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
        path = $"{path}.csv";

        if (!File.Exists(path)) File.Create(path);

        _path = path;
    }

    public void Write(List<DataRow> rows)
    {
        if (rows.Count == 0) return;

        using var writer = new StreamWriter(_path, append: true);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        // Scrive l'header della tabella (colonne)
        foreach (DataColumn column in rows.First().Table.Columns) csv.WriteField(column.ColumnName);
        csv.NextRecord();

        // Scrive i dati
        foreach (var item in rows.SelectMany(row => row.ItemArray.Cast<DataRow>()))
        {
            csv.WriteField(item);
            csv.NextRecord();
        }
    }
}