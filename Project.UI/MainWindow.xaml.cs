using System.Windows;
using ImageConvert.ViewModels;

namespace ImageConvert.UI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
