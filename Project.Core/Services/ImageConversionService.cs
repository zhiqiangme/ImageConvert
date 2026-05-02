using ImageMagick;
using ImageConvert.Core.Abstractions;
using ImageConvert.Core.Exceptions;
using ImageConvert.Models.Entities;
using ImageConvert.Models.Enums;

namespace ImageConvert.Core.Services;

/// <summary>
/// 图片转换服务，使用 Magick.NET 将 WebP 文件转换为 PNG。
/// </summary>
public sealed class ImageConversionService : IImageConversionService
{
    public Task<ConversionSummary> ConvertAsync(
        IReadOnlyList<ConversionWorkItem> items,
        IProgress<ConversionProgress>? progress,
        CancellationToken cancellationToken)
    {
        // 将实际转换工作放到后台线程执行，避免阻塞 UI。
        // 传递 cancellationToken 使得取消时可以即时取消 Task 的调度。
        return Task.Run(() => ConvertInternal(items, progress, cancellationToken), cancellationToken);
    }

    /// <summary>
    /// 串行执行批量转换，逐个处理文件并在每个文件前后上报进度。
    /// </summary>
    private static ConversionSummary ConvertInternal(
        IReadOnlyList<ConversionWorkItem> items,
        IProgress<ConversionProgress>? progress,
        CancellationToken cancellationToken)
    {
        var processed = 0;
        var converted = 0;
        var failed = 0;
        var unsupportedAnimated = 0;

        try
        {
            for (var index = 0; index < items.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = items[index];

                // 上报"开始处理"进度，放在 try-catch 中防止回调异常中断转换
                try
                {
                    progress?.Report(new ConversionProgress(
                        ConversionProgressStage.Started,
                        index + 1,
                        items.Count,
                        item,
                        null));
                }
                catch (Exception)
                {
                    // 进度回调失败不应中断转换流程
                }

                ConversionItemResult result;

                try
                {
                    var outputPath = ConvertSingle(item.InputPath, cancellationToken);
                    result = new ConversionItemResult(item.InputPath, ConversionItemState.Succeeded, outputPath, "转换成功");
                    converted++;
                }
                catch (AnimatedWebpNotSupportedException ex)
                {
                    // 动态 WebP 标记为"不支持"，不计入失败，继续处理后续文件
                    result = new ConversionItemResult(item.InputPath, ConversionItemState.UnsupportedAnimated, null, ex.Message);
                    unsupportedAnimated++;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // 其他异常标记为失败，继续处理后续文件
                    result = new ConversionItemResult(item.InputPath, ConversionItemState.Failed, null, ex.Message);
                    failed++;
                }

                processed++;

                // 上报"处理完成"进度
                try
                {
                    progress?.Report(new ConversionProgress(
                        ConversionProgressStage.Completed,
                        index + 1,
                        items.Count,
                        item,
                        result));
                }
                catch (Exception)
                {
                    // 进度回调失败不应中断转换流程
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 用户取消：返回已处理的部分结果，并标记 IsCanceled = true
            return new ConversionSummary(
                items.Count,
                processed,
                converted,
                failed,
                unsupportedAnimated,
                true);
        }

        // 正常完成：所有文件均已处理
        return new ConversionSummary(
            items.Count,
            processed,
            converted,
            failed,
            unsupportedAnimated,
            false);
    }

    /// <summary>
    /// 转换单个 WebP 文件为 PNG。
    /// 先检测是否为动态 WebP，若是则抛出 AnimatedWebpNotSupportedException；
    /// 否则加载图像并写入同目录下的 PNG 文件（自动避让重名）。
    /// </summary>
    private static string ConvertSingle(string inputPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("找不到待转换文件。", inputPath);
        }

        // 一次性读取文件并检测帧数，避免重复 I/O
        using var collection = new MagickImageCollection();
        collection.Read(inputPath);
        if (collection.Count > 1)
        {
            throw new AnimatedWebpNotSupportedException(inputPath);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // 从已加载的 collection 中取第一帧进行转换，无需再次读取文件
        var outputPath = GetUniqueOutputPath(inputPath);
        using var image = new MagickImage(collection[0]);
        image.Format = MagickFormat.Png;
        image.Write(outputPath);
        return outputPath;
    }

    /// <summary>
    /// 生成唯一的输出文件路径。如果目标 PNG 已存在，则追加 _1、_2 等后缀直到不重名。
    /// </summary>
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
