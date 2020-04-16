using MahApps.Metro.Controls.Dialogs;
using ProjectKappa.Base;
using ProjectKappa.Base.WPF;
using ProjectKappa.LasToolsAPI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ProjectKappa.ViewModels
{
    public class MainWindowViewModel : BasePropertyChanged
    {
        public delegate void UpdateConsoleOutDelegate(string data);
        private IDialogCoordinator _DialogCoordinator;

        public MainWindowViewModel()
        {
            Settings = new SettingsViewModel();
            GamelandFolders = new ObservableCollection<GamelandFolder>();
            InitGamelandFoldersCommand = new BaseCommand((p) => CanExecuteInitGamelandFolders(), (p) => ExecuteInitGamelandFolders());
            ExecuteNextStepCommand = new BaseCommand((p) => CanExecuteNextStep(), (p) => ExecuteNextStep());
            Log.StaticLog.AddEntry(LogEntry.InformationEntry("Project Kappa", "Loaded MainWindowViewModel"));
            CLICalls.UpdateConsoleOut = UpdateConsoleOut;
            CurrentConsoleOut = string.Empty;
        }

        public void SetDialogCoordinator(IDialogCoordinator dialogCoordinator)
        {
            _DialogCoordinator = dialogCoordinator;
        }

        public SettingsViewModel Settings
        {
            get => GetValue<SettingsViewModel>();
            set => SetValue(value);
        }

        public ObservableCollection<GamelandFolder> GamelandFolders
        {
            get => GetValue<ObservableCollection<GamelandFolder>>();
            set => SetValue(value);
        }

        public Log Log
        {
            get => Log.StaticLog;
        }

        public string CurrentConsoleOut
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public BaseCommand InitGamelandFoldersCommand
        {
            get => GetValue<BaseCommand>();
            set => SetValue(value);
        }

        private bool CanExecuteInitGamelandFolders()
        {
            return GamelandFolders.Count == 0 && !Settings.LASFilesRoot.Equals("unset");
        }

        private void ExecuteInitGamelandFolders()
        {
            foreach (var directory in Directory.EnumerateDirectories(Settings.LASFilesRoot))
            {
                GamelandFolders.Add(new GamelandFolder(directory));
            }
            Log.StaticLog.AddEntry(LogEntry.InformationEntry("Project Kappa", "Initialized Gameland folders"));
        }

        public BaseCommand ExecuteNextStepCommand
        {
            get => GetValue<BaseCommand>();
            set => SetValue(value);
        }

        private bool CanExecuteNextStep()
        {
            return !CurrentlyWorking && GamelandFolders.Count > 0 && !Settings.LAStoolsRoot.Equals("unset") && StepCounter <= TotalSteps;
        }

        private int StepCounter = 1;
        private int TotalSteps = 5;
        private async void ExecuteNextStep()
        {
            try
            {
                CurrentConsoleOut = string.Empty;
                ProgressDialogController controller = await _DialogCoordinator.ShowProgressAsync(this, $"Working on Step {StepCounter++} of {TotalSteps}...", "Work is being done in the background...");
                controller.SetIndeterminate();
                controller.Minimum = 0;
                controller.Maximum = 1;
                await Task.Run(() =>
                {
                    CurrentlyWorking = true;
                    //for(int i = 0; i < GamelandFolders.Count; i++)
                    //{
                    //    controller.SetMessage($"Work is being done in the background...{Environment.NewLine}Item {i} of {GamelandFolders.Count}");
                    //    controller.SetProgress((double)i / GamelandFolders.Count);
                    //    GamelandFolders[i].ExecuteNextStepCommand.Execute(null);
                    //}
                    int finishedCount = 0;
                    object syncLock = new object();
                    Parallel.For(0, GamelandFolders.Count, (i) =>
                    {
                        GamelandFolders[i].ExecuteNextStepCommand.Execute(null);
                        lock (syncLock)
                        {
                            finishedCount++;
                        }
                        controller.SetMessage($"Work is being done in the background...{Environment.NewLine}Item {finishedCount} of {GamelandFolders.Count} finished");
                        controller.SetProgress((double)finishedCount / GamelandFolders.Count);
                    });
                    CurrentlyWorking = false;
                });
                await controller.CloseAsync();
            }
            finally
            {
                CurrentlyWorking = false;
            }

            // auto execute all steps
            if (CanExecuteNextStep())
            {
                ExecuteNextStep();
            }
        }

        public bool CurrentlyWorking
        {
            get => GetStructOrDefaultValue<bool>();
            set => SetValue(value);
        }

        public void UpdateConsoleOut(string data)
        {
            Dispatcher.CurrentDispatcher.Invoke(() => 
            {
                if(CurrentConsoleOut.Length > 5000)
                {
                    CurrentConsoleOut = string.Empty;
                }
                CurrentConsoleOut += data;
            });
        }
    }
}
