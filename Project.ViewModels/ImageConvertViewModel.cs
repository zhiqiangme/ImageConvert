using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageConvert.Core.Abstractions;
using ImageConvert.Models.Entities;
using ImageConvert.Models.Enums;

namespace ImageConvert.ViewModels;

public sealed partial class ImageConvertViewModel : ObservableObject
{
    private readonly IImageConversionService _imageConversionService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IUserNotificationService _userNotificationService;
    private readonly Dictionary<string, ConversionItemViewModel> _fileLookup = new(StringComparer.OrdinalIgnoreCase);

    private CancellationTokenSource? _conversionCts;

    [ObservableProperty]
    private bool _isConverting;

    [ObservableProperty]
    private double _progressMaximum = 1;

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private string _progressText = "当前未开始转换。";

    [ObservableProperty]
    private string _statusText = "请选择或拖拽 WebP 文件。";

    [ObservableProperty]
    private string _summaryText = "当前未选择文件。";

    public ImageConvertViewModel(
        IImageConversionService imageConversionService,
        IFileDialogService fileDialogService,
        IUserNotificationService userNotificationService)
    {
        _imageConversionService = imageConversionService;
        _fileDialogService = fileDialogService;
        _userNotificationService = userNotificationService;

        BrowseFilesCommand = new AsyncRelayCommand(BrowseFilesAsync, CanBrowseFiles);
        ReplaceFilesCommand = new AsyncRelayCommand<IReadOnlyList<string>?>(ReplaceFilesAsync, CanReplaceFiles);
        ConvertCommand = new AsyncRelayCommand(ConvertAsync, CanConvert);
        CancelCommand = new RelayCommand(Cancel, CanCancel);
        ClearFilesCommand = new RelayCommand(ClearFiles, CanClearFiles);
    }

    public ObservableCollection<ConversionItemViewModel> Files { get; } = [];

    public bool HasFiles => Files.Count > 0;

    public IAsyncRelayCommand BrowseFilesCommand { get; }

    public IAsyncRelayCommand<IReadOnlyList<string>?> ReplaceFilesCommand { get; }

    public IAsyncRelayCommand ConvertCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public IRelayCommand ClearFilesCommand { get; }

    partial void OnIsConvertingChanged(bool value)
    {
        BrowseFilesCommand.NotifyCanExecuteChanged();
        ReplaceFilesCommand.NotifyCanExecuteChanged();
        ConvertCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
        ClearFilesCommand.NotifyCanExecuteChanged();
    }

    private bool CanBrowseFiles() => !IsConverting;

    private bool CanReplaceFiles(IReadOnlyList<string>? _) => !IsConverting;

    private bool CanConvert() => !IsConverting && Files.Count > 0;

    private bool CanCancel() => IsConverting;

    private bool CanClearFiles() => !IsConverting && Files.Count > 0;

    private async Task BrowseFilesAsync()
    {
        var paths = await _fileDialogService.PickWebpFilesAsync();
        if (paths.Count > 0)
        {
            await ReplaceFilesAsync(paths);
        }
    }

