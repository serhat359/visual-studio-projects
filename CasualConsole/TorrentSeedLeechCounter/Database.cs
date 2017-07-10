using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace TorrentSeedLeechCounter
{
    public class Database
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

        public static void UpdateCorrected(TorrentPeerState torrentPeerState)
        {
            string query = "UPDATE TorrentPeerState" + "\n" +
                "SET SeedCorrected = @CorrectedSeed, LeechCorrected = @CorrectedLeech" + "\n" +
                "WHERE ID = @ID";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", torrentPeerState.ID));
            parameters.Add(new SqlParameter("CorrectedSeed", NullCheck(torrentPeerState.SeedCorrected)));
            parameters.Add(new SqlParameter("CorrectedLeech", NullCheck(torrentPeerState.LeechCorrected)));

            RunQueryUpdateOneRow(query, parameters);
        }

        public static object NullCheck(object obj)
        {
            if (obj != null)
                return obj;
            else
                return DBNull.Value;
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

        public static List<T> RunSelectQuery<T>(string sql) where T : new()
        {
            ConnectionStringSettings connectionStringSetting = GetConnectionString();

            SqlConnection connection = new SqlConnection(connectionStringSetting.ConnectionString);

            connection.Open();

            DbCommand cmd = new SqlCommand(sql, (SqlConnection)connection);

            IDataReader dataReader = cmd.ExecuteReader();

            List<T> list = new ExpressionTreeMapperAs<T>().MapAll(dataReader);

            dataReader.Close();

            connection.Close();

            return list;
        }

        private static ConnectionStringSettings GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["MSSQL"];
        }
    }
}
