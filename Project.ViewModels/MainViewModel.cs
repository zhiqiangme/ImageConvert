namespace ImageConvert.ViewModels;

/// <summary>
/// 应用程序主 ViewModel，作为顶层 DataContext 暴露标题和图片转换子 ViewModel。
/// </summary>
public sealed class MainViewModel
{
    public MainViewModel(ImageConvertViewModel converter)
    {
        Converter = converter;
    }

    /// <summary>窗口标题</summary>
    public string Title => "ImageConvert";

    /// <summary>图片转换功能的 ViewModel，XAML 中通过 Converter.* 路径绑定</summary>
    public ImageConvertViewModel Converter { get; }
}
