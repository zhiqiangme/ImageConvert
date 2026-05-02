using Microsoft.Win32;
using ImageConvert.Core.Abstractions;

namespace ImageConvert.UI.Services;

/// <summary>
/// WPF 平台的文件选择对话框服务，使用 Win32 OpenFileDialog。
/// </summary>
public sealed class WpfFileDialogService : IFileDialogService
{
    public Task<IReadOnlyList<string>> PickWebpFilesAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择 WebP 文件",
            Filter = "WebP files (*.webp)|*.webp",
            Multiselect = true,
            CheckFileExists = true
        };

        var result = dialog.ShowDialog() == true
            ? dialog.FileNames
            : Array.Empty<string>();

        return Task.FromResult<IReadOnlyList<string>>(result);
    }
}
