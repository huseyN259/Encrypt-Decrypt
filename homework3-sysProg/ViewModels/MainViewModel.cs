using homework3_sysProg.Commands;
using homework3_sysProg.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;

namespace homework3_sysProg.ViewModels;

public class MainViewModel
{
	MainView mainView;
	CancellationTokenSource cts;

	public RelayCommand FileFromCommand { get; set; }
	public RelayCommand StartCommand { get; set; }
	public RelayCommand CancelCommand { get; set; }

	public List<string> Text { get; set; } = new List<string>();

	public MainViewModel(MainView _mainView)
	{
		mainView = _mainView;

		FileFromCommand = new RelayCommand
		(
			a => FileButtonClick(),
			p => true
		);

		StartCommand = new RelayCommand
		(
			a => ThreadPool.QueueUserWorkItem(e =>
			{
				cts = new CancellationTokenSource();
				EncryptDecryptProcess(cts.Token);
			}),
			p => true
		);

		CancelCommand = new RelayCommand
		(
			a =>
			{
				cts?.Cancel();
			},
			p => true
		);
	}

	public void FileButtonClick()
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "All files|*.*|Text files|*.txt";
		openFileDialog.FilterIndex = 2;

		if (openFileDialog.ShowDialog() == true)
			mainView.txbFile.Text = openFileDialog.FileName;
	}

	public string XORCipher(string data, string key)
	{
		int dataLen = data.Length;
		int keyLen = key.Length;
		char[] result = new char[dataLen];

		for (int i = 0; i < dataLen; ++i)
			result[i] = (char)(data[i] ^ key[i % keyLen]);

		return new string(result);
	}

	public void EncryptDecryptProcess(CancellationToken token)
	{
		bool? isEncrypt = null;
		string filePath = "";
		string passwordKey = "";

		mainView.Dispatcher.Invoke(new Action(() =>
		{
			filePath = mainView.txbFile.Text;
			passwordKey = mainView.txbPassword.Text;

			if (mainView.rbEncrypt.IsChecked == true)
				isEncrypt = true;
			else if (mainView.rbDecrypt.IsChecked == true)
				isEncrypt = false;

			mainView.pbLoading.Value = 0;
		}));

		if (!string.IsNullOrEmpty(filePath) && isEncrypt == true)
		{
			using (StreamReader sr = new StreamReader(filePath))
			{
				string line;

				Text.Clear();

				while ((line = sr.ReadLine()) != null)
					Text.Add(line);
			}

			File.WriteAllText(filePath, String.Empty);

			using (StreamWriter sw = new StreamWriter(filePath))
			{
				foreach (var item in Text)
				{
					if (token.IsCancellationRequested)
						break;

					Thread.Sleep(500);
					mainView.Dispatcher.Invoke(new Action(() => mainView.pbLoading.Value += 100 / Text.Count));

					var encrypt = XORCipher(item, passwordKey);
					sw.WriteLine(encrypt);
				}
			}

			if (!token.IsCancellationRequested)
			{
				mainView.Dispatcher.Invoke(new Action(() => mainView.pbLoading.Value = 100));

				MessageBox.Show("Encrypt process is successfully.", "", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		if (!string.IsNullOrEmpty(filePath) && isEncrypt == false)
		{
			using (StreamReader sr = new StreamReader(filePath))
			{
				string line;
				Text.Clear();

				while ((line = sr.ReadLine()) != null)
					Text.Add(line);
			}

			File.WriteAllText(filePath, String.Empty);

			using (StreamWriter sw = new StreamWriter(filePath))
			{
				foreach (var item in Text)
				{
					if (token.IsCancellationRequested)
						break;

					Thread.Sleep(500);
					mainView.Dispatcher.Invoke(new Action(() => mainView.pbLoading.Value += 100 / Text.Count));

					var encrypt = XORCipher(item, passwordKey);
					sw.WriteLine(encrypt);
				}
			}
			if (!token.IsCancellationRequested)
			{
				mainView.Dispatcher.Invoke(new Action(() => mainView.pbLoading.Value = 100));

				MessageBox.Show("Decrypt process is successfully.", "", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}
	}
}