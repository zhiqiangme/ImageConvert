using ImageMagick;
using ImageConvert.Core.Abstractions;
using ImageConvert.Core.Exceptions;
using ImageConvert.Models.Entities;
using ImageConvert.Models.Enums;

namespace ImageConvert.Core.Services;

public sealed class ImageConversionService : IImageConversionService
{
    public Task<ConversionSummary> ConvertAsync(
        IReadOnlyList<ConversionWorkItem> items,
        IProgress<ConversionProgress>? progress,
        CancellationToken cancellationToken)
    {
        return Task.Run(() => ConvertInternal(items, progress, cancellationToken), CancellationToken.None);
    }

    private static ConversionSummary ConvertInternal(
        IReadOnlyList<ConversionWorkItem> items,
        IProgress<ConversionProgress>? progress,
        CancellationToken cancellationToken)
    {
        var processed = 0;
        var converted = 0;
        var failed = 0;
        var unsupportedAnimated = 0;

        for (var index = 0; index < items.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var item = items[index];
            progress?.Report(new ConversionProgress(
                ConversionProgressStage.Started,
                index + 1,
                items.Count,
                item,
                null));

            ConversionItemResult result;

            try
            {
                var outputPath = ConvertSingle(item.InputPath, cancellationToken);
                result = new ConversionItemResult(item.InputPath, ConversionItemState.Succeeded, outputPath, "转换成功");
                converted++;
            }
            catch (AnimatedWebpNotSupportedException ex)
            {
                result = new ConversionItemResult(item.InputPath, ConversionItemState.UnsupportedAnimated, null, ex.Message);
                unsupportedAnimated++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                result = new ConversionItemResult(item.InputPath, ConversionItemState.Failed, null, ex.Message);
                failed++;
            }

            processed++;
            progress?.Report(new ConversionProgress(
                ConversionProgressStage.Completed,
                index + 1,
                items.Count,
                item,
                result));
        }

        return new ConversionSummary(
            items.Count,
            processed,
            converted,
            failed,
            unsupportedAnimated,
            false);
    }

    private static string ConvertSingle(string inputPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("找不到待转换文件。", inputPath);
        }

        using var collection = new MagickImageCollection();
        collection.Ping(inputPath);
        if (collection.Count > 1)
        {
            throw new AnimatedWebpNotSupportedException(inputPath);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var outputPath = GetUniqueOutputPath(inputPath);
        using var image = new MagickImage(inputPath);
        image.Format = MagickFormat.Png;
        image.Write(outputPath);
        return outputPath;
    }

    private static string GetUniqueOutputPath(string inputPath)
    {
        var directory = Path.GetDirectoryName(inputPath) ?? AppContext.BaseDirectory;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputPath);
        var outputPath = Path.Combine(directory, $"{fileNameWithoutExtension}.png");
        var index = 1;

        while (File.Exists(outputPath))
        {
            outputPath = Path.Combine(directory, $"{fileNameWithoutExtension}_{index}.png");
            index++;
        }

        return outputPath;
    }
}
