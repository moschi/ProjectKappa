using ProjectKappa.Base.WPF;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace ProjectKappa.Base
{
    public enum LogLevel
    {
        TRACE = 0,
        DEBUG = 1,
        INFORMATION = 2,
        WARNING = 3,
        ERROR = 4,
        CRITICAL = 5,
        NONE = 6
    }

    public class Log : BasePropertyChanged
    {
        static Log()
        {
            _StaticLog = new Log();
        }

        private static Log _StaticLog { get; set; }
        public static Log StaticLog { get => _StaticLog; }

        private Log()
        {
            Entries = new ObservableCollection<LogEntry>();
        }

        public ObservableCollection<LogEntry> Entries
        {
            get => GetValue<ObservableCollection<LogEntry>>();
            set => SetValue(value);
        }

        public async void AddEntry(LogEntry entry)
        {
            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                Entries.Add(entry);
            });
        }

        public void AddEntry(LogLevel logLevel, string sender, string message)
        {
            AddEntry(LogEntry.Entry(logLevel, sender, message));
        }

        //public BaseCommand SaveCommand
        //{
        //    get => GetValue<BaseCommand>();
        //    set => SetValue(value);
        //}
    }

    public class LogEntry : BasePropertyChanged
    {
        public LogEntry()
        {
            TimeStamp = DateTime.Now;
        }

        public DateTime TimeStamp
        {
            get => GetStructOrDefaultValue<DateTime>();
            set => SetValue(value);
        }

        public string Message
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public LogLevel LogLevel
        {
            get => GetStructOrDefaultValue<LogLevel>();
            set => SetValue(value);
        }

        public string Sender
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public override string ToString()
        {
            return $"[{TimeStamp.ToLongTimeString()}]\t[{LogLevel.ToString()}]\t[{Sender}]\t\t\t{Message}";
        }

        public static LogEntry Entry(LogLevel logLevel, string sender, string message)
        {
            return new LogEntry()
            {
                LogLevel = logLevel,
                Sender = sender,
                Message = message
            };
        }

        public static LogEntry TraceEntry(string sender, string message) => Entry(LogLevel.TRACE, sender, message);

        public static LogEntry DebugEntry(string sender, string message) => Entry(LogLevel.DEBUG, sender, message);

        public static LogEntry InformationEntry(string sender, string message) => Entry(LogLevel.INFORMATION, sender, message);

        public static LogEntry WarningEntry(string sender, string message) => Entry(LogLevel.WARNING, sender, message);

        public static LogEntry ErrorEntry(string sender, string message) => Entry(LogLevel.ERROR, sender, message);

        public static LogEntry CriticalEntry(string sender, string message) => Entry(LogLevel.CRITICAL, sender, message);
    }
}
