using ProjectKappa.Base;
using ProjectKappa.Base.WPF;
using ProjectKappa.LasToolsAPI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ProjectKappa.ViewModels
{
    public abstract class DirectoryTree : BasePropertyChanged
    {
        protected DirectoryTree(string rootDir)
        {
            RootDir = rootDir;
            GamelandID = new DirectoryInfo(RootDir).Name;
        }

        public string RootDir
        {
            get => GetValue<string>();
            private set => SetValue(value);
        }

        public string GamelandID
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public ObservableCollection<string> ContainedFiles { get; private set; }

        public virtual void ScanFiles()
        {
            ContainedFiles = new ObservableCollection<string>();
            Log.StaticLog.AddEntry(LogEntry.TraceEntry("DirectoryTree", "Scanning files"));
            foreach (var filepath in Directory.EnumerateFiles(GetDirPath()))
            {
                ContainedFiles.Add(filepath);
            }
        }

        public abstract void PrepareForExecution();
        public abstract string GetDirPath();

        public bool Finished
        {
            get => GetStructOrDefaultValue<bool>();
            set => SetValue(value);
        }

        public abstract IEnumerable<CliTask> GetTasks();
    }

    public class Las2LasDirTree : DirectoryTree
    {
        public Las2LasDirTree(string rootDir)
            : base(rootDir)
        {

        }

        public ObservableCollection<string> LAZFiles
        {
            get => GetValue<ObservableCollection<string>>();
            set => SetValue(value);
        }

        public override string GetDirPath()
        {
            return $"{RootDir}\\{GamelandFolder.Las2LasSuffix}";
        }

        public override IEnumerable<CliTask> GetTasks()
        {
            return new List<CliTask>()
            {
                new CliTask("Convert LAZ files to tiles", () =>
                {
                    string fileListName = $"file_list_LAZ_lastile.{GamelandID}.txt";
                    CLICalls.CreateListOfFiles(LAZFiles, fileListName);
                    CLICalls.CallLasTile(fileListName, $"{RootDir}\\{GamelandFolder.LasTileSuffix}");

                    // todo: make sure list of files isn't deleted prematurely
                    CLICalls.RemoveListOfFiles(fileListName);
                })
            };
        }

        public override void PrepareForExecution()
        {
            Directory.CreateDirectory($"{RootDir}\\{GamelandFolder.LasTileSuffix}");
        }

        public override void ScanFiles()
        {
            base.ScanFiles();
            LAZFiles = new ObservableCollection<string>();
            foreach (string file in ContainedFiles)
            {
                if (Path.GetExtension(file).EndsWith("laz"))
                {
                    LAZFiles.Add(file);
                }
                else
                {
                    Log.StaticLog.AddEntry(LogEntry.WarningEntry("Las2LasDirTree", $"Gameland\\las2las folder {GetDirPath()} containes file {file} which is not of the type .laz therefore be ignored when converting."));
                }
            }
        }
    }

    public class LasTileDirTree : DirectoryTree
    {
        public LasTileDirTree(string rootDir)
            : base(rootDir)
        {

        }

        public ObservableCollection<string> LAZFiles
        {
            get => GetValue<ObservableCollection<string>>();
            set => SetValue(value);
        }

        public override string GetDirPath()
        {
            return $"{RootDir}\\{GamelandFolder.LasTileSuffix}";
        }

        public override IEnumerable<CliTask> GetTasks()
        {
            return new List<CliTask>()
            {
                new CliTask("Blast LAZ tiles to DEM", () =>
                {
                    string fileListName = $"file_list_LAZ_blast2dem.{GamelandID}.txt";
                    CLICalls.CreateListOfFiles(LAZFiles, fileListName);
                    CLICalls.CallBlast2Dem(fileListName, $"{RootDir}\\{GamelandFolder.Blast2DemSuffix}");

                    // todo: make sure list of files isn't deleted prematurely
                    CLICalls.RemoveListOfFiles(fileListName);
                })
            };
        }

        public override void PrepareForExecution()
        {
            Directory.CreateDirectory($"{RootDir}\\{GamelandFolder.Blast2DemSuffix}");
        }

        public override void ScanFiles()
        {
            base.ScanFiles();
            LAZFiles = new ObservableCollection<string>();
            foreach (string file in ContainedFiles)
            {
                if (Path.GetExtension(file).EndsWith("laz"))
                {
                    LAZFiles.Add(file);
                }
                else
                {
                    Log.StaticLog.AddEntry(LogEntry.WarningEntry("LasTileDirTree", $"Gameland\\lastile folder {GetDirPath()} containes file {file} which is not of the type .laz therefore be ignored when converting."));
                }
            }
        }
    }

    // for possible future use to convert with QGIS
    public class Blast2demDirTree : DirectoryTree
    {
        public Blast2demDirTree(string rootDir)
            : base(rootDir)
        {

        }

        public override string GetDirPath()
        {
            return $"{RootDir}\\{GamelandFolder.Blast2DemSuffix}";
        }

        public ObservableCollection<string> TIFFiles
        {
            get => GetValue<ObservableCollection<string>>();
            set => SetValue(value);
        }

        public override IEnumerable<CliTask> GetTasks()
        {
            return new List<CliTask>()
            {
                new CliTask("QGIS", () =>
                {
                    string fileListName = $"file_list_TIF_qgis.{GamelandID}.txt";
                    CLICalls.CreateListOfFiles(TIFFiles, fileListName);
                    CLICalls.CallQGISMerger(fileListName, $"{RootDir}\\{GamelandFolder.QGISSuffix}\\{GamelandID}_DEM.tif");

                    // todo: make sure list of files isn't deleted prematurely
                    CLICalls.RemoveListOfFiles(fileListName);
                })
            };
        }

        public override void PrepareForExecution()
        {
            Directory.CreateDirectory($"{RootDir}\\{GamelandFolder.QGISSuffix}");
        }

        public override void ScanFiles()
        {
            base.ScanFiles();
            TIFFiles = new ObservableCollection<string>();
            foreach (string file in ContainedFiles)
            {
                if (Path.GetExtension(file).EndsWith("tif"))
                {
                    TIFFiles.Add(file);
                }
            }
        }
    }

    public class GamelandDirTree : DirectoryTree
    {
        public GamelandDirTree(string rootDir)
            : base(rootDir)
        {

        }

        public ObservableCollection<string> PANFiles
        {
            get => GetValue<ObservableCollection<string>>();
            set => SetValue(value);
        }

        public ObservableCollection<string> PASFiles
        {
            get => GetValue<ObservableCollection<string>>();
            set => SetValue(value);
        }

        public override string GetDirPath()
        {
            return RootDir;
        }

        public override void PrepareForExecution()
        {
            Directory.CreateDirectory($"{RootDir}\\{GamelandFolder.Las2LasSuffix}");
        }

        public override void ScanFiles()
        {
            base.ScanFiles();

            PANFiles = new ObservableCollection<string>();
            PASFiles = new ObservableCollection<string>();
            foreach (var file in ContainedFiles)
            {
                if (file.EndsWith("PAS.las"))
                {
                    PASFiles.Add(file);
                }
                else if (file.EndsWith("PAN.las"))
                {
                    PANFiles.Add(file);
                }
                else
                {
                    Log.StaticLog.AddEntry(LogLevel.WARNING, "GamelandDirTree", $"Gameland folder {RootDir} containes file {file} which is neither a PAS nor a PAN file and will therefore be ignored when converting.");
                }
            }
            Log.StaticLog.AddEntry(LogEntry.TraceEntry("GamelandDirTree", "Separated files into PAN and PAS"));
        }

        public override IEnumerable<CliTask> GetTasks()
        {
            return new List<CliTask>()
            {
                new CliTask("Convert PAN files", () =>
                {
                    if(PANFiles.Count > 0)
                    {
                        string fileListName = $"file_list_PAN_las2las.{GamelandID}.txt";
                        CLICalls.CreateListOfFiles(PANFiles, fileListName);
                        CLICalls.CallLas2Las(fileListName, Las2LasProjectionMode.PA_N, $"{RootDir}\\{GamelandFolder.Las2LasSuffix}");

                        // todo: make sure list of files isn't deleted prematurely
                        CLICalls.RemoveListOfFiles(fileListName);
                    }
                }),
                new CliTask("Convert PAS files", () =>
                {
                    if(PASFiles.Count > 0)
                    {
                        string fileListName = $"file_list_PAS_las2las.{GamelandID}.txt";
                        CLICalls.CreateListOfFiles(PASFiles, fileListName);
                        CLICalls.CallLas2Las(fileListName, Las2LasProjectionMode.PA_S, $"{RootDir}\\{GamelandFolder.Las2LasSuffix}");

                        // todo: make sure list of files isn't deleted prematurely
                        CLICalls.RemoveListOfFiles(fileListName);
                    }
                })
            };
        }
    }

    public class GamelandFolder : BasePropertyChanged
    {
        public static string Las2LasSuffix = "las2las";
        public static string LasTileSuffix = "lastile";
        public static string Blast2DemSuffix = "blast2dem";
        public static string QGISSuffix = "QGISDEM";

        public GamelandFolder(string rootPath)
        {
            RootPath = rootPath;
            GamelandTree = new GamelandDirTree(RootPath);
            Las2LasTree = new Las2LasDirTree(RootPath);
            LasTileTree = new LasTileDirTree(RootPath);
            Blast2DemTree = new Blast2demDirTree(RootPath);
            ExecuteNextStepCommand = new BaseCommand((p) => true, (p) => Initialize());
            NextStep = GamelandProcessingStep.INITIAL;
        }

        protected string RootPath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public GamelandDirTree GamelandTree
        {
            get => GetValue<GamelandDirTree>();
            set => SetValue(value);
        }

        public Las2LasDirTree Las2LasTree
        {
            get => GetValue<Las2LasDirTree>();
            private set => SetValue(value);
        }

        public LasTileDirTree LasTileTree
        {
            get => GetValue<LasTileDirTree>();
            private set => SetValue(value);
        }

        public Blast2demDirTree Blast2DemTree
        {
            get => GetValue<Blast2demDirTree>();
            private set => SetValue(value);
        }

        public GamelandProcessingStep NextStep
        {
            get => GetStructOrDefaultValue<GamelandProcessingStep>();
            set => SetValue(value);
        }

        public BaseCommand ExecuteNextStepCommand
        {
            get => GetValue<BaseCommand>();
            set => SetValue(value);
        }

        private void Initialize()
        {
            GamelandTree.ScanFiles();
            NextStep = GamelandProcessingStep.LAS2LAS;
            ExecuteNextStepCommand = new BaseCommand((p) => true, (p) => ExecuteLas2Las());
        }

        private void ExecuteLas2Las()
        {
            GamelandTree.ScanFiles();
            GamelandTree.PrepareForExecution();
            foreach (var task in GamelandTree.GetTasks())
            {
                task.Execute();
            }
            NextStep = GamelandProcessingStep.LASTILE;
            ExecuteNextStepCommand = new BaseCommand((p) => true, (p) => ExecuteLasTile());
        }

        private void ExecuteLasTile()
        {
            Las2LasTree.ScanFiles();
            Las2LasTree.PrepareForExecution();
            foreach (var task in Las2LasTree.GetTasks())
            {
                task.Execute();
            }
            NextStep = GamelandProcessingStep.BLAST2DEM;
            ExecuteNextStepCommand = new BaseCommand((p) => true, (p) => ExecuteBlast2Dem());
        }

        private void ExecuteBlast2Dem()
        {
            LasTileTree.ScanFiles();
            LasTileTree.PrepareForExecution();
            foreach (var task in LasTileTree.GetTasks())
            {
                task.Execute();
            }
            NextStep = GamelandProcessingStep.QGIS;
            ExecuteNextStepCommand = new BaseCommand((p) => true, (p) => ExecuteQGIS());
        }

        private void ExecuteQGIS()
        {
            Blast2DemTree.ScanFiles();
            Blast2DemTree.PrepareForExecution();
            foreach (var task in Blast2DemTree.GetTasks())
            {
                task.Execute();
            }
            NextStep = GamelandProcessingStep.FINISHED;
            ExecuteNextStepCommand = new BaseCommand((p) => false, (p) => { });
        }

        public override string ToString()
        {
            return $"Folder {GamelandTree.GamelandID}";
        }
    }

    public enum GamelandProcessingStep
    {
        INITIAL,
        LAS2LAS,
        LASTILE,
        BLAST2DEM,
        QGIS,
        FINISHED
    }
}
