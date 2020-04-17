using ProjectKappa.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static ProjectKappa.ViewModels.MainWindowViewModel;

namespace ProjectKappa.LasToolsAPI
{
    public enum Las2LasProjectionMode
    {
        PA_N = 'N',
        PA_S = 'S'
    }

    public static class CLICalls
    {
        private static string LAStoolsRootDir = string.Empty;
        private static string QGISRootDir = string.Empty;
        public static UpdateConsoleOutDelegate UpdateConsoleOut;

        public static void SetLAStoolsRootDir(string path)
        {
            LAStoolsRootDir = path;
        }

        public static void SetQGISRootDir(string path)
        {
            QGISRootDir = path;

        }

        private static string EscapeStringQuotes(string value)
        {
            return $"\"{value}\"";
        }

        private static string BuildListOfFilesFilePath(string fileName)
        {
            return $"{LAStoolsRootDir}\\bin\\{fileName}";
        }

        public static void CreateListOfFiles(IEnumerable<string> files, string fileName, bool encloseInQuotes = false)
        {
            string path = BuildListOfFilesFilePath(fileName);
            if (!encloseInQuotes)
            {
                File.WriteAllLines(path, files);
            }
            else
            {
                foreach (var file in files)
                {
                    File.AppendAllText(path, $"{EscapeStringQuotes(file)}{Environment.NewLine}");
                }
            }
        }

        public static void RemoveListOfFiles(string fileName)
        {
            File.Delete(BuildListOfFilesFilePath(fileName));
        }

        private static string GetExecutablePath(string exeName)
        {
            if (LAStoolsRootDir.IsEmpty())
            {
                throw new ArgumentException("LAStools root directory not set.");
            }

            return EscapeStringQuotes($"{LAStoolsRootDir}\\bin\\{exeName}.exe");
        }

        private static string GetListOfFilePath(string fileName)
        {
            if (LAStoolsRootDir.IsEmpty())
            {
                throw new ArgumentException("LAStools root directory not set.");
            }

            return EscapeStringQuotes($"{LAStoolsRootDir}\\bin\\{fileName}");
        }

        private static string GetOSGeo4WShell()
        {
            return EscapeStringQuotes($"{QGISRootDir}\\OSGeo4W.bat");
        }

        public static void CallLas2Las(string listOfFiles, Las2LasProjectionMode projectionMode, string outputDir)
        {
            CallCLI(GetExecutablePath("las2las"), $"-lof {GetListOfFilePath(listOfFiles)} -keep_classification 2 -target_sp83  {projectionMode} -odir {EscapeStringQuotes(outputDir)} -olaz");
        }

        public static void CallLasTile(string listOfFiles, string outputDir)
        {
            CallCLI(GetExecutablePath("lastile"), $"-lof {GetListOfFilePath(listOfFiles)} -o tile.laz -tile_size 1000 -buffer 25 -odir {EscapeStringQuotes(outputDir)} -olaz");
        }

        public static void CallBlast2Dem(string listOfFiles, string outputDir)
        {
            CallCLI(GetExecutablePath("blast2dem"), $"-lof {GetListOfFilePath(listOfFiles)} -elevation -odir {EscapeStringQuotes(outputDir)} -otif");
        }

        public static void EnsureQGISEnvVars()
        {
            CallCLI($@"{GetOSGeo4WShell()} py3_env");
            CallCLI($@"{GetOSGeo4WShell()} qt5_env");
        }

        public static void CallQGISMerger(string listOfFiles, string outputPath)
        {
            CallCLI(GetOSGeo4WShell(), $@"cd {EscapeStringQuotes(QGISRootDir)}&py3_env&qt5_env&python3 -m gdal_merge -ot Float32 -of GTiff -o {EscapeStringQuotes(outputPath)} --optfile {GetListOfFilePath(listOfFiles)}");
        }

        private static void CallCLI(string command)
        {
            UpdateConsoleOut($"> Starting {command}");
            UpdateConsoleOut($"{Environment.NewLine}");
            var procStartInfo = new ProcessStartInfo("cmd.exe")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = $"/c {command}"
            };

            var proc = Process.Start(procStartInfo);

            var standardOutput = new AsyncStreamReader(proc.StandardOutput);
            var standardError = new AsyncStreamReader(proc.StandardError);

            standardOutput.DataReceived += (sender, data) =>
            {
                UpdateConsoleOut(data);
            };

            standardError.DataReceived += (sender, data) =>
            {
                Log.StaticLog.AddEntry(LogEntry.ErrorEntry("CallCLI", $"Command [{command}] resulted in ERROR"));
                UpdateConsoleOut(data);
            };

            standardOutput.Start();
            standardError.Start();

            Log.StaticLog.AddEntry(LogEntry.TraceEntry("CallCLI", $"command: {command}"));
            proc.WaitForExit();
            proc.Close();
            UpdateConsoleOut($"{Environment.NewLine}");

            standardOutput.Stop();
            standardError.Stop();
        }

        private static void CallCLI(string name, string args)
        {
            UpdateConsoleOut($"> Starting {name} with {args}");
            UpdateConsoleOut($"{Environment.NewLine}");
            var procStartInfo = new ProcessStartInfo(name)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = args
            };

            var proc = Process.Start(procStartInfo);

            var standardOutput = new AsyncStreamReader(proc.StandardOutput);
            var standardError = new AsyncStreamReader(proc.StandardError);

            standardOutput.DataReceived += (sender, data) =>
            {
                UpdateConsoleOut(data);
            };

            standardError.DataReceived += (sender, data) =>
            {
                Log.StaticLog.AddEntry(LogEntry.ErrorEntry("CallCLI", $"Command [{name} {args}] resulted in ERROR"));
                UpdateConsoleOut(data);
            };

            standardOutput.Start();
            standardError.Start();

            Log.StaticLog.AddEntry(LogEntry.TraceEntry("CallCLI", $"command: {name} {args}"));
            proc.WaitForExit();
            proc.Close();
            UpdateConsoleOut($"{Environment.NewLine}");

            standardOutput.Stop();
            standardError.Stop();
        }
    }
}
