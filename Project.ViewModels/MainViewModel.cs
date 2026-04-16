namespace ImageConvert.ViewModels;

public sealed class MainViewModel
{
    public MainViewModel(ImageConvertViewModel converter)
    {
        Converter = converter;
    }

    public string Title => "ImageConvert";

    public ImageConvertViewModel Converter { get; }
}
