using Export;
using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Unity;

namespace ExportService
{
    /// <summary>
    /// Выполнение экспорта данных
    /// </summary>
    public class ExecuteExport
    {
        private readonly IExport export;

        Timer m_timer;

        public ExecuteExport(IExport export)
        {
            this.export = export;


            var settingExecution = new SettingManager().GetSetting();
            var Queries = new SettingManager().GetSettingTable();

            foreach (var query in Queries)
            {
                Setting setting = new Setting();
                setting.FileName = query.FileName;
                setting.Records = query.Records;
                setting.Folder = query.Folder;
                setting.Path = settingExecution.ExportPath.Path;
                setting.Sftp = settingExecution.ExportPath.Sftp;
                setting.Host = settingExecution.ExportPath.Host;
                setting.Login = settingExecution.ExportPath.Login;
                setting.Pass = settingExecution.ExportPath.Login;

                if (settingExecution.NowExport.On)
                    new System.Threading.Thread(() => export.Execute(query.SqlQuery, setting));

                m_timer = new Timer();

                if (settingExecution.IsExecuteByHour)
                {
                    ExecuteByHour(settingExecution.ExecuteByHour.Hour, query.SqlQuery, setting);
                }
                else if (settingExecution.IsExecuteByDay)
                {
                    ExecuteByDay(settingExecution.ExecuteByDay.Day, settingExecution.ExecuteByDay.Time, query.SqlQuery, setting);
                }
                m_timer.Start();
            }
        }

        /// <summary>
        /// Выполнение по заданному интервалу (по часам)
        /// </summary>
        private void ExecuteByHour(int hour, string query, Setting setting)
        {
            m_timer.Interval = hour * 1000 * 1000 * 60;

            m_timer.Elapsed += new ElapsedEventHandler((s, e) => export.Execute(query, setting));
        }

        /// <summary>
        /// Выполнение в заданное время через установленное количество дней
        /// </summary>
        private void ExecuteByDay(int day, string time, string query, Setting setting)
        {
            m_timer.Interval = GetMillisecondsNextTime(day, time);

            m_timer.Elapsed += new ElapsedEventHandler((s, e) => ExecuteAndUpdateInterval(day, time, query, setting));
        }

        /// <summary>
        /// Выполнить и обновить интервал
        /// </summary>
        private void ExecuteAndUpdateInterval(int day, string time, string query, Setting setting)
        {
            m_timer.Interval = GetMillisecondsNextTime(day, time);

            export.Execute(query, setting);
        }

        /// <summary>
        /// Получить миллисекуды до следующего выполнения с текущего времени
        /// </summary>
        /// <param name="day">Количество дней между выполнением</param>
        /// <param name="time">Время выполнения</param>
        /// <returns>Миллисекунды</returns>
        private double GetMillisecondsNextTime(int day, string time)
        {
            return (DateTime.Parse(time).AddDays(day) - DateTime.Now).TotalMilliseconds;
        }
    }
}
