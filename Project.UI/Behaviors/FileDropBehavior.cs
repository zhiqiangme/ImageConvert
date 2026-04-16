using System.Windows;
using System.Windows.Input;

namespace ImageConvert.UI.Behaviors;

public static class FileDropBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(FileDropBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

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

    private static void Element_OnPreviewDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

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
