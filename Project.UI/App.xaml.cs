using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ImageConvert.Core.Abstractions;
using ImageConvert.Core.Services;
using ImageConvert.ViewModels;
using ImageConvert.UI.Services;

namespace ImageConvert.UI;

/// <summary>
/// 应用程序入口，负责配置依赖注入容器并启动主窗口。
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // 注册核心服务（单例，应用生命周期内共享同一实例）
        services.AddSingleton<IImageConversionService, ImageConversionService>();
        services.AddSingleton<IFileDialogService, WpfFileDialogService>();
        services.AddSingleton<IUserNotificationService, WpfUserNotificationService>();

        // 注册 ViewModel（单例，与服务生命周期一致）
        services.AddSingleton<ImageConvertViewModel>();
        services.AddSingleton<MainViewModel>();

        // 主窗口使用瞬态注册，每次请求创建新实例
        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        // 解析并显示主窗口，MainViewModel 通过构造函数注入
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
