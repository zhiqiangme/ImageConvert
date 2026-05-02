using ImageConvert.Models.Enums;

namespace ImageConvert.Models.Entities;

/// <summary>
/// 转换进度通知，在每个文件开始和完成时通过 IProgress 回调上报。
/// </summary>
/// <param name="Stage">进度阶段（开始/完成）</param>
/// <param name="CurrentIndex">当前处理的文件序号（从 1 开始）</param>
/// <param name="TotalCount">文件总数</param>
/// <param name="Item">当前正在处理的工作项</param>
/// <param name="Result">转换结果，仅在 Completed 阶段有值</param>
public sealed record ConversionProgress(
    ConversionProgressStage Stage,
    int CurrentIndex,
    int TotalCount,
    ConversionWorkItem Item,
    ConversionItemResult? Result);
