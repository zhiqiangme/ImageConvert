using System.Windows;
using ImageConvert.ViewModels;

namespace ImageConvert.UI;

/// <summary>
/// 主窗口，通过构造函数注入 MainViewModel 作为 DataContext。
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
