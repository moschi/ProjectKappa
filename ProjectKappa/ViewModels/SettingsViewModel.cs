using ProjectKappa.Base;
using ProjectKappa.Base.WPF;
using ProjectKappa.LasToolsAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectKappa.ViewModels
{
    public class SettingsViewModel : BasePropertyChanged
    {
        public SettingsViewModel()
        {
            SyncCommand = new BaseCommand((p) => true, (p) => Sync(true));
            LogLevels = new ObservableCollection<DisplayWrapper<LogLevel>>()
            {
                new DisplayWrapper<LogLevel>(LogLevel.TRACE, "Trace"),
                new DisplayWrapper<LogLevel>(LogLevel.DEBUG, "Debug"),
                new DisplayWrapper<LogLevel>(LogLevel.INFORMATION, "Information"),
                new DisplayWrapper<LogLevel>(LogLevel.WARNING, "Warning"),
                new DisplayWrapper<LogLevel>(LogLevel.ERROR, "Error"),
                new DisplayWrapper<LogLevel>(LogLevel.CRITICAL, "Critical"),
                new DisplayWrapper<LogLevel>(LogLevel.NONE, "None"),
            };
            Sync(false);
        }

        public string LAStoolsRoot
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string LASFilesRoot
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string QGISRoot
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public DisplayWrapper<LogLevel> Level
        {
            get => GetValue<DisplayWrapper<LogLevel>>();
            set => SetValue(value);
        }

        public bool RunParallel
        {
            get => GetStructOrDefaultValue<bool>();
            set => SetValue(value);
        }

        private void Sync(bool toSource = true)
        {
            if (toSource)
            {
                Properties.Settings.Default.LAStoolsRoot = LAStoolsRoot;
                Properties.Settings.Default.LASFilesRoot = LASFilesRoot;
                Properties.Settings.Default.QGISRoot = QGISRoot;
                Properties.Settings.Default.LogLevel = Level.Item.ToString();
                Properties.Settings.Default.RunParallel = RunParallel;

                Properties.Settings.Default.Save();
                Log.StaticLog.AddEntry(LogEntry.InformationEntry("Settings", "Saved settings settings"));
            }
            else
            {
                Properties.Settings.Default.Reload();

                LAStoolsRoot = Properties.Settings.Default.LAStoolsRoot;
                LASFilesRoot = Properties.Settings.Default.LASFilesRoot;
                QGISRoot = Properties.Settings.Default.QGISRoot;
                RunParallel = Properties.Settings.Default.RunParallel;
                if(!Enum.TryParse<LogLevel>(Properties.Settings.Default.LogLevel, out LogLevel tempRes))
                {
                    tempRes = LogLevel.WARNING;
                    MessageBox.Show("Settings file containes illegal value for LogLevel, defaulting to WARNING");
                }
                Level = LogLevels.Where(l => l.Item == tempRes).First();
                Log.StaticLog.AddEntry(LogEntry.InformationEntry("Settings", "Loaded settings"));
            }

            CLICalls.SetLAStoolsRootDir(LAStoolsRoot);
            CLICalls.SetQGISRootDir(QGISRoot);
        }

        public BaseCommand SyncCommand
        {
            get => GetValue<BaseCommand>();
            set => SetValue(value);
        }

        public ObservableCollection<DisplayWrapper<LogLevel>> LogLevels
        {
            get => GetValue<ObservableCollection<DisplayWrapper<LogLevel>>>();
            set => SetValue(value);
        }
    }
}
