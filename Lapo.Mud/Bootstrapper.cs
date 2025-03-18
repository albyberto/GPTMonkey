using Lapo.Mud.Options;
using Lapo.Mud.Services;

namespace Lapo.Mud;

public static class Bootstrapper
{
    public static void AddOptions(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<AluOptions>().Bind(builder.Configuration.GetSection("Configuration:Columns"));
        builder.Services.AddOptions<CsvOptions>().Bind(builder.Configuration.GetSection("Configuration:Csv"));
        builder.Services.AddOptions<ConfigurationOptions>().Bind(builder.Configuration.GetSection("Configuration:Configurations"));
    }

    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ConfigurationService>();
        builder.Services.AddSingleton<CsvService>();
        builder.Services.AddSingleton<MarkdownService>();
        builder.Services.AddSingleton<TableService>();
        builder.Services.AddScoped<DatabaseService>();
        builder.Services.AddScoped<AluService>();
    }
}