    private Task ReplaceFilesAsync(IReadOnlyList<string>? paths)
    {
        if (paths is null)
        {
            return Task.CompletedTask;
        }

        var normalizedPaths = paths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Where(static path => string.Equals(Path.GetExtension(path), ".webp", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedPaths.Count == 0)
        {
            _userNotificationService.ShowInfo("提示", "未检测到 .webp 文件。");
            return Task.CompletedTask;
        }

        Files.Clear();
        _fileLookup.Clear();

        foreach (var path in normalizedPaths)
        {
            var item = new ConversionItemViewModel(path);
            Files.Add(item);
            _fileLookup[path] = item;
        }

        ProgressMaximum = Files.Count;
        ProgressValue = 0;
        ProgressText = $"已载入 {Files.Count} 个待处理文件。";
        StatusText = Files.Count == 1
            ? $"已选择 1 张图片: {Files[0].FileName}"
            : $"已选择 {Files.Count} 张图片。";
        SummaryText = "等待开始转换。";

        OnPropertyChanged(nameof(HasFiles));
        ConvertCommand.NotifyCanExecuteChanged();
        ClearFilesCommand.NotifyCanExecuteChanged();
        return Task.CompletedTask;
    }

    private async Task ConvertAsync()
    {
        if (Files.Count == 0)
        {
            return;
        }

        foreach (var item in Files)
        {
            item.Reset();
        }

        _conversionCts = new CancellationTokenSource();
        ProgressMaximum = Files.Count;
        ProgressValue = 0;
        ProgressText = $"共 {Files.Count} 个文件，准备开始。";
        StatusText = "转换中";
        SummaryText = "正在处理，请稍候。";
        IsConverting = true;

        var workItems = Files
            .Select(item => new ConversionWorkItem(item.InputPath))
            .ToList();

        var progress = new Progress<ConversionProgress>(HandleProgress);

        try
        {
            var summary = await _imageConversionService.ConvertAsync(workItems, progress, _conversionCts.Token);
            ApplySummary(summary);
        }
        catch (OperationCanceledException)
        {
            var processed = (int)ProgressValue;
            StatusText = "已取消";
            SummaryText = $"已取消。已处理 {processed}/{Files.Count} 个文件。";
            ProgressText = $"取消请求已生效，最终停在 {processed}/{Files.Count}。";
        }
        finally
        {
            _conversionCts?.Dispose();
            _conversionCts = null;
            IsConverting = false;
        }
    }

    private void Cancel()
    {
        _conversionCts?.Cancel();
        StatusText = "正在取消";
        SummaryText = "当前文件结束后会停止后续转换。";
    }

    private void ClearFiles()
    {
        Files.Clear();
        _fileLookup.Clear();
        ProgressMaximum = 1;
        ProgressValue = 0;
        ProgressText = "当前未开始转换。";
        StatusText = "请选择或拖拽 WebP 文件。";
        SummaryText = "当前未选择文件。";
        OnPropertyChanged(nameof(HasFiles));
        ConvertCommand.NotifyCanExecuteChanged();
        ClearFilesCommand.NotifyCanExecuteChanged();
    }

    private void HandleProgress(ConversionProgress progress)
    {
        if (!_fileLookup.TryGetValue(progress.Item.InputPath, out var itemViewModel))
        {
            return;
        }

        if (progress.Stage == ConversionProgressStage.Started)
        {
            itemViewModel.MarkConverting();
            StatusText = $"正在转换 {progress.CurrentIndex}/{progress.TotalCount}: {progress.Item.FileName}";
            ProgressText = $"正在处理第 {progress.CurrentIndex} 个文件，共 {progress.TotalCount} 个。";
            return;
        }

        if (progress.Result is null)
        {
            return;
        }

        itemViewModel.ApplyResult(progress.Result);
        ProgressValue = progress.CurrentIndex;
        StatusText = $"已处理 {progress.CurrentIndex}/{progress.TotalCount}";
        ProgressText = progress.Result.State switch
        {
            ConversionItemState.Succeeded => $"已输出: {progress.Result.OutputPath}",
            ConversionItemState.UnsupportedAnimated => progress.Result.Message ?? "该文件为动图 WebP。",
            ConversionItemState.Failed => progress.Result.Message ?? "转换失败。",
            _ => ProgressText
        };
    }

    private void ApplySummary(ConversionSummary summary)
    {
        ProgressValue = summary.ProcessedCount;
        StatusText = "转换完成";
        SummaryText = $"成功 {summary.ConvertedCount}，失败 {summary.FailedCount}，不支持动图 {summary.UnsupportedAnimatedCount}。";
        ProgressText = summary.ProcessedCount == summary.TotalCount
            ? "全部任务已结束。"
            : $"已处理 {summary.ProcessedCount}/{summary.TotalCount}。";
    }
}
