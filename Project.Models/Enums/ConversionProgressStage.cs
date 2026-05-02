namespace ImageConvert.Models.Enums;

/// <summary>
/// 转换进度的阶段标识。
/// </summary>
public enum ConversionProgressStage
{
    /// <summary>文件开始处理</summary>
    Started = 0,

    /// <summary>文件处理完成（成功或失败）</summary>
    Completed = 1
}
