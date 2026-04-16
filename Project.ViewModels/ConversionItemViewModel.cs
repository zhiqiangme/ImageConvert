using CommunityToolkit.Mvvm.ComponentModel;
using ImageConvert.Models.Entities;
using ImageConvert.Models.Enums;

namespace ImageConvert.ViewModels;

public sealed partial class ConversionItemViewModel : ObservableObject
{
    [ObservableProperty]
    private ConversionItemState _state = ConversionItemState.Pending;

    [ObservableProperty]
    private string _statusText = "待转换";

    [ObservableProperty]
    private string _detailText;

    public ConversionItemViewModel(string inputPath)
    {
        InputPath = inputPath;
        FileName = Path.GetFileName(inputPath);
        DetailText = inputPath;
    }

    public string FileName { get; }

    public string InputPath { get; }

    public void Reset()
    {
        State = ConversionItemState.Pending;
        StatusText = "待转换";
        DetailText = InputPath;
    }

    public void MarkConverting()
    {
        State = ConversionItemState.Converting;
        StatusText = "转换中";
        DetailText = InputPath;
    }

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
