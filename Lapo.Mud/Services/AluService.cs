using System.Data;
using Lapo.Mud.Comparers;
using Lapo.Mud.Options;
using Microsoft.Extensions.Options;

namespace Lapo.Mud.Services;

public class AluService(IOptions<AluOptions> controlUnitOptions)
{
    string PrimaryKeyColumn => controlUnitOptions.Value.PrimaryKeyColumn;
    string OperationColumn => controlUnitOptions.Value.OperationColumn;
    string TableColumn => controlUnitOptions.Value.TableColumn;
    
    public Dictionary<string, List<DataRow>> LoadDiff(Dictionary<string, DataTable> previousScan, Dictionary<string, DataTable> lastScan)
    {
        var diff = new Dictionary<string, List<DataRow>>();

        foreach (var table in lastScan.Keys)
        {
            if (!previousScan.TryGetValue(table, out var previous)) continue;
            EnhanceDataTable(previous);

            var last = lastScan[table];
            EnhanceDataTable(last);

            var previousRows = previous.AsEnumerable().ToList();
            var lastRows = last.AsEnumerable().ToList();

            var previousDict = previousRows.ToDictionary(row => row[PrimaryKeyColumn]);
            var lastDict = lastRows.ToDictionary(row => row[PrimaryKeyColumn]);
            
            var added = lastRows
                .Where(row => !previousDict.ContainsKey(row[PrimaryKeyColumn]))
                .ToList();
            
            foreach (var row in added) row[OperationColumn] = "ADDED";
            
            var removed = previousRows
                .Where(row => !lastDict.ContainsKey(row[PrimaryKeyColumn]))
                .ToList();
            
            foreach (var row in removed) row[OperationColumn] = "REMOVED";

            var updatedOld = GetDiff(previousRows, lastDict).ToList();
            foreach (var row in updatedOld) row[OperationColumn] = "UPDATED_OLD";

            var updatedNew = GetDiff(lastRows, previousDict).ToList();
            foreach (var row in updatedNew) row[OperationColumn] = "UPDATED_NEW";
            
            var acc = new List<DataRow>();
            acc.AddRange(added);
            acc.AddRange(removed);
            acc.AddRange(updatedOld);
            acc.AddRange(updatedNew);
            foreach (var row in acc) row[TableColumn] = table;
            
            diff[table] = acc
                .OrderBy(x => x[PrimaryKeyColumn])
                .ThenBy(x => x[OperationColumn], new CustomOperationComparer())
                .ToList();
        }

        return diff;
    }
    
    public void ClearDataTable(Dictionary<string, DataTable> diff)
    {
        foreach (var table in diff.Values)
        {
            if (table.Columns.Contains(OperationColumn)) table.Columns.Remove(OperationColumn);
            if (table.Columns.Contains(TableColumn)) table.Columns.Remove(TableColumn);
        }
    }

    IEnumerable<DataRow> GetDiff(List<DataRow> left, Dictionary<object, DataRow> right) => 
        from row in left 
        let key = row[PrimaryKeyColumn] 
        let exists = right.ContainsKey(key) 
        where exists let lastRow = right[key] 
        let operator1 = string.Join("|", row.ItemArray.Select(x => x?.ToString())) 
        let operator2 = string.Join("|", lastRow.ItemArray.Select<object?, string?>(x => x?.ToString())) 
        where !string.Equals(operator1, operator2) 
        select row;

    void EnhanceDataTable(DataTable previous)
    {
        if (!previous.Columns.Contains(TableColumn))
        {
            previous.Columns.Add(TableColumn, typeof(string));
            previous.Columns[TableColumn]?.SetOrdinal(0);
        }

        if (previous.Columns.Contains(OperationColumn)) return;

        previous.Columns.Add(OperationColumn, typeof(string));
        previous.Columns[OperationColumn]?.SetOrdinal(1);
    }
}