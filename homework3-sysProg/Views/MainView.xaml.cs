using homework3_sysProg.ViewModels;
using System;
using System.Windows;

namespace homework3_sysProg.Views;

public partial class MainView : Window
{
	public MainViewModel MainViewModel { get; set; }

	public MainView()
	{
		InitializeComponent();

		try
		{
			MainViewModel = new MainViewModel(this);
			DataContext = MainViewModel;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "Exception occured...", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}