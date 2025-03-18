using System.Data;
using System.Linq.Expressions;
using Lapo.Mud.Options;
using Microsoft.Extensions.Options;

namespace Lapo.Mud.Services;

public class TableService(IOptions<AluOptions> options)
{
    readonly string _primaryKeyColumn = options.Value.PrimaryKeyColumn;
    readonly string _tableColumn = options.Value.TableColumn;
    
    public (Dictionary<string, List<Dictionary<string, object>>> newGridData, Dictionary<string, Dictionary<string, Type>> newColumns) BuildTable(Dictionary<string, List<DataRow>> diff)
    {
        var tables = diff.Keys.ToList(); // Usa le tabelle presenti in _diff
        var newGridData = new Dictionary<string, List<Dictionary<string, object>>>();
        var newColumns = new Dictionary<string, Dictionary<string, Type>>();

        foreach (var table in tables)
        {
            if (!diff.TryGetValue(table, out var dataRows) || dataRows.Count == 0)
                continue;

            var firstRow = dataRows[0];

            var columns = firstRow.Table.Columns
                .Cast<DataColumn>()
                .Where(col => col.ColumnName != _tableColumn && col.ColumnName != _primaryKeyColumn) // ⬅️ Exclude columns Table and Id
                .ToDictionary(col => col.ColumnName, col => col.DataType);

            newColumns[table] = columns;

            var rows = dataRows
                .Select(row => columns.Keys.ToDictionary(colName => colName, colName => row[colName]))
                .ToList();

            newGridData[table] = rows;
        }

        return (newGridData, newColumns);
    }

    public static Expression<Func<T, TP>> GetPropertyLambdaExpression<T, TP>(string propertyName)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var keyProperty = typeof(T).GetProperty("Item");
        var indexArgument = Expression.Constant(propertyName);
        var propertyAccess = Expression.MakeIndex(param, keyProperty, [indexArgument]);
        var lambdaExpression = Expression.Lambda<Func<T, TP>>(propertyAccess, param);

        return lambdaExpression;
    }
}