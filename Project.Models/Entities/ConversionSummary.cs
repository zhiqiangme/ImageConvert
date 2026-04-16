namespace ImageConvert.Models.Entities;

public sealed record ConversionSummary(
    int TotalCount,
    int ProcessedCount,
    int ConvertedCount,
    int FailedCount,
    int UnsupportedAnimatedCount,
    bool IsCanceled);
