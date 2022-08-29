using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExportService
{
    /// <summary>
    /// Менеджер конфигурационных настроек
    /// </summary>
    public class SettingManager
    {

        ExportSetting _setting;
        public SettingManager()
        {
            _setting = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).SectionGroups["ExportSetting"] as ExportSetting;

            if (_setting is null)
                throw new Exception("Ошибка чтения конфигурационного файла");
        }

        /// <summary>
        /// Получить настройки выполнения
        /// </summary>
        public SettingExecution GetSetting()
        {
            var setting = new SettingExecution();
            var IsExecuteByHour = false;
            var IsExecuteByDay = false;

            foreach (ConfigurationSection section in _setting.Sections)
            {
                if (section.GetType() == typeof(ExecuteByDay))
                {
                    setting.ExecuteByDay = GetExecuteByDay(section, out IsExecuteByDay);
                } 
                else if (section.GetType() == typeof(ExecuteByHour))
                {
                    setting.ExecuteByHour = GetExecuteByHour(section, out IsExecuteByHour);
                }
                else if (section.GetType() == typeof(ExportPath))
                {
                    setting.ExportPath = section as ExportPath;
                }
                else if (section.GetType() == typeof(NowExport))
                {
                    setting.NowExport = section as NowExport;
                }
            }

            //Выполнение по часам имеет приоритет
            if (IsExecuteByHour)
            {
                setting.IsExecuteByHour = IsExecuteByHour;
                setting.IsExecuteByDay = false;
            }
            else
                setting.IsExecuteByDay = IsExecuteByDay;


            if (!setting.IsExecuteByDay && !setting.IsExecuteByHour)
                throw new Exception("Необходимо указать переодичность выполнения");

            return setting;
        }

        /// <summary>
        /// Получить коллекцию с настройками запросов
        /// </summary>
        public List<SettingQuery> GetSettingQueries()
        {
            var queries = new List<SettingQuery>();
            foreach (ConfigurationSection section in _setting.Sections)
            {
                if (section.GetType() == typeof(TableSection))
                {
                    queries.Add(GetSettingQuery(section));
                }
            }
            return queries;
        }

        /// <summary>
        /// Получить настройки выполнения по дням
        /// </summary>
        /// <param name="section">Секция данных</param>
        /// <param name="IsExecuteByDay">Выходной аргумент. Включен выполнение по дням</param>
        private ExecuteByDay GetExecuteByDay(ConfigurationSection section, out bool IsExecuteByDay)
        {
            var executeByDay = section as ExecuteByDay;
            if(executeByDay.Day == 0)
            {
                IsExecuteByDay = false;
                return new ExecuteByDay();
            }

            IsExecuteByDay = true;
            return executeByDay;
        }

        /// <summary>
        /// Получить настройки выполнения по часам
        /// </summary>
        /// <param name="section">Секция данных</param>
        /// <param name="IsExecuteByHour">Выходной аргумент. Включен выполнение по часам</param>
        private ExecuteByHour GetExecuteByHour(ConfigurationSection section, out bool IsExecuteByHour)
        {
            var executeByHour = section as ExecuteByHour;
            if(executeByHour.Hour == 0)
            {
                IsExecuteByHour = false;
                return new ExecuteByHour();
            }

            IsExecuteByHour = true;
            return executeByHour;
        }

        /// <summary>
        /// Получить настройки запроса
        /// </summary>
        /// <param name="section">Секция с конфигурационными настройками</param>
        /// <returns>Настройки запроса со стракой запроса</returns>
        private SettingQuery GetSettingQuery(ConfigurationSection section)
        {
            TableSection tableSetting = section as TableSection;

            var limit = tableSetting.Limit.Value;

            var query = new SqlBuilder().Builde(tableSetting);

            return new SettingQuery
            {
                SqlQuery = query,
                FileName = tableSetting.File.Name,
                Folder = tableSetting.Folder,
                Records = tableSetting.File.Records,
                DataCount = limit
            };
        }

        /// <summary>
        /// Формирование части Where
        /// </summary>
        /// <param name="where">Настройки Where</param>
        /// <returns>Строка части сортировки Where</returns>
        private string BuildWhereStr(WhereElementCollection where)
        {
            var query = string.Empty;
            if (where != null && where.Count > 0)
            {
                query += $" where ";
                int i = 0;
                foreach (WhereElement item in where)
                {
                    if (i != 0 && !string.IsNullOrWhiteSpace(item.Operator) && (item.Operator.Trim().Equals("OR") || item.Operator.Trim().Equals("AND")))
                        query += $" {item.Operator} ";
                    //if (CheckWhere(item.Condition))
                    //    query += item.Condition;

                    i++;
                }
            }
            return query;
        }

        /// <summary>
        /// Формирование части OrderBy
        /// </summary>
        /// <param name="orderBy">Настройки OrderBy</param>
        /// <returns>Строка части сортировки OrderBy</returns>
        private string BuildOrderByStr(OrderByElementCollection orderBy)
        {
            var query = string.Empty;
            if (orderBy != null && orderBy.Count > 0)
            {
                query += $" order by";
                int i = 0;
                foreach (OrderByElement item in orderBy)
                {
                    if (i != 0)
                        query += ",";

                    if (CheckWord(item.Column))
                    {
                        string desc = item.Desc ? "DESC" : string.Empty;
                        query += $" {item.Column} {desc}";
                    }
                    i++;
                }
            }

            return query;
        }

        private bool CheckWord(string str)
        {

            if (SqlKeywords().Any(c => str.Contains(c)))
                throw new Exception($"Строка {str} содержит недопустимые ключевые слова");

            if (!Regex.Match(str, @"^[a-zA-Z_]{1,25}$").Success)
                throw new Exception($"Строка {str} содержит недопустимые символы");

            return true;
        }

        private bool CheckWhere(string str)
        {

            var words = SplitToWords(str);

            if (SqlKeywords().Any(c => words.Any(x => x.Equals(c))))
                throw new Exception($"Строка {str} содержит недопустимые ключевые слова");

            if (!Regex.Match(str, @"^[a-zA-Z_\s]+[\=|>=|<=]{1,2}[\sa-zA-Z0-9]{1,}$").Success)
                throw new Exception($"Строка {str} содержит недопустимые символы");

            return true;
        }

        private bool CheckSelect(string str)
        {
            if (str.Equals("*"))
                return true;

            if (SqlKeywords().Any(c => str.Contains(c)))
                throw new Exception($"Строка {str} содержит недопустимые ключевые слова");

            if (!Regex.Match(str, @"^[a-zA-Z,\s]{1,}$").Success)
                throw new Exception($"Строка {str} содержит недопустимые символы");

            return true;
        }

        private string[] SplitToWords(string str)
        {
            str = str.Replace("  ", " ");
            str = str.Replace("   ", " ");

            return str.Split(new char[] { ' ' });
        }

        private string[] SqlKeywords()
        {
            return new string[] {
                "ADD",
                "CONSTRAINT",
                "ALL",
                "ALTER",
                "TABLE",
                "AND",
                "ANY",
                "AS",
                "ASC",
                "BACKUP DATABASE",
                "BETWEEN",
                "CASE",
                "CHECK",
                "COLUMN",
                "CONSTRAINT",
                "CREATE",
                "DATABASE",
                "PROCEDURE",
                "UNIQUE",
                "INDEX",
                "VIEW",
                "DEFAULT",
                "DELETE",
                "DESC",
                "DISTINCT",
                "DROP",
                "EXEC",
                "EXISTS",
                "FOREIGN",
                "KEY",
                "FROM",
                "FULL",
                "OUTER",
                "JOIN",
                "GROUP",
                "BY",
                "HAVING",
                "IN",
                "INNER",
                "INSERT",
                "INTO",
                "SELECT",
                "IS",
                "NULL",
                "NOT",
                "LIKE",
                "LIMIT",
                "OR",
                "AND",
                "ORDER",
                "PRIMARY",
                "PROCEDURE",
                "RIGHT",
                "LEFT",
                "TOP",
                "VALUES",
                "UPDATE",
                "ROWNUM",
                "SET"
            };
        }
    }
}
