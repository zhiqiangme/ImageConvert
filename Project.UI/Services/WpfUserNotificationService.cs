using System.Windows;
using ImageConvert.Core.Abstractions;

namespace ImageConvert.UI.Services;

public sealed class WpfUserNotificationService : IUserNotificationService
{
    public void ShowInfo(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
