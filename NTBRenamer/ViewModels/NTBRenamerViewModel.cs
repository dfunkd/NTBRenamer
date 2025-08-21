using NTBRenamer.Core;
using System.Diagnostics;
using System.IO;

namespace NTBRenamer.ViewModels;

public class NTBRenamerViewModel : BaseViewModel
{
    #region Properties
    private string elapsedTime = "0:00:00.0000";
    public string ElapsedTime
    {
        get => elapsedTime;
        set
        {
            if (elapsedTime != value)
            {
                elapsedTime = value;
                OnPropertyChanged();
            }
        }
    }

    private List<string> directories = [];
    public List<string> Directories
    {
        get => directories;
        set
        {
            if (directories != value)
            {
                directories = value;
                OnPropertyChanged();
            }
        }
    }

    private int failedCount = 0;
    public int FailedCount
    {
        get => failedCount;
        set
        {
            if (failedCount != value)
            {
                failedCount = value;
                OnPropertyChanged();
            }
        }
    }

    private int fileCount = 0;
    public int FileCount
    {
        get => fileCount;
        set
        {
            if (fileCount != value)
            {
                fileCount = value;
                OnPropertyChanged();
            }
        }
    }

    private List<string> files = [];
    public List<string> Files
    {
        get => files;
        set
        {
            if (files != value)
            {
                files = value;
                OnPropertyChanged();
            }
        }
    }

    private bool isProcessing = false;
    public bool IsProcessing
    {
        get => isProcessing;
        set
        {
            if (isProcessing != value)
            {
                isProcessing = value;
                OnPropertyChanged();
            }
        }
    }

    private int processedCount = 0;
    public int ProcessedCount
    {
        get => processedCount;
        set
        {
            if (processedCount != value)
            {
                processedCount = value;
                OnPropertyChanged();
            }
        }
    }

    private string selectedDirectory = @"C:\NTBImages";
    public string SelectedDirectory
    {
        get => selectedDirectory;
        set
        {
            if (selectedDirectory != value)
            {
                selectedDirectory = value;
                OnPropertyChanged();
            }
        }
    }

    private int skipCount = 0;
    public int SkipCount
    {
        get => skipCount;
        set
        {
            if (skipCount != value)
            {
                skipCount = value;
                OnPropertyChanged();
            }
        }
    }

    private Stopwatch stopwatch = new();
    public Stopwatch Stopwatch
    {
        get => stopwatch;
        set
        {
            if (stopwatch != value)
            {
                stopwatch = value;
                OnPropertyChanged();
            }
        }
    }
    #endregion

