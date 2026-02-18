using ImageMagick;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ImageConvert;

public sealed partial class MainWindow : Window
{
    private readonly List<string> _selectedFiles = [];
    private CancellationTokenSource? _convertCts;
    private bool _isConverting;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        if (_isConverting)
        {
            return;
        }

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            await ShowInfoDialogAsync("仅支持拖入文件。");
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        var paths = items
            .OfType<StorageFile>()
            .Select(file => file.Path)
            .Where(IsWebpPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (paths.Count == 0)
        {
            await ShowInfoDialogAsync("未检测到 .webp 文件。");
            return;
        }

        SetSelectedFiles(paths);
    }

    private async void SelectFilesButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isConverting)
        {
            return;
        }

        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".webp");
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

        var files = await picker.PickMultipleFilesAsync();
        if (files is null || files.Count == 0)
        {
            return;
        }

        SetSelectedFiles(files.Select(file => file.Path));
    }

    private async void StartConvertButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isConverting || _selectedFiles.Count == 0)
        {
            return;
        }

        _convertCts = new CancellationTokenSource();
        SetConvertingState(true);
        await ConvertFilesSerialAsync(_selectedFiles, _convertCts.Token);
        SetConvertingState(false);
    }

    private void CancelConvertButton_Click(object sender, RoutedEventArgs e)
    {
        _convertCts?.Cancel();
    }

    // 串行执行队列，点击取消后仅停止后续图片转换。
    private async Task ConvertFilesSerialAsync(IReadOnlyList<string> inputPaths, CancellationToken token)
    {
        var total = inputPaths.Count;
        var processed = 0;
        var converted = 0;
        var failed = 0;
        var unsupportedAnimated = 0;

        ConvertProgressBar.Maximum = total;
        ConvertProgressBar.Value = 0;
        StatusTextBlock.Text = $"开始转换，共 {total} 张。";

        foreach (var inputPath in inputPaths)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await Task.Run(() => ConvertSingleWebpToPng(inputPath, token), token);
                converted++;
            }
            catch (AnimatedWebpNotSupportedException)
            {
                unsupportedAnimated++;
                await ShowInfoDialogAsync($"文件“{Path.GetFileName(inputPath)}”是动图，暂未支持动图。");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                failed++;
                await ShowInfoDialogAsync($"文件“{Path.GetFileName(inputPath)}”转换失败：{ex.Message}");
            }
            finally
            {
                processed++;
                ConvertProgressBar.Value = processed;
                StatusTextBlock.Text = $"已转换 {converted}/{total}，已处理 {processed}/{total}。";
            }
        }

        if (token.IsCancellationRequested)
        {
            StatusTextBlock.Text = $"已取消。已转换 {converted}/{total}，已处理 {processed}/{total}。";
            return;
        }

        StatusTextBlock.Text = $"转换完成。成功 {converted} 张，失败 {failed} 张，动图未支持 {unsupportedAnimated} 张。";
    }

    // 单张转换逻辑：检测是否为动图，若是则抛出专用异常。
    private static void ConvertSingleWebpToPng(string inputPath, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var collection = new MagickImageCollection();
        collection.Ping(inputPath);
        if (collection.Count > 1)
        {
            throw new AnimatedWebpNotSupportedException();
        }

        var outputPath = GetUniqueOutputPath(inputPath);
        using var image = new MagickImage(inputPath);
        image.Format = MagickFormat.Png;
        image.Write(outputPath);
    }

    // 同名输出文件自动重命名：a.png -> a_1.png -> a_2.png ...
    private static string GetUniqueOutputPath(string inputPath)
    {
        var directory = Path.GetDirectoryName(inputPath)!;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputPath);

        var outputPath = Path.Combine(directory, $"{fileNameWithoutExtension}.png");
        var index = 1;
        while (File.Exists(outputPath))
        {
            outputPath = Path.Combine(directory, $"{fileNameWithoutExtension}_{index}.png");
            index++;
        }

        return outputPath;
    }

    private void SetSelectedFiles(IEnumerable<string> paths)
    {
        _selectedFiles.Clear();
        _selectedFiles.AddRange(paths
            .Where(IsWebpPath)
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase));

        SelectedFilesListView.ItemsSource = _selectedFiles.Select(Path.GetFileName).ToList();
        StartConvertButton.IsEnabled = _selectedFiles.Count > 0 && !_isConverting;
        StatusTextBlock.Text = _selectedFiles.Count switch
        {
            0 => "请选择或拖拽 WebP 文件。",
            1 => $"已选择 1 张：{Path.GetFileName(_selectedFiles[0])}",
            _ => $"已选择 {_selectedFiles.Count} 张图片。",
        };
    }

    private void SetConvertingState(bool converting)
    {
        _isConverting = converting;
        StartConvertButton.IsEnabled = !converting && _selectedFiles.Count > 0;
        CancelConvertButton.IsEnabled = converting;
        SelectFilesButton.IsEnabled = !converting;
    }

    private static bool IsWebpPath(string path) =>
        string.Equals(Path.GetExtension(path), ".webp", StringComparison.OrdinalIgnoreCase);

    private async Task ShowInfoDialogAsync(string message)
    {
        if (Content is not FrameworkElement frameworkElement)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "提示",
            Content = message,
            CloseButtonText = "确定",
            XamlRoot = frameworkElement.XamlRoot,
        };

        await dialog.ShowAsync();
    }

    private sealed class AnimatedWebpNotSupportedException : Exception;
}
