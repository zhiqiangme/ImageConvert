namespace ImageConvert.Core.Exceptions;

/// <summary>
/// 当输入文件为动态 WebP 时抛出此异常，当前版本仅支持静态 WebP。
/// </summary>
public sealed class AnimatedWebpNotSupportedException : Exception
{
    public AnimatedWebpNotSupportedException(string inputPath)
        : base($"文件 “{Path.GetFileName(inputPath)}” 是动图 WebP，当前版本暂不支持。")
    {
    }
}
