using System;
using System.Collections.Generic;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;
using System.Diagnostics;

namespace Export
{
    public class QueryOracle : IQuery
    {
        public object[][] Execute(string query)
        {
            object[][] data; 

            OracleDataReader reader;
            var connectionString = ConfigurationManager.ConnectionStrings["OTCS"].ConnectionString;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand command = new OracleCommand(query, connection);
                connection.Open();
                reader = command.ExecuteReader();

                object[] output = new object[reader.FieldCount];
                data = new object[reader.FetchSize][];

                Stopwatch sws = new Stopwatch();
                sws.Start();

                for (int i = 0; i < reader.FieldCount; i++)
                    output[i] = reader.GetName(i);

                int j = 1;
                while (reader.Read())
                {
                    reader.GetValues(output);
                    data[j] = output;
                    j++;
                }

                sws.Stop();

                Console.WriteLine(sws.ElapsedTicks + " " + sws.ElapsedMilliseconds);

            }
            return data;
        }

        
    }
}
