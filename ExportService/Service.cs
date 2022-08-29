using System;
using System.ServiceProcess;
using System.Timers;
using Export;
using NLog;

namespace ExportService
{
    /// <summary>
    /// Сервис регулярного выполнение экспорта
    /// </summary>
    public partial class Service : ServiceBase
    {
        /// <summary>
        /// Сервис экспорта данных
        /// </summary>
        private readonly IExport export;

        /// <summary>
        /// Таймер переодичности выполнения экспорта
        /// </summary>
        Timer m_timer;

        /// <summary>
        /// Сервис логгирования
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Service(IExport export)
        {
            this.export = export;

            InitializeComponent();

            try
            {
                ExecuteExport();
            } catch (Exception ex)
            {
                logger.Error(ex);
            }
            
        }

        /// <summary>
        /// Выполнить экспорт данных
        /// </summary>
        private void ExecuteExport()
        {
            var settingExecution = new SettingManager().GetSetting();
            var queries = new SettingManager().GetSettingQueries();

            foreach (var query in queries)
            {
                var setting = CreateFillSetting(query, settingExecution);

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

                //Выполнить сейчас
                if (settingExecution.NowExport.On)
                    Export(query.SqlQuery, setting);
            }
        }

        /// <summary>
        /// Выполнение по заданному интервалу (по часам)
        /// </summary>
        private void ExecuteByHour(int hour, string query, Setting setting)
        {
            m_timer.Interval = hour * 1000 * 60 * 60;

            logger.Debug($"Выполнение в интервале {hour} час");

            m_timer.Elapsed += new ElapsedEventHandler((s, e) => Export(query, setting));
        }

        /// <summary>
        /// Выполнение в заданное время через установленное количество дней
        /// </summary>
        private void ExecuteByDay(int day, string time, string query, Setting setting)
        {
            //Если сегодняшнее время прошло, то выполнить через указанное кол-во дней
            if (GetMillisecondsNextTime(0, time) > 0)
                m_timer.Interval = GetMillisecondsNextTime(0, time);
            else
                m_timer.Interval = GetMillisecondsNextTime(day, time);


            logger.Debug($"Выполнение в {time} каждый {day} день");

            m_timer.Elapsed += new ElapsedEventHandler((s, e) => ExecuteAndUpdateInterval(day, time, query, setting));
        }

        /// <summary>
        /// Выполнить и обновить интервал
        /// </summary>
        private void ExecuteAndUpdateInterval(int day, string time, string query, Setting setting)
        {
            m_timer.Interval = GetMillisecondsNextTime(day, time);

            Export(query, setting);
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

        /// <summary>
        /// Выполнение экспорта и логгирование результата
        /// </summary>
        private void Export(string query, Setting setting)
        {
            var success = export.Execute(query, setting);

            if (success)
                logger.Debug($"Экспорт файла {setting.FileName} выполнен успешно");
            else
                logger.Debug($"Не удалось экспортировать данные для файла {setting.FileName}");
        }

        /// <summary>
        /// Создание и заполнение Setting
        /// </summary>
        /// <param name="table">Настройки запроса</param>
        /// <param name="settingExecution">Настройки регулярности выполнения</param>
        /// <returns>Объект Setting</returns>
        private Setting CreateFillSetting(SettingQuery table, SettingExecution settingExecution)
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

            return setting;
        }


        protected override void OnStart(string[] args)
        {
            
        }

        protected override void OnStop()
        {
        }
    }
}
