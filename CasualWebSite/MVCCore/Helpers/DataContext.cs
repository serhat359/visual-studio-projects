using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace MVCCore.Helpers;

public class DataContext
{
    private readonly string connectionString;

    public DataContext(IConfiguration configuration)
    {
        this.connectionString = configuration.GetConnectionString("SqliteDatabase") ?? throw new Exception("Could not get config for SqliteDatabase");
    }

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(connectionString);
    }
}