    #region Private Methods
    public static void ExecuteBatchFile(string batchPath, string dir)
    {
        try
        {
            ProcessStartInfo start = new()
            {
                Arguments = $"/C \"{batchPath}\"",
                CreateNoWindow = true,
                WorkingDirectory = dir,
                FileName="cmd.exe",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using Process process = Process.Start(start);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            var testExitCode = process.ExitCode;
            Console.WriteLine(output);
            Console.WriteLine(error);
        }
        catch (Exception ex)
        {
            var test = ex.Message;
        }
    }

    public void FileCleanup()
    {
        PopulateAllDirectories();
        PopulateAllFiles();
        List<FileInfo> pdfs = [];
        List<FileInfo> pss = [];

        foreach (var file in Files)
        {
            FileInfo fileInfo = new(file);
            if (!fileInfo.Exists)
                continue;
            if (fileInfo.Extension == ".pdf")
                pdfs.Add(fileInfo);
            if (fileInfo.Extension == ".ps")
                pss.Add(fileInfo);
            UpdateStopwatch();
        }

        foreach (var pdf in pdfs)
        {
            var ps = pss.FirstOrDefault(f => f.Name.StartsWith(pdf.Name.Replace("pdf", string.Empty)));
            if (ps is null)
            {
                SkipCount++;
                UpdateStopwatch();
                continue;
            }
            ps?.Delete();
            ProcessedCount++;
            UpdateStopwatch();
        }
    }

    private void PopulateAllDirectories()
    {
        Directories = [$"{SelectedDirectory}"];
        Directories.AddRange(GetDirectories(SelectedDirectory));
    }

    private void PopulateAllFiles()
    {
        Files = [];
        foreach (string dir in Directories)
        {
            Files.AddRange(Directory.GetFiles(dir));
            UpdateStopwatch();
        }
    }

    private Task ProcessFiles(List<string> files)
    {
        foreach (string file in files)
        {
            FileInfo fileInfo = new(file);
            if (!fileInfo.Exists)
                continue;

            fileCount++;

            if (!string.IsNullOrEmpty(fileInfo.Extension))
            {
                skipCount++;
                continue;
            }

            string? mime = FileExtension.GetMimeType(File.ReadAllBytes(file), file);
            string? ext = string.IsNullOrEmpty(mime)
                ? ".ps"
                : mime.Replace("application/", "").Replace("audio/", "").Replace("image/", "").Replace("video/", "").Replace(".", "");

            if (string.IsNullOrEmpty(ext))
            {
                FailedCount++;
                continue;
            }

            File.Move(file, Path.ChangeExtension(file, ext));
            ProcessedCount++;
        }
        return Task.FromResult(0);
    }

    private static List<string> GetDirectories(string path, string searchPattern)
    {
        try
        {
            return [.. Directory.GetDirectories(path, searchPattern)];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    private static List<string> GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
    {
        if (searchOption == SearchOption.TopDirectoryOnly)
            return [.. Directory.GetDirectories(path, searchPattern)];

        var directories = new List<string>(GetDirectories(path, searchPattern));

        for (var i = 0; i < directories.Count; i++)
            directories.AddRange(GetDirectories(directories[i], searchPattern));

        return directories;
    }
    #endregion

    #region Public Methods
    public async Task ProcessFiles()
    {
        PopulateAllDirectories();
        PopulateAllFiles();

        List<Task> tasks = [];
        int count = 0, takeQty = 100;
        for (int i = 0; i < Files.Count; i = i + takeQty)
        {
            int skip = takeQty * count;
            int take = Files.Count - skip > takeQty ? takeQty : Files.Count - skip;
            tasks.Add(Task.Run(async () => await ProcessFiles([.. files.Skip(skip).Take(take)])));
            count++;
            UpdateStopwatch();
        }
        await Task.WhenAll([.. tasks]);
        UpdateStopwatch();
    }

    public void RunBatch()
    {
        PopulateAllDirectories();

        string batchPath = @"C:\NTBImages\gsBatch.bat";

        foreach (var directory in directories)
        {
            UpdateStopwatch();

            var path = $@"{directory}\gsBatch.bat";
            if (directory != @"C:\NTBImages" && !File.Exists(path))
                File.Copy(batchPath, path);

            ExecuteBatchFile(path, directory);

            if (directory != "C:\\NTBImages")
                File.Delete(path);

            UpdateStopwatch();
        }
        UpdateStopwatch();
    }

    public void StripExtensions()
    {
        PopulateAllDirectories();
        PopulateAllFiles();

        UpdateStopwatch();
        foreach (string directory in Directories)
        {
            Files.AddRange(Directory.GetFiles(directory));
            foreach (string file in Files)
            {
                if (!File.Exists(file))
                    continue;

                FileInfo info = new(file);
                if (info.Extension == ".bat" || string.IsNullOrEmpty(info.Extension))
                    continue;

                string stripped = file.Replace(info.Extension, string.Empty);
                if (File.Exists(stripped))
                    continue;

                File.Move(file, stripped);
                UpdateStopwatch();
            }
            UpdateStopwatch();
        }
        UpdateStopwatch();
    }

    public string UpdateStopwatch()
        => ElapsedTime = $"Processed in {Stopwatch.Elapsed.Hours:00}:{Stopwatch.Elapsed.Minutes:00}:{Stopwatch.Elapsed.Seconds:00}.{Stopwatch.Elapsed.Microseconds:0000}";
    #endregion
}
