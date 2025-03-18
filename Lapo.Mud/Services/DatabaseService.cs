using System.Data;
using System.Text;
using Dapper;
using Lapo.Mud.Enums;
using Lapo.Mud.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Lapo.Mud.Services;

public class DatabaseService(IConfiguration configuration, IOptions<AluOptions> options)
{
    readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration), "Connection string not found.");
    readonly string _primaryKeyColumn = options.Value.PrimaryKeyColumn;
    
    public async Task<DataTable> QueryAsync(string table, string? where = null, int? top = null, OrderByDirection direction = OrderByDirection.Desc)
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

        if (string.IsNullOrEmpty(primaryKey))
            throw new KeyNotFoundException($"Primary key not found for the table '{table}'.");

        var queryBuilder = BuildQuery(table, primaryKey, direction, top, where);
        await using var reader = await connection.ExecuteReaderAsync(queryBuilder);

        var dataTable = new DataTable();
        dataTable.Load(reader);
        
        dataTable.Columns.Add(_primaryKeyColumn, typeof(int));
        dataTable.Columns[_primaryKeyColumn]?.SetOrdinal(0);
        foreach (DataRow row in dataTable.Rows) row[_primaryKeyColumn] = row[primaryKey];

        return dataTable;
    }
    
    static string BuildQuery(string table, string primary, OrderByDirection direction, int? top = null,
        string? where = null)
    {
        var queryBuilder = new StringBuilder();

        queryBuilder
            .Append("SELECT")
            .Append(' ');

        // if (top.HasValue)
        //     queryBuilder
        //         .Append($"TOP ({top.Value})")
        //         .Append(' ');

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

        // queryBuilder
        //     .Append("ORDER BY")
        //     .Append(' ')
        //     .Append(primary)
        //     .Append(' ')
        //     .Append(direction == OrderByDirection.Asc ? "ASC" : "DESC");

        return queryBuilder.ToString();
    }
}