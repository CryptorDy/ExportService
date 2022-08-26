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
using NLog;

namespace Export
{
    public class QueryOracle : IQuery
    {
        /// <summary>
        /// Максимальное количество записей получаемых за одно обращение к БД
        /// </summary>
        private int _maxDataCount = 500000;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        object[][] data;

        public object[][] Execute(string query, int dataCount)
        {
            
            logger.Debug($"Начало получения данных по запросу: {query}");

            OracleDataReader reader;
            var connectionString = ConfigurationManager.ConnectionStrings["OTCS"].ConnectionString;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand command = new OracleCommand(query, connection);
                connection.Open();
                reader = command.ExecuteReader();

                reader.FetchSize = reader.RowSize * _maxDataCount;

                object[] output = new object[reader.FieldCount];
                InitData(dataCount);

                Stopwatch sws = new Stopwatch();
                sws.Start();

                for (int i = 0; i < reader.FieldCount; i++)
                    output[i] = reader.GetName(i);

                int j = 0;
                data[j] = output;

                while (reader.Read())
                {
                    j++;

                    CheckResizeArray(j);

                    data[j] = new object[reader.FieldCount];
                    reader.GetValues(data[j]);
                }

                //Удаляем все пустые ячейки
                data = data.Where(c => c != null).ToArray();

                sws.Stop();

                logger.Debug($"Получено {data.Length} строк за {sws.ElapsedMilliseconds} милисекунд");

            }
            return data;
        }

        /// <summary>
        /// Проверка индекса на превышение длины массива
        /// </summary>
        private void CheckResizeArray(int index)
        {
            if (index > data.Length - 1)
                Array.Resize(ref data, data.Length + 500000);
        }

        /// <summary>
        /// Инициализация массива данных
        /// </summary>
        /// <param name="count">Количество значений</param>
        private void InitData(int count)
        {
            if (count == 0)
                count = 1000000;

            data = new object[count + 1][];
        }

    }
}
