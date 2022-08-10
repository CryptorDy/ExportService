using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExportService
{
    public class SettingManager
    {

        ExportSetting _setting;
        public SettingManager()
        {
            _setting = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).SectionGroups["ExportSetting"] as ExportSetting;

            if (_setting is null)
                throw new Exception("Ошибка чтения конфигурационного файла");
        }

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

        public List<SettingQuery> GetSettingTable()
        {
            var tables = new List<SettingQuery>();
            foreach (ConfigurationSection section in _setting.Sections)
            {
                if (section.GetType() == typeof(TableSection))
                {
                    tables.Add(BuilderSqlQuery(section));
                }
            }
            return tables;
        }

        private ExecuteByDay GetExecuteByDay(ConfigurationSection section, out bool IsExecuteByDay)
        {
            var executeByDay = section as ExecuteByDay;
            if(executeByDay.Day == 0)
            {
                IsExecuteByDay = false;
                return new ExecuteByDay();
            }

            IsExecuteByDay = true;
            return new ExecuteByDay { Day = executeByDay.Day, Time = executeByDay.Time };
        }

        private ExecuteByHour GetExecuteByHour(ConfigurationSection section, out bool IsExecuteByHour)
        {
            var executeByHour = section as ExecuteByHour;
            if(executeByHour.Hour == 0)
            {
                IsExecuteByHour = false;
                return new ExecuteByHour();
            }

            IsExecuteByHour = true;
            return new ExecuteByHour { Hour = executeByHour.Hour };
        }

        private SettingQuery BuilderSqlQuery(ConfigurationSection section)
        {
            TableSection tableSetting = section as TableSection;
            
            var name = tableSetting.Name;
            var select = tableSetting.Select != string.Empty ? tableSetting.Select : "*";
            var where = tableSetting.Where;
            var orderBy = tableSetting.OrderBy;
            var limit = tableSetting.Limit;

            CheckWord(name);
            CheckSelect(select);

            var query = $"Select {select} from {name}";

            query += BuildWhereStr(where);

            query += BuildOrderByStr(orderBy);

            

            if (limit != 0)
                query = $"Select {select} from ({query}) where ROWNUM <= " + limit;

            return new SettingQuery
            {
                SqlQuery = query,
                FileName = tableSetting.File.Name,
                Folder = tableSetting.Folder,
                Records = tableSetting.File.Records,
                DataCount = tableSetting.Limit
            };
        }

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
                    if (CheckWhere(item.Condition))
                        query += item.Condition;

                    i++;
                }
            }
            return query;
        }

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

            if (!Regex.Match(str, @"^[a-zA-Z_\s]{1,20}$").Success)
                throw new Exception($"Строка {str} содержит недопустимые символы");

            return true;
        }

        private bool CheckWhere(string str)
        {
            if (SqlKeywords().Any(c => str.Contains(c)))
                throw new Exception($"Строка {str} содержит недопустимые ключевые слова");

            if (!Regex.Match(str, @"^[a-zA-Z\s]+[\=|>=|<=]{1,2}[\sa-zA-Z0-9]{1,}$").Success)
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
