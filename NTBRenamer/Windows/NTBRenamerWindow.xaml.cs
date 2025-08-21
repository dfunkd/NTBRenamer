using Microsoft.Win32;
using NTBRenamer.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NTBRenamer.Windows;

public partial class NTBRenamerWindow : Window
{
    #region Routed Commands
    #region ChangeDirectory Command
    private static readonly RoutedCommand changeDirectoryCommand = new();
    public static RoutedCommand ChangeDirectoryCommand = changeDirectoryCommand;
    private void CanExecuteChangeDirectoryCommand(object sender, CanExecuteRoutedEventArgs e)
        => e.CanExecute = e.Source is Control && DataContext is NTBRenamerViewModel vm && vm.IsProcessing == false;
    private void ExecutedChangeDirectoryCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (DataContext is NTBRenamerViewModel vm)
        {
            OpenFolderDialog d = new();
            if (d.ShowDialog(this) == true)
                vm.SelectedDirectory = d.FolderName;
        }
    }
    #endregion

    #region Close Command
    private static readonly RoutedCommand closeCommand = new();
    public static RoutedCommand CloseCommand = closeCommand;
    private void CanExecuteCloseCommand(object sender, CanExecuteRoutedEventArgs e)
        => e.CanExecute = e.Source is Control;
    private void ExecutedCloseCommand(object sender, ExecutedRoutedEventArgs e)
        => Close();
    #endregion

    #region FileCleanup Command
    private static readonly RoutedCommand fileCleanupCommand = new();
    public static RoutedCommand FileCleanupCommand = fileCleanupCommand;
    private void CanExecuteFileCleanupCommand(object sender, CanExecuteRoutedEventArgs e)
        => e.CanExecute = e.Source is Control && DataContext is NTBRenamerViewModel vm && vm.IsProcessing == false;
    private async void ExecutedFileCleanupCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (DataContext is NTBRenamerViewModel vm)
        {
            await Task.Run(() =>
            {
                vm.IsProcessing = true;
                vm.FailedCount = 0;
                vm.FileCount = 0;
                vm.ProcessedCount = 0;
                vm.SkipCount = 0;
                vm.Directories = [];
                vm.Files = [];
                vm.Stopwatch = Stopwatch.StartNew();

                vm.FileCleanup();

                vm.Stopwatch.Stop();
                vm.UpdateStopwatch();
                vm.IsProcessing = false;
            });
        }
    }
    #endregion

    #region Process Command
    private static readonly RoutedCommand processCommand = new();
    public static RoutedCommand ProcessCommand = processCommand;
    private void CanExecuteProcessCommand(object sender, CanExecuteRoutedEventArgs e)
        => e.CanExecute = e.Source is Control && DataContext is NTBRenamerViewModel vm && vm.IsProcessing == false;
    private async void ExecutedProcessCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (DataContext is NTBRenamerViewModel vm)
        {
            await Task.Run(async () =>
            {
                vm.IsProcessing = true;
                vm.FailedCount = 0;
                vm.FileCount = 0;
                vm.ProcessedCount = 0;
                vm.SkipCount = 0;
                vm.Directories = [];
                vm.Files = [];
                vm.Stopwatch = Stopwatch.StartNew();

                await vm.ProcessFiles();

                vm.Stopwatch.Stop();
                vm.UpdateStopwatch();
                vm.IsProcessing = false;
            });
        }
    }
    #endregion

    #region RunBatchCommand Command
    private static readonly RoutedCommand runBatchCommand = new();
    public static RoutedCommand RunBatchCommand = runBatchCommand;
    private void CanExecuteRunBatchCommand(object sender, CanExecuteRoutedEventArgs e)
        => e.CanExecute = e.Source is Control && DataContext is NTBRenamerViewModel vm && vm.IsProcessing == false;
    private async void ExecutedRunBatchCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (DataContext is NTBRenamerViewModel vm)
        {
            await Task.Run(() =>
            {
                vm.IsProcessing = true;
                vm.FailedCount = 0;
                vm.FileCount = 0;
                vm.ProcessedCount = 0;
                vm.SkipCount = 0;
                vm.Directories = [];
                vm.Files = [];
                vm.Stopwatch = Stopwatch.StartNew();

                vm.RunBatch();

                vm.Stopwatch.Stop();
                vm.UpdateStopwatch();
                vm.IsProcessing = false;
            });
        }
    }
    #endregion

    #region StripExtensionsCommand Command
    private static readonly RoutedCommand stripExtensionsCommand = new();
    public static RoutedCommand StripExtensionsCommand = stripExtensionsCommand;
    private void CanExecuteStripExtensionsCommand(object sender, CanExecuteRoutedEventArgs e)
        => e.CanExecute = e.Source is Control && DataContext is NTBRenamerViewModel vm && vm.IsProcessing == false;
    private async void ExecutedStripExtensionsCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (DataContext is NTBRenamerViewModel vm)
        {
            await Task.Run(() =>
            {
                vm.IsProcessing = true;
                vm.FailedCount = 0;
                vm.FileCount = 0;
                vm.ProcessedCount = 0;
                vm.SkipCount = 0;
                vm.Directories = [];
                vm.Files = [];
                vm.Stopwatch = Stopwatch.StartNew();

                vm.StripExtensions();

                vm.Stopwatch.Stop();
                vm.UpdateStopwatch();
                vm.IsProcessing = false;
            });
        }
    }
    #endregion
    #endregion

    public NTBRenamerWindow()
    {
        InitializeComponent();
    }

    private void OnDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }
}
