namespace ImageConvert.Models.Entities;

/// <summary>
/// 转换工作项，表示一个待转换的 WebP 文件。
/// </summary>
/// <param name="InputPath">输入文件的完整路径</param>
public sealed record ConversionWorkItem(string InputPath)
{
    /// <summary>
    /// 从完整路径中提取的文件名（含扩展名），用于 UI 显示。
    /// </summary>
    public string FileName => Path.GetFileName(InputPath);
}
