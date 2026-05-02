using System.Windows;
using ImageConvert.Core.Abstractions;

namespace ImageConvert.UI.Services;

/// <summary>
/// WPF 平台的用户通知服务，使用 MessageBox 弹出提示。
/// </summary>
public sealed class WpfUserNotificationService : IUserNotificationService
{
    public void ShowInfo(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
