using System.Text;
using Dapper;
using Lapo.Enums;
using Microsoft.Data.SqlClient;

namespace Lapo.Services;

public class DatabaseService(IConfiguration configuration)
{
    readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
                                        ?? throw new ArgumentNullException(nameof(configuration), "Connection string not found.");

    public async Task<List<dynamic>> QueryAsync(string table, string? where = null, int? top = null, OrderByDirection direction = OrderByDirection.Desc)
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("Table name cannot be null or empty.", nameof(table));

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string keyQuery = """
                                    SELECT COLUMN_NAME
                                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                                    WHERE TABLE_NAME = @TableName
                                    AND OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1
                                """;

        var primaryKey = await connection.QueryFirstOrDefaultAsync<string>(keyQuery, new { TableName = table });

        if (string.IsNullOrEmpty(primaryKey)) throw new KeyNotFoundException($"Primary key not found for the table '{table}'.");

        var queryBuilder = BuildQuery(table, primaryKey, direction, top, where);
        var rows = (await connection.QueryAsync<dynamic>(queryBuilder)).ToList();

        foreach (var dictionaryRow in rows.Select(row => (IDictionary<string, object>)row)) dictionaryRow["Id"] = dictionaryRow[primaryKey];

        return rows;
    }
    
    static string BuildQuery(string table, string primary, OrderByDirection direction, int? top = null,
        string? where = null)
    {
        var queryBuilder = new StringBuilder();

        queryBuilder
            .Append("SELECT")
            .Append(' ');

        if (top.HasValue)
            queryBuilder
                .Append($"TOP ({top.Value})")
                .Append(' ');

        queryBuilder
            .Append('*')
            .Append(' ')
            .Append("FROM")
            .Append(' ')
            .Append(table)
            .Append(' ');

        if (!string.IsNullOrEmpty(where))
            queryBuilder
                .Append("WHERE")
                .Append(' ')
                .Append(where)
                .Append(' ');

        queryBuilder
            .Append("ORDER BY")
            .Append(' ')
            .Append(primary)
            .Append(' ')
            .Append(direction == OrderByDirection.Asc ? "ASC" : "DESC");

        return queryBuilder.ToString();
    }
}