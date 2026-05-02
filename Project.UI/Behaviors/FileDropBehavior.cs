using System.Windows;
using System.Windows.Input;

namespace ImageConvert.UI.Behaviors;

/// <summary>
/// 文件拖拽附加行为，为 UIElement 启用拖拽文件的能力。
/// 通过 IsEnabled 和 DropCommand 两个附加属性在 XAML 中声明式配置。
/// </summary>
public static class FileDropBehavior
{
    /// <summary>是否启用拖拽功能</summary>
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(FileDropBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    /// <summary>拖拽释放时执行的命令，参数为文件路径数组</summary>
    public static readonly DependencyProperty DropCommandProperty =
        DependencyProperty.RegisterAttached(
            "DropCommand",
            typeof(ICommand),
            typeof(FileDropBehavior),
            new PropertyMetadata(null));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static ICommand? GetDropCommand(DependencyObject obj) => (ICommand?)obj.GetValue(DropCommandProperty);

    public static void SetDropCommand(DependencyObject obj, ICommand? value) => obj.SetValue(DropCommandProperty, value);

    /// <summary>
    /// IsEnabled 属性变化时，订阅或取消拖拽事件。
    /// </summary>
    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.AllowDrop = true;
            element.PreviewDragOver += Element_OnPreviewDragOver;
            element.Drop += Element_OnDrop;
        }
        else
        {
            element.AllowDrop = false;
            element.PreviewDragOver -= Element_OnPreviewDragOver;
            element.Drop -= Element_OnDrop;
        }
    }

    /// <summary>
    /// 拖拽悬停时，根据数据格式决定显示复制还是禁止图标。
    /// </summary>
    private static void Element_OnPreviewDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    /// <summary>
    /// 文件释放时，提取文件路径并执行绑定的命令。
    /// </summary>
    private static void Element_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not DependencyObject target)
        {
            return;
        }

        var command = GetDropCommand(target);
        if (command is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        var files = e.Data.GetData(DataFormats.FileDrop) as string[] ?? Array.Empty<string>();
        if (command.CanExecute(files))
        {
            command.Execute(files);
        }
    }
}
