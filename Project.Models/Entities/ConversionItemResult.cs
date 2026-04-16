using ImageConvert.Models.Enums;

namespace ImageConvert.Models.Entities;

public sealed record ConversionItemResult(
    string InputPath,
    ConversionItemState State,
    string? OutputPath,
    string? Message);
