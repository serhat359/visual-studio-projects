using System.Collections.Generic;

namespace Data
{
    public class DataFactory
    {
        public static List<T> RunSelectQuery<T>(string sql) where T : new()
        {
            DBConnection DBConnection = new DBConnection(DBConnection.Mode.Read);

            List<T> list = DBConnection.RunSelectQuery<T>(sql);

            return list;
        }
    }
}
