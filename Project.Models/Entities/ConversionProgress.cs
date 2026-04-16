using ImageConvert.Models.Enums;

namespace ImageConvert.Models.Entities;

public sealed record ConversionProgress(
    ConversionProgressStage Stage,
    int CurrentIndex,
    int TotalCount,
    ConversionWorkItem Item,
    ConversionItemResult? Result);
