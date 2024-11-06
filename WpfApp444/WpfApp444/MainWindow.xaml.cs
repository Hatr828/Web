using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;

namespace WpfApp444
{
    public partial class MainWindow : Window
    {
        private string currentFilePath = string.Empty;
        private readonly FileManager _fileManager;
        private readonly FileManagerUI _fileManagerUI; 

        public MainWindow()
        {
            InitializeComponent();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/log.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("App started");

            _fileManager = new FileManager();
            _fileManagerUI = new FileManagerUI(_fileManager); 
        }

        private void LoadFile(object sender, RoutedEventArgs e)
        {
            try
            {
                _fileManagerUI.OpenFile(RichTextEditor, TextBlock, ref currentFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load file.");
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            try
            {
                _fileManagerUI.SaveFile(RichTextEditor, TextBlock, ref currentFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save file.");
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteFile(object sender, RoutedEventArgs e)
        {
            try
            {
                _fileManagerUI.DeleteFile(ref currentFilePath, TextBlock);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete file.");
                MessageBox.Show($"Error deleting file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenameFile(object sender, RoutedEventArgs e)
        {
            try
            {
                _fileManagerUI.RenameFile(ref currentFilePath, TextBlock);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to rename file.");
                MessageBox.Show($"Error renaming file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeFont(object sender, RoutedEventArgs e)
        {
            try
            {
                _fileManagerUI.ChangeFont(RichTextEditor);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to change font.");
                MessageBox.Show($"Error changing font: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeColor(object sender, RoutedEventArgs e)
        {
            try
            {
                _fileManagerUI.ChangeColor(RichTextEditor);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to change color.");
                MessageBox.Show($"Error changing color: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ItalicizeText(object sender, RoutedEventArgs e)
        {
            try
            {
                _fileManagerUI.ItalicizeText(RichTextEditor);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to italicize text.");
                MessageBox.Show($"Error italicizing text: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddBulletList(object sender, RoutedEventArgs e)
        {
            _fileManagerUI.AddBulletList(RichTextEditor);
        }

        private void InsertImage(object sender, RoutedEventArgs e)
        {
            _fileManagerUI.InsertImage(RichTextEditor);
        }

        private void ChangeBackgroundColor(object sender, RoutedEventArgs e)
        {
            _fileManagerUI.ChangeBackgroundColor(RichTextEditor);
        }

        private void NewFile(object sender, RoutedEventArgs e)
        {
            try
            {
                _fileManagerUI.CreateNewFile(RichTextEditor, TextBlock, ref currentFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create a new file.");
                System.Windows.MessageBox.Show($"Error creating new file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Log.Information("App closing");
            Log.CloseAndFlush(); 
        }
    }
}
