namespace ImageConvert.Models.Enums;

/// <summary>
/// 单个文件的转换状态。
/// </summary>
public enum ConversionItemState
{
    /// <summary>等待转换</summary>
    Pending = 0,

    /// <summary>正在转换中</summary>
    Converting = 1,

    /// <summary>转换成功</summary>
    Succeeded = 2,

    /// <summary>因文件为动态 WebP 而跳过</summary>
    UnsupportedAnimated = 3,

    /// <summary>转换失败</summary>
    Failed = 4
}
