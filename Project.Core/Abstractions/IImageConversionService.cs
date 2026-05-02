using ImageConvert.Models.Entities;

namespace ImageConvert.Core.Abstractions;

/// <summary>
/// 图片转换服务接口，负责将 WebP 文件批量转换为 PNG。
/// </summary>
public interface IImageConversionService
{
    /// <summary>
    /// 批量转换 WebP 文件为 PNG。
    /// </summary>
    /// <param name="items">待转换的工作项列表</param>
    /// <param name="progress">进度回调，用于报告每个文件的转换状态</param>
    /// <param name="cancellationToken">取消令牌，用于中断转换流程</param>
    /// <returns>转换结果摘要，包含成功/失败/取消等统计信息</returns>
    Task<ConversionSummary> ConvertAsync(
        IReadOnlyList<ConversionWorkItem> items,
        IProgress<ConversionProgress>? progress,
        CancellationToken cancellationToken);
}
