using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data.SqlClient;

namespace MapperTextlibrary
{
    public class DBConnection
    {
        public enum Mode
        {
            Read,
            Write,
            MSSQL
        }

        private DbConnection connection;
        private Mode mode;

        private const int CONNECTION_ERROR = 0;
        private const int INVALID_USERNAME_PASSWORD = 1045;

        public DBConnection(Mode mode)
        {
            this.mode = mode;
            Initialize(mode);
        }

        public LinkedList<T> RunSelectQuery<T>(string sql, IMapper<T> mapper) where T : new()
        {
            OpenConnection();

            DbCommand cmd;

            if (mode == Mode.MSSQL)
                cmd = new SqlCommand(sql, (SqlConnection)connection);
            else
                cmd = new MySqlCommand(sql, (MySqlConnection)connection);

            IDataReader dataReader = cmd.ExecuteReader();

            LinkedList<T> list = mapper.MapAll(dataReader);

            dataReader.Close();

            this.CloseConnection();

            return list;
        }

        private void Initialize(Mode mode)
        {
            ConnectionStringSettings connectionStringSetting = GetConnectionString(mode);

            if (mode == Mode.MSSQL)
                connection = new SqlConnection(connectionStringSetting.ConnectionString);
            else
                connection = new MySqlConnection(connectionStringSetting.ConnectionString);
        }

        private ConnectionStringSettings GetConnectionString(Mode mode)
        {
            switch (mode)
            {
                case Mode.Read:
                    return ConfigurationManager.ConnectionStrings["MySQLReaderConnection"];
                case Mode.Write:
                    return ConfigurationManager.ConnectionStrings["MySQLWriterConnection"];
                case Mode.MSSQL:
                    return ConfigurationManager.ConnectionStrings["MSSQL"];
                default:
                    throw new Exception("Invalid operation mode");
            }
        }

        private void OpenConnection()
        {
            try
            {
                connection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case CONNECTION_ERROR:
                        throw new Exception("Cannot connect to server.  Contact administrator", ex);
                    case INVALID_USERNAME_PASSWORD:
                        throw new Exception("Invalid username/password, please try again", ex);
                }
            }
        }

        private void CloseConnection()
        {
            try
            {
                connection.Close();
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error when closing the connection", ex);
            }
        }
    }
}
