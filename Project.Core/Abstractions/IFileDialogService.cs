namespace ImageConvert.Core.Abstractions;

/// <summary>
/// 文件选择对话框服务接口，由平台层（WPF）实现。
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// 打开文件选择对话框，让用户选择 WebP 文件。
    /// </summary>
    /// <returns>用户选择的文件路径列表；如果取消则返回空列表</returns>
    Task<IReadOnlyList<string>> PickWebpFilesAsync();
}
