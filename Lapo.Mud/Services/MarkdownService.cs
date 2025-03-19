using System.Data;
using System.Text;
using Lapo.Mud.Options;
using Microsoft.Extensions.Options;

namespace Lapo.Mud.Services;

public class MarkdownService
{
    readonly ILogger<MarkdownService> _logger;
    readonly string _path;

    public MarkdownService(IOptions<CsvOptions> options, ILogger<MarkdownService> logger)
    {
        _logger = logger;
        _path = Path.Combine(AppContext.BaseDirectory, $"{options.Value.Path}.md");

        _logger.LogInformation($"MarkdownService initialized with path '{_path}'.");
    }

    public async Task WriteAsync(Dictionary<string, List<DataRow>> diff)
    {
        var sb = new StringBuilder();

        foreach (var (key, data) in diff.OrderBy(x => x.Key))
        {
            if (data.Count == 0) continue;

            // Write table name as header
            sb.AppendLine($"# {key}");
            sb.AppendLine();

            // Write header
            sb.Append("| ");
            foreach (DataColumn column in data.First().Table.Columns)
            {
                sb.Append($"{column.ColumnName} | ");
            }
            sb.AppendLine();

            // Write separator
            sb.Append("| ");
            foreach (DataColumn column in data.First().Table.Columns)
            {
                sb.Append("--- | ");
            }
            sb.AppendLine();

            // Write rows
            foreach (var row in data)
            {
                sb.Append("| ");
                foreach (var item in row.ItemArray)
                {
                    sb.Append($"{item} | ");
                }
                sb.AppendLine();
            }

            sb.AppendLine();
        }

        await File.AppendAllTextAsync(_path, sb.ToString());
        _logger.LogInformation($"Markdown file written to '{_path}'.");
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