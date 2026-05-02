namespace ImageConvert.Models.Entities;

/// <summary>
/// 批量转换的最终结果摘要。
/// </summary>
/// <param name="TotalCount">文件总数</param>
/// <param name="ProcessedCount">已处理的文件数</param>
/// <param name="ConvertedCount">成功转换的文件数</param>
/// <param name="FailedCount">转换失败的文件数</param>
/// <param name="UnsupportedAnimatedCount">因动图 WebP 而跳过的文件数</param>
/// <param name="IsCanceled">是否因用户取消而中断</param>
public sealed record ConversionSummary(
    int TotalCount,
    int ProcessedCount,
    int ConvertedCount,
    int FailedCount,
    int UnsupportedAnimatedCount,
    bool IsCanceled);
