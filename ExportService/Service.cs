using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;
using Export;

namespace ExportService
{
    public partial class Service : ServiceBase
    {

        private readonly IExport export;

        Timer m_timer;

        public Service(IExport export)
        {
            this.export = export;

            InitializeComponent();

            ExecuteExport();
        }


        private void ExecuteExport()
        {
            var settingExecution = new SettingManager().GetSetting();
            var tables = new SettingManager().GetSettingTable();


            foreach (var table in tables)
            {
                Setting setting = new Setting();
                setting.FileName = table.FileName;
                setting.Records = table.Records;
                setting.DataCount = table.DataCount;
                setting.Folder = table.Folder;
                setting.Path = settingExecution.ExportPath.Path;
                setting.Sftp = settingExecution.ExportPath.Sftp;
                setting.Host = settingExecution.ExportPath.Host;
                setting.Login = settingExecution.ExportPath.Login;
                setting.Pass = settingExecution.ExportPath.Pass;

                if (settingExecution.NowExport.On)
                    new System.Threading.Thread(() => export.Execute(table.SqlQuery, setting)).Start();

                m_timer = new Timer();

                if (settingExecution.IsExecuteByHour)
                {
                    ExecuteByHour(settingExecution.ExecuteByHour.Hour, table.SqlQuery, setting);
                }
                else if (settingExecution.IsExecuteByDay)
                {
                    ExecuteByDay(settingExecution.ExecuteByDay.Day, settingExecution.ExecuteByDay.Time, table.SqlQuery, setting);
                }
                m_timer.Start();
            }
        }

        /// <summary>
        /// Выполнение по заданному интервалу (по часам)
        /// </summary>
        private void ExecuteByHour(int hour, string query, Setting setting)
        {
            m_timer.Interval = hour * 1000 * 60 * 60;

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


        protected override void OnStart(string[] args)
        {
            
        }

        protected override void OnStop()
        {
        }
    }
}
