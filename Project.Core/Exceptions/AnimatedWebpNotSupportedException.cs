namespace ImageConvert.Core.Exceptions;

public sealed class AnimatedWebpNotSupportedException : Exception
{
    public AnimatedWebpNotSupportedException(string inputPath)
        : base($"文件“{Path.GetFileName(inputPath)}”是动图 WebP，当前版本暂不支持。")
    {
    }
}
