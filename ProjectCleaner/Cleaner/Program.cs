using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Serilog;
using Serilog.Core;

namespace Cleaner
{
    class Program
    {
        private static Dictionary<string, string> _commands;
        private static DirectoryInfo RootDestinationDir;
        private static DirectoryInfo AssetsDir;
        private static DirectoryInfo SrcDir;

        static void Main(string[] args)
        {
            _commands = GetCommands(args);

            var src = args[0];
            if (string.IsNullOrWhiteSpace(src))
            {
                Console.WriteLine("No Source Path Provided");
                return;
            }

            if (!_commands.ContainsKey("-o"))
            {
                Console.WriteLine("No Output Path Provided");
                return;
            }

            RootDestinationDir = new DirectoryInfo(_commands["-o"]);
            SrcDir = new DirectoryInfo(Path.Combine(RootDestinationDir.FullName, "src"));
            AssetsDir = new DirectoryInfo(Path.Combine(RootDestinationDir.FullName, "assets"));
            Directory.CreateDirectory(SrcDir.FullName);
            Directory.CreateDirectory(AssetsDir.FullName);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(RootDestinationDir.FullName, "Zip.log"))
                .CreateLogger();

            Log.Information($"Root Destination Dir :{RootDestinationDir.FullName}");
            Log.Information($"Source Destination Dir :{SrcDir.FullName}");
            Log.Information($"Assets Destination Dir :{AssetsDir.FullName}");

            DirectoryCopy(src, SrcDir.FullName, true);

            ZipFile.CreateFromDirectory(SrcDir.FullName, Path.Combine(RootDestinationDir.FullName, "Src.zip")
                , CompressionLevel.Optimal, true);

            Console.WriteLine("Success");
            Console.Read();
        }

        private static Dictionary<string, string> GetCommands(string[] args)
        {
            var commandDict = new Dictionary<string, string>();
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine("Argument Value not Provided for Last Arguement");
                        break;
                    }

                    commandDict.Add(args[i], args[i + 1].Trim());
                }
            }

            return commandDict;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            Log.Information($"Copying Source => {sourceDirName}, Dest => {destDirName}");

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (dir.Name == ".vs" || dir.Name == ".git" || dir.Name == "bin" || dir.Name == "obj")
                return;

            if (!dir.Exists)
            {
                return;
            }

            var lowerDirName = dir.Name.ToLower();
            if (lowerDirName == "wwwroot"
                || lowerDirName == "scripts" || lowerDirName == "js"
                || lowerDirName == "css" || lowerDirName == "images" || lowerDirName == "lib")
            {
                ZipAssets(dir);
                return;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static void ZipAssets(DirectoryInfo dir)
        {
            var dirSplit = dir.FullName.Split(Path.DirectorySeparatorChar);
            var parentName = dirSplit[dirSplit.Length - 2];
            var targetZipName = $"{parentName}_{dir.Name}.zip";

            ZipFile.CreateFromDirectory(dir.FullName, Path.Combine(AssetsDir.FullName, targetZipName), CompressionLevel.Optimal, true);
            Log.Information($"Create Asset => {parentName}, Dest => {targetZipName}");
        }
    }
}
