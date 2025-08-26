using OmronLibPlcTagComm.Services;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OmronLibPlcTagComm;

public partial class MainWindow : Window
{
	ViewModels.MainWindow_VM viewModel;

	public MainWindow()
    {
        InitializeComponent();
		viewModel = new ViewModels.MainWindow_VM(new MessageService());
		this.DataContext = viewModel;
		viewModel.StatusText = "App Started";
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{


	}
}