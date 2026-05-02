using CommunityToolkit.Mvvm.ComponentModel;
using ImageConvert.Models.Entities;
using ImageConvert.Models.Enums;

namespace ImageConvert.ViewModels;

/// <summary>
/// 单个转换文件的 ViewModel，跟踪文件的转换状态并在 ListView 中展示。
/// </summary>
public sealed partial class ConversionItemViewModel : ObservableObject
{
    [ObservableProperty]
    private ConversionItemState _state = ConversionItemState.Pending;

    [ObservableProperty]
    private string _statusText = "待转换";

    [ObservableProperty]
    private string _detailText = "";

    public ConversionItemViewModel(string inputPath)
    {
        InputPath = inputPath;
        FileName = Path.GetFileName(inputPath);
        DetailText = inputPath;
    }

    /// <summary>文件名（含扩展名），用于 UI 显示</summary>
    public string FileName { get; }

    /// <summary>输入文件的完整路径</summary>
    public string InputPath { get; }

    /// <summary>
    /// 重置为待转换状态，用于重新开始批量转换前清空上一次的结果。
    /// </summary>
    public void Reset()
    {
        State = ConversionItemState.Pending;
        StatusText = "待转换";
        DetailText = InputPath;
    }

    /// <summary>
    /// 标记为正在转换中。
    /// </summary>
    public void MarkConverting()
    {
        State = ConversionItemState.Converting;
        StatusText = "转换中";
        DetailText = InputPath;
    }

    /// <summary>
    /// 应用转换结果，更新状态文本和详情文本。
    /// </summary>
    public void ApplyResult(ConversionItemResult result)
    {
        State = result.State;
        StatusText = result.State switch
        {
            ConversionItemState.Succeeded => "已完成",
            ConversionItemState.UnsupportedAnimated => "不支持动图",
            ConversionItemState.Failed => "转换失败",
            _ => "待转换"
        };

        DetailText = result.State == ConversionItemState.Succeeded && !string.IsNullOrWhiteSpace(result.OutputPath)
            ? $"输出文件: {result.OutputPath}"
            : result.Message ?? InputPath;
    }
}
