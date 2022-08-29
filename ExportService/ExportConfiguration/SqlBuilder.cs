using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExportService
{
    /// <summary>
    /// Формирование sql запроса по настройкам конфигурационного файла
    /// </summary>
    public class SqlBuilder
    {
        public string Builde(TableSection tableSetting)
        {
            var name = tableSetting.Name;
            var select = tableSetting.Select != string.Empty ? tableSetting.Select : "*";
            var where = tableSetting.Where;
            var orderBy = tableSetting.OrderBy;
            var limit = tableSetting.Limit.Value;

            CheckWord(name);
            CheckSelect(select);

            var query = $"Select {select} from {name}";

            query += BuildWhereStr(where);

            query += BuildOrderByStr(orderBy);

            if (limit > 0)
                query = $"Select {select} from ({query}) where ROWNUM <= " + limit;

            return query;
        }

        /// <summary>
        /// Формирование части Where строки запроса
        /// </summary>
        /// <param name="where">Настройки Where</param>
        /// <returns>Строка части условий Where</returns>
        private string BuildWhereStr(WhereElementCollection where)
        {
            var query = string.Empty;

            if (where == null || where.Count == 0)
                return String.Empty;
            
            query += $" where ";
            int i = 0;
            foreach (WhereElement item in where)
            {
                if (string.IsNullOrWhiteSpace(item.Column) ||
                    string.IsNullOrWhiteSpace(item.Operator) ||
                    string.IsNullOrWhiteSpace(item.Value))
                throw new Exception("Заполните все обязательные поля объекта Where");

                //Если условия проверки не первая, то сначало добавить AND или OR
                if (i > 0)
                {
                    if (item.And)
                        query += " AND ";
                    else query += " OR ";
                }
                
                var oper = CheckOperator(item.Operator);

                //Формирование строки
                if (CheckWord(item.Column) && CheckValue(item.Value))
                    query += $"{item.Column} {oper} \'{item.Value}\'"; 

                i++;
            }
            
            return query;
        }

        /// <summary>
        /// Формирование части OrderBy строки запроса
        /// </summary>
        /// <param name="orderBy">Настройки OrderBy</param>
        /// <returns>Строки части сортировки OrderBy</returns>
        private string BuildOrderByStr(OrderByElementCollection orderBy)
        {
            var query = string.Empty;

            if (orderBy == null || orderBy.Count == 0)
                return String.Empty;
            
            query += $" order by";
            int i = 0;
            foreach (OrderByElement item in orderBy)
            {
                if (i != 0)
                    query += ",";

                if (string.IsNullOrWhiteSpace(item.Column))
                    throw new Exception("Заполните все обязательные поля объекта OrderBy");

                string desc = item.Desc ? "DESC" : string.Empty;

                if (CheckWord(item.Column))
                    query += $" {item.Column} {desc}";
                
                i++;
            }
            
            return query;
        }

        /// <summary>
        /// Проверка названия слова на недопустимые ключевые слова и символы
        /// </summary>
        /// <param name="str">Строка для проверки</param>
        /// <returns>True - если прошел проверку, иначе False</returns>
        private bool CheckWord(string str)
        {
            if (SqlKeywords().Any(c => str.Equals(c.Trim())))
                throw new Exception($"Строка {str} содержит недопустимые ключевые слова: {SqlKeywords().Where(c => str.Contains(c)).FirstOrDefault()}");

            if (!Regex.Match(str, @"^[a-zA-Z0-9_]{1,25}$").Success)
                throw new Exception($"Строка {str} содержит недопустимые символы");

            return true;
        }

        /// <summary>
        /// Проверка оператора сравнения на формат
        /// </summary>
        /// <param name="oper">Оператор сравнения</param>
        /// <returns>Замененная безопасная строка</returns>
        private string CheckOperator(string oper)
        {
            switch (oper)
            {
                case "=":
                    return oper;
                case ">=":
                    return oper;
                case ">":
                    return oper;
                case "^":
                    return "<";
                case "^=":
                    return "<=";
                default: throw new Exception($"Оператор {oper} содержит недопустимые символы");
            }
        }

        /// <summary>
        /// Проверка значения на недопустимые ключевые слова и символы
        /// </summary>
        /// <param name="str">Значение проверки</param>
        /// <returns>True - если прошел проверку, иначе False</returns>
        private bool CheckValue(string str)
        {
            if (SqlKeywords().Any(c => str.Contains(c)))
                throw new Exception($"Строка {str} содержит недопустимые ключевые слова: {SqlKeywords().Where(c => str.Contains(c)).FirstOrDefault()}");

            if (Regex.Match(str, @"[;\-']").Success)
                throw new Exception($"Строка {str} содержит недопустимые символы");

            return true;
        }

        /// <summary>
        /// Проверка строки Select на недопустимые ключевые слова и символы
        /// </summary>
        /// <param name="str">Строка select</param>
        /// <returns>True - если прошел проверку, иначе False</returns>
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

        /// <summary>
        /// Ключевые слова SQL
        /// </summary>
        /// <returns>Массив</returns>
        private string[] SqlKeywords()
        {
            return new string[] {
                "ALTER",
                "DROP",
                "TABLE",
                "CREATE",
                "DELETE",
                "INSERT",
            };
        }
    }
}
