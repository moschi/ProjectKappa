using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ProjectKappa.LasToolsAPI;
using ProjectKappa.ViewModels;
using System.Timers;

namespace ProjectKappa
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        MainWindowViewModel ViewModel { get => (MainWindowViewModel)DataContext; }

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private async void Init()
        {
            ViewModel.SetDialogCoordinator(DialogCoordinator.Instance);
            if (ViewModel.Settings.LASFilesRoot.Equals("unset"))
            {
                string result = await this.ShowInputAsync("LAS files root directory not set", "The directory root for the LAS files is currently not set. Please enter a path:");
                ViewModel.Settings.LASFilesRoot = result;
                ViewModel.Settings.SyncCommand.Execute(null);
            }

            if (ViewModel.Settings.LAStoolsRoot.Equals("unset"))
            {
                string result = await this.ShowInputAsync("LAS tools root directory not set", "The directory root for the LAS Tools is currently not set. Please enter a path:");
                ViewModel.Settings.LAStoolsRoot = result;
                ViewModel.Settings.SyncCommand.Execute(null);
            }

            if (ViewModel.Settings.QGISRoot.Equals("unset"))
            {
                string result = await this.ShowInputAsync("QGIS root directory not set", "The directory root for QGIS is currently not set. Please enter a path:");
                ViewModel.Settings.QGISRoot = result;
                ViewModel.Settings.SyncCommand.Execute(null);
            }

            CLICalls.EnsureQGISEnvVars();
            ViewModel.UpdateConsoleOut("Ready.");
        }
    }
}
