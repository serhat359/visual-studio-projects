using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace TorrentSeedLeechCounter
{
    class Database
    {
        public static void Insert(TorrentAnnounceInfo parsedTorrentAnnounceInfo)
        {
            string query = "INSERT INTO [Casual].[dbo].[TorrentPeerState] ([TorrentHash], [Datetime], [Seed], [Leech], [Peers], [FullText], [Interval], [MinInterval])" + "\n" +
                "VALUES (@TorrentHash, @Datetime, @Seed, @Leech, @Peers, @FullText, @Interval, @MinInterval)";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("TorrentHash", parsedTorrentAnnounceInfo.TorrentHash));
            parameters.Add(new SqlParameter("Datetime", DateTime.Now));
            parameters.Add(new SqlParameter("Seed", parsedTorrentAnnounceInfo.Complete));
            parameters.Add(new SqlParameter("Leech", parsedTorrentAnnounceInfo.Incomplete));
            parameters.Add(new SqlParameter("Peers", parsedTorrentAnnounceInfo.Peers));
            parameters.Add(new SqlParameter("FullText", parsedTorrentAnnounceInfo.FullText));
            parameters.Add(new SqlParameter("Interval", parsedTorrentAnnounceInfo.Interval));
            parameters.Add(new SqlParameter("MinInterval", parsedTorrentAnnounceInfo.MinInterval));

            RunQueryUpdateOneRow(query, parameters);
        }

        public static bool CheckAvailability()
        {
            bool result;

            try
            {
                ConnectionStringSettings connectionStringSetting = GetConnectionString();

                SqlConnection connection = new SqlConnection(connectionStringSetting.ConnectionString);

                connection.Open();

                connection.Close();

                result = true;
            }
            catch (Exception)
            {

                result = false;
            }

            return result;
        }

        public static void RunQueryUpdateOneRow(string sql, List<SqlParameter> parameters)
        {
            ConnectionStringSettings connectionStringSetting = GetConnectionString();

            SqlConnection connection = new SqlConnection(connectionStringSetting.ConnectionString);

            connection.Open();

            var transaction = connection.BeginTransaction();

            var command = new SqlCommand(sql, connection);
            command.Transaction = transaction;

            foreach (SqlParameter parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            int numberOfRecords = command.ExecuteNonQuery();

            if (numberOfRecords == 1)
            {
                transaction.Commit();
                connection.Close();
                return;
            }
            else
            {
                transaction.Rollback();
                connection.Close();
                throw new Exception(string.Format("Affected number of rows was not as expected: {0}", numberOfRecords));
            }
        }

        private static ConnectionStringSettings GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["MSSQL"];
        }
    }
}
