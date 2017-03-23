using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace Data
{
    public class DBConnection
    {
        public enum Mode
        {
            Read,
            Write
        }

        private MySqlConnection connection;

        private const int CONNECTION_ERROR = 0;
        private const int INVALID_USERNAME_PASSWORD = 1045;

        public DBConnection(Mode mode)
        {
            Initialize(mode);
        }

        public List<T> RunSelectQuery<T>(string sql) where T : new()
        {
            OpenConnection();

            DbCommand cmd = new MySqlCommand(sql, connection);

            IDataReader dataReader = cmd.ExecuteReader();

            List<T> list = Mapper.MapAllByExpression<T>(dataReader);

            dataReader.Close();

            this.CloseConnection();

            return list;
        }

        public void RunQueryUpdateOneRow(string sql)
        {
            OpenConnection();

            var trans = connection.BeginTransaction();

            DbCommand cmd = new MySqlCommand(sql, connection);

            int numberOfRecords = cmd.ExecuteNonQuery();

            if (numberOfRecords == 1)
            {
                trans.Commit();
                this.CloseConnection();
                return;
            }
            else
            {
                trans.Rollback();
                throw new Exception(string.Format("Affected number of rows was not expected: {0}", numberOfRecords));
            }
        }

        private void Initialize(Mode mode)
        {
            ConnectionStringSettings connectionStringSetting = GetConnectionString(mode);

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
