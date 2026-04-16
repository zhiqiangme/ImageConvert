namespace ImageConvert.Core.Abstractions;

public interface IFileDialogService
{
    Task<IReadOnlyList<string>> PickWebpFilesAsync();
}
