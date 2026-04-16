using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ImageConvert.Core.Abstractions;
using ImageConvert.Core.Services;
using ImageConvert.ViewModels;
using ImageConvert.UI.Services;

namespace ImageConvert.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services.AddSingleton<IImageConversionService, ImageConversionService>();
        services.AddSingleton<IFileDialogService, WpfFileDialogService>();
        services.AddSingleton<IUserNotificationService, WpfUserNotificationService>();
        services.AddSingleton<ImageConvertViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
