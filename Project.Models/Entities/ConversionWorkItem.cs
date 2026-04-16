namespace ImageConvert.Models.Entities;

public sealed record ConversionWorkItem(string InputPath)
{
    public string FileName => Path.GetFileName(InputPath);
}
