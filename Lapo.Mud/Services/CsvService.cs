using System.Data;
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

        if (!File.Exists(path)) File.Create(path);

        _path = path;
    }

    public void Write(Dictionary<string, List<DataRow>> records)
    {
        if (records.Count == 0) return;

        using var writer = new StreamWriter(_path, append: true);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        foreach (var (tableName, dataRows) in records)
        {
            if (dataRows.Count == 0) continue; // Evita di scrivere tabelle vuote

            // Scrive l'intestazione con il nome della tabella
            writer.WriteLine($"Table: {tableName}");

            // Scrive l'header della tabella (colonne)
            var firstRow = dataRows.First();
            foreach (DataColumn column in firstRow.Table.Columns)
            {
                csv.WriteField(column.ColumnName);
            }
            csv.NextRecord();

            // Scrive i dati delle righe
            foreach (DataRow row in dataRows)
            {
                foreach (var item in row.ItemArray)
                {
                    csv.WriteField(item);
                }
                csv.NextRecord();
            }
        }
    }

}