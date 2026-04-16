using ImageConvert.Models.Entities;

namespace ImageConvert.Core.Abstractions;

public interface IImageConversionService
{
    Task<ConversionSummary> ConvertAsync(
        IReadOnlyList<ConversionWorkItem> items,
        IProgress<ConversionProgress>? progress,
        CancellationToken cancellationToken);
}
