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
        protected DirectoryTree(string rootDir, string suffix, string targetSuffix)
        {
            RootDir = rootDir;
            GamelandID = new DirectoryInfo(RootDir).Name;
            Suffix = suffix;
            TargetSuffix = targetSuffix;
        }

        public string Suffix
        {
            get => GetValue<string>();
            private set => SetValue(value);
        }

        public string TargetSuffix
        {
            get => GetValue<string>();
            private set => SetValue(value);
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
            ScanFiles(GetDirPath());
        }


        protected void ScanFiles(string path)
        {
            Log.StaticLog.AddEntry(LogEntry.TraceEntry("DirectoryTree", "Scanning files"));
            foreach (var filepath in Directory.EnumerateFiles(path))
            {
                ContainedFiles.Add(filepath);
            }
        }

        public abstract void PrepareForExecution();
        public abstract string GetDirPath();

        public abstract void CleanUp();

        public bool Finished
        {
            get => GetStructOrDefaultValue<bool>();
            set => SetValue(value);
        }

        public abstract IEnumerable<CliTask> GetTasks();
    }

    public class Las2LasDirTree : DirectoryTree
    {
        public Las2LasDirTree(string rootDir, string suffix, string targetSuffix)
            : base(rootDir, suffix, targetSuffix)
        {

        }

        public ObservableCollection<string> LAZFiles
        {
            get => GetValue<ObservableCollection<string>>();
            set => SetValue(value);
        }

        public override void CleanUp()
        {
            Directory.Delete(GetDirPath(), true);
            Log.StaticLog.AddEntry(LogEntry.InformationEntry($"Folder {this.GamelandID}, Las2Las", $"Cleaned up directory"));
        }

        public override string GetDirPath()
        {
            return $"{RootDir}\\{Suffix}";
        }

        public override IEnumerable<CliTask> GetTasks()
        {
            return new List<CliTask>()
            {
                new CliTask("Convert LAZ files to tiles", () =>
                {
                    string fileListName = $"file_list_LAZ_lastile{Suffix}.{GamelandID}.txt";
                    if(CLICalls.CreateListOfFiles(LAZFiles, fileListName))
                    {
                        CLICalls.CallLasTile(fileListName, $"{RootDir}\\{TargetSuffix}");
                        Log.StaticLog.AddEntry(LogEntry.InformationEntry($"Folder {this.GamelandID}, LAZ to tiles", $"Converted *{Suffix} LAZ files to tiles"));

                        CLICalls.RemoveListOfFiles(fileListName);
                    }
                })
            };
        }

        public override void PrepareForExecution()
        {
            Directory.CreateDirectory($"{RootDir}\\{TargetSuffix}");
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
        public LasTileDirTree(string rootDir, string suffix, string targetSuffix)
            : base(rootDir, suffix, targetSuffix)
        {

        }

        public ObservableCollection<string> LAZFiles
        {
            get => GetValue<ObservableCollection<string>>();
            set => SetValue(value);
        }

        public override void CleanUp()
        {
            Directory.Delete(GetDirPath(), true);
            Log.StaticLog.AddEntry(LogEntry.InformationEntry($"Folder {this.GamelandID}, LasTile", $"Cleaned up directory"));
        }

        public override string GetDirPath()
        {
            return $"{RootDir}\\{Suffix}";
        }

        public override IEnumerable<CliTask> GetTasks()
        {
            return new List<CliTask>()
            {
                new CliTask("Blast LAZ tiles to DEM", () =>
                {
                    string fileListName = $"file_list_LAZ_blast2dem{Suffix}.{GamelandID}.txt";
                    if(CLICalls.CreateListOfFiles(LAZFiles, fileListName))
                    {
                        CLICalls.CallBlast2Dem(fileListName, $"{RootDir}\\{TargetSuffix}");
                        Log.StaticLog.AddEntry(LogEntry.InformationEntry($"Folder {this.GamelandID}, Blast2Dem", $"Blasted *{Suffix} files to .tif, .kml [...]"));


                        CLICalls.RemoveListOfFiles(fileListName);
                    }
                })
            };
        }

        public override void PrepareForExecution()
        {
            Directory.CreateDirectory($"{RootDir}\\{TargetSuffix}");
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

    public class Blast2demDirTree : DirectoryTree
    {
        public Blast2demDirTree(string rootDir, string suffix, string targetSuffix)
            : base(rootDir, suffix, targetSuffix)
        {

        }

        public override string GetDirPath()
        {
            return $"{RootDir}\\{Suffix}";
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
                    string fileListName = $"file_list_TIF_qgis{Suffix}.{GamelandID}.txt";
                    string workDirPath = $"{RootDir}\\{GamelandFolder.QGISSuffix}";
                    string fileNameDem = $"{GamelandID}{Suffix}_DEM.tif";
                    string fileNameSlope = $"{GamelandID}{Suffix}_Slope_DEM.tif";
                    string fileNameHillshade = $"{GamelandID}{Suffix}_Hillshade_DEM.tif";

                    if(CLICalls.CreateListOfFiles(TIFFiles, fileListName, true))
                    {
                        CLICalls.CallQGISMerger(fileListName, $"{workDirPath}\\{fileNameDem}");
                        Log.StaticLog.AddEntry(LogEntry.InformationEntry($"Folder {this.GamelandID}, QGIS processing", $"Merged *{Suffix} files into a DEM"));

                        CLICalls.CallQGISSlopeAnalysis($"{workDirPath}\\{fileNameDem}", $"{workDirPath}\\{fileNameSlope}");
                        Log.StaticLog.AddEntry(LogEntry.InformationEntry($"Folder {this.GamelandID}, QGIS processing", $"Created Slope Analysis for *{Suffix} from previously created DEM"));

                        CLICalls.CallQGISHillshadeAnalysis($"{workDirPath}\\{fileNameDem}", $"{workDirPath}\\{fileNameHillshade}");
                        Log.StaticLog.AddEntry(LogEntry.InformationEntry($"Folder {this.GamelandID}, QGIS processing", $"Created Hillshade Analysis for *{Suffix} from previously created DEM"));

                        CLICalls.RemoveListOfFiles(fileListName);
                    }
                })
            };
        }

        public override void PrepareForExecution()
        {
            Directory.CreateDirectory($"{RootDir}\\{TargetSuffix}");
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

        public override void CleanUp()
        {
            Directory.Delete(GetDirPath(), true);
            Log.StaticLog.AddEntry(LogEntry.InformationEntry($"Folder {this.GamelandID}, Blast2Dem", $"Cleaned up directory"));
        }
    }

    public class GamelandDirTree : DirectoryTree
    {
        public GamelandDirTree(string rootDir, string suffix, string targetSuffix)
            : base(rootDir, suffix, targetSuffix)
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
            Directory.CreateDirectory($"{RootDir}\\{GamelandFolder.Las2LasPANSuffix}");
            Directory.CreateDirectory($"{RootDir}\\{GamelandFolder.Las2LasPASSuffix}");
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
                        if(CLICalls.CreateListOfFiles(PANFiles, fileListName))
                        {
                            CLICalls.CallLas2Las(fileListName, Las2LasProjectionMode.PA_N, $"{RootDir}\\{GamelandFolder.Las2LasPANSuffix}");
                            Log.StaticLog.AddEntry(LogEntry.InformationEntry($"Folder {this.GamelandID}, Las2Las", $"Converted *PAN LAS files to LAZ"));

                            CLICalls.RemoveListOfFiles(fileListName);
                        }
                    }
                }),
                new CliTask("Convert PAS files", () =>
                {
                    if(PASFiles.Count > 0)
                    {
                        string fileListName = $"file_list_PAS_las2las.{GamelandID}.txt";
                        if(CLICalls.CreateListOfFiles(PASFiles, fileListName))
                        {
                            CLICalls.CallLas2Las(fileListName, Las2LasProjectionMode.PA_S, $"{RootDir}\\{GamelandFolder.Las2LasPASSuffix}");
                            Log.StaticLog.AddEntry(LogEntry.InformationEntry($"Folder {this.GamelandID}, Las2Las", $"Converted *PAS LAS files to LAZ"));

                            CLICalls.RemoveListOfFiles(fileListName);
                        }
                    }
                })
            };
        }

        public override void CleanUp()
        {
            // nothing to clean up...
        }
    }

    public class GamelandFolder : BasePropertyChanged
    {
        public static string Las2LasSuffix = "las2las";
        public static string Las2LasPANSuffix = $"{Las2LasSuffix}PAN";
        public static string Las2LasPASSuffix = $"{Las2LasSuffix}PAS";

        public static string LasTileSuffix = "lastile";
        public static string LasTilePANSuffix = $"{LasTileSuffix}PAN";
        public static string LasTilePASSuffix = $"{LasTileSuffix}PAS";

        public static string Blast2DemSuffix = "blast2dem";
        public static string Blast2DemPANSuffix = $"{Blast2DemSuffix}PAN";
        public static string Blast2DemPASSuffix = $"{Blast2DemSuffix}PAS";

        public static string QGISSuffix = "QGISDEM";

        public GamelandFolder(string rootPath)
        {
            RootPath = rootPath;
            GamelandTree = new GamelandDirTree(RootPath, string.Empty, string.Empty);

            Las2LasTrees = new List<Las2LasDirTree>()
            {
                new Las2LasDirTree(RootPath, Las2LasPANSuffix, LasTilePANSuffix),
                new Las2LasDirTree(RootPath, Las2LasPASSuffix, LasTilePASSuffix)
            };

            LasTileTrees = new List<LasTileDirTree>()
            {
                new LasTileDirTree(RootPath, LasTilePANSuffix, Blast2DemPANSuffix),
                new LasTileDirTree(RootPath, LasTilePASSuffix, Blast2DemPASSuffix)
            };

            Blast2DemTrees = new List<Blast2demDirTree>()
            {
                new Blast2demDirTree(RootPath, Blast2DemPANSuffix, QGISSuffix),
                new Blast2demDirTree(RootPath, Blast2DemPASSuffix, QGISSuffix)
            };

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

        public List<Las2LasDirTree> Las2LasTrees
        {
            get => GetValue<List<Las2LasDirTree>>();
            private set => SetValue(value);
        }

        public List<LasTileDirTree> LasTileTrees
        {
            get => GetValue<List<LasTileDirTree>>();
            private set => SetValue(value);
        }

        public List<Blast2demDirTree> Blast2DemTrees
        {
            get => GetValue<List<Blast2demDirTree>>();
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
            GamelandTree.CleanUp();
            NextStep = GamelandProcessingStep.LASTILE;
            ExecuteNextStepCommand = new BaseCommand((p) => true, (p) => ExecuteLasTile());
        }

        private void ExecuteLasTile()
        {
            foreach (var las2lasTree in Las2LasTrees)
            {
                las2lasTree.ScanFiles();
                las2lasTree.PrepareForExecution();
                foreach (var task in las2lasTree.GetTasks())
                {
                    task.Execute();
                }
                las2lasTree.CleanUp();
            }

            NextStep = GamelandProcessingStep.BLAST2DEM;
            ExecuteNextStepCommand = new BaseCommand((p) => true, (p) => ExecuteBlast2Dem());
        }

        private void ExecuteBlast2Dem()
        {
            foreach (var lasTileTree in LasTileTrees)
            {
                lasTileTree.ScanFiles();
                lasTileTree.PrepareForExecution();
                foreach (var task in lasTileTree.GetTasks())
                {
                    task.Execute();
                }
                lasTileTree.CleanUp();
            }

            NextStep = GamelandProcessingStep.QGIS;
            ExecuteNextStepCommand = new BaseCommand((p) => true, (p) => ExecuteQGIS());
        }

        private void ExecuteQGIS()
        {
            foreach (var blast2DemTree in Blast2DemTrees)
            {
                blast2DemTree.ScanFiles();
                blast2DemTree.PrepareForExecution();
                foreach (var task in blast2DemTree.GetTasks())
                {
                    task.Execute();
                }
                blast2DemTree.CleanUp();
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
