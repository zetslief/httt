using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace gen;

public static class DataContextHelpers
{
    public static void Configure(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
    {
        var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>();
        var databaseFullPath = Path.GetFullPath(databaseOptions.Value.DatabasePath);
        if (!File.Exists(databaseFullPath)) throw new InvalidOperationException($"Database file is not found: {databaseFullPath}");
        options.UseSqlite($"DataSource={databaseFullPath}");
    }
}