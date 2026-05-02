using ImageConvert.Models.Enums;

namespace ImageConvert.Models.Entities;

/// <summary>
/// 单个文件的转换结果。
/// </summary>
/// <param name="InputPath">输入文件路径</param>
/// <param name="State">转换状态（成功/失败/不支持动图等）</param>
/// <param name="OutputPath">输出文件路径，仅成功时有值</param>
/// <param name="Message">附加信息，如错误原因或成功提示</param>
public sealed record ConversionItemResult(
    string InputPath,
    ConversionItemState State,
    string? OutputPath,
    string? Message);
