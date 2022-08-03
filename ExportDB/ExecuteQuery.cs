using Oracle.ManagedDataAccess.Client;

namespace ExportDB
{
    public class ExecuteQuery
    {
        public static void ConvertCSV(string connectionString)
        {
            string fileName = @"c:\test\test2.csv";
            string queryString = "SELECT * FROM SAPDM_RES WHERE DATAID < 38200";
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand command = new OracleCommand(queryString, connection);
                connection.Open();
                OracleDataReader reader = command.ExecuteReader();

                StreamWriter sw = new StreamWriter(fileName);

                object[] output = new object[reader.FieldCount];

                for (int i = 0; i < reader.FieldCount; i++)
                    output[i] = reader.GetName(i);

                sw.WriteLine(string.Join(";", output));

                while (reader.Read())
                {
                    reader.GetValues(output);
                    sw.WriteLine(string.Join(";", output));
                }

                sw.Close();
                reader.Close();
            }
        }
    }
}
