using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace MVCCore.Helpers;

public class DataContext
{
    private readonly string connectionString;

    public DataContext(IConfiguration configuration)
    {
        this.connectionString = configuration.GetConnectionString("SqliteDatabase");
    }

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(connectionString);
    }
}
