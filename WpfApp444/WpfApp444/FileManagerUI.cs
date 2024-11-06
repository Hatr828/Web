using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Serilog;
using System.IO;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;
using System.Text;
using System.Windows.Media;
using FontFamily = System.Windows.Media.FontFamily;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using RichTextBox = System.Windows.Controls.RichTextBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using Path = System.IO.Path;
using ListItem = System.Windows.Documents.ListItem;
using Paragraph = System.Windows.Documents.Paragraph;
using Run = System.Windows.Documents.Run;

namespace WpfApp444
{
    public class FileManagerUI
    {
        private readonly FileManager _fileManager;

        public FileManagerUI(FileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public void OpenFile(RichTextBox richTextEditor, TextBlock textBlock, ref string currentFilePath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XAML Documents (*.xaml)|*.xaml|Text Documents (*.txt)|*.txt|PDF Documents (*.pdf)|*.pdf|Word Documents (*.docx)|*.docx",
                DefaultExt = ".xaml",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Texts")
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                string extension = Path.GetExtension(fileName).ToLower();
                currentFilePath = openFileDialog.FileName;

                try
                {
                    switch (extension)
                    {
                        case ".xaml":
                            FlowDocument xamlDocument = _fileManager.LoadDocumentXaml(fileName);
                            richTextEditor.Document = xamlDocument;
                            break;

                        case ".txt":
                            string textContent = File.ReadAllText(currentFilePath);
                            richTextEditor.Document = new FlowDocument(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(textContent)));
                            break;

                        case ".pdf":
                            string pdfText = LoadPdfContent(currentFilePath);
                            richTextEditor.Document = new FlowDocument(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(pdfText)));
                            break;

                        case ".docx":
                            string docxText = LoadDocxContent(currentFilePath);
                            richTextEditor.Document = new FlowDocument(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(docxText)));
                            break;

                        default:
                            MessageBox.Show("Unsupported file format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                    }

                    textBlock.Text = $"File: {fileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}");
                }
            }
        }

        private string LoadPdfContent(string filePath)
        {
            StringBuilder pdfText = new StringBuilder();

            using (var pdfReader = new PdfReader(filePath))
            using (var pdfDocument = new PdfDocument(pdfReader))
            {
                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                {
                    var page = pdfDocument.GetPage(i);
                    pdfText.Append(PdfTextExtractor.GetTextFromPage(page));
                }
            }

            return pdfText.ToString();
        }

        private string LoadDocxContent(string filePath)
        {
            StringBuilder docxText = new StringBuilder();

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                Body body = wordDoc.MainDocumentPart.Document.Body;

                foreach (var paragraph in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    foreach (var run in paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
                    {
                        foreach (var text in run.Elements<Text>())
                        {
                            docxText.Append(text.Text);
                        }
                    }

                    docxText.AppendLine();
                }
            }

            return docxText.ToString();
        }

        public void SaveFile(RichTextBox richTextEditor, TextBlock textBlock, ref string currentFilePath)
        {
            string fileName;
            string extension;

            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "XAML Documents (*.xaml)|*.xaml|Text Documents (*.txt)|*.txt|PDF Documents (*.pdf)|*.pdf|Word Documents (*.docx)|*.docx",
                    DefaultExt = ".xaml",
                    InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Texts")
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    fileName = System.IO.Path.GetFileName(saveFileDialog.FileName);
                    currentFilePath = saveFileDialog.FileName;
                    extension = Path.GetExtension(fileName);
                }
                else
                {
                    return;
                }
            }
            else
            {
                fileName = System.IO.Path.GetFileName(currentFilePath);
                extension = Path.GetExtension(currentFilePath);
            }

            switch (extension.ToLower())
            {
                case ".xaml":
                    _fileManager.SaveDocumentXaml(fileName, richTextEditor.Document);
                    break;
                case ".txt":
                    _fileManager.SaveDocumentText(fileName, richTextEditor);
                    break;
                case ".pdf":
                    _fileManager.SaveDocumentPdf(fileName, richTextEditor);
                    break;
                case ".docx":
                    _fileManager.SaveDocumentDocx(fileName, richTextEditor);
                    break;
                default:
                    MessageBox.Show("Unsupported file format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            textBlock.Text = $"File: {fileName}";
        }

        public void CreateNewFile(RichTextBox richTextEditor, TextBlock textBlock, ref string currentFilePath)
        {
            richTextEditor.Document.Blocks.Clear();
            currentFilePath = string.Empty;
            textBlock.Text = "File: Untitled";
        }

        public void DeleteFile(ref string currentFilePath, TextBlock textBlock)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                MessageBox.Show("Please save the file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string fileName = System.IO.Path.GetFileName(currentFilePath);
            try
            {
                _fileManager.DeleteFile(fileName);
                currentFilePath = string.Empty;
                textBlock.Text = "File: Untitled";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting file: {ex.Message}");
            }
        }

        public void RenameFile(ref string currentFilePath, TextBlock textBlock)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                MessageBox.Show("Please save the file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "XAML Documents (*.xaml)|*.xaml",
                DefaultExt = ".xaml",
                FileName = System.IO.Path.GetFileName(currentFilePath),
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Texts")
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string newFileName = System.IO.Path.GetFileName(saveFileDialog.FileName);
                try
                {
                    _fileManager.RenameFile(System.IO.Path.GetFileName(currentFilePath), newFileName);
                    currentFilePath = saveFileDialog.FileName;
                    textBlock.Text = $"File: {newFileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error renaming file: {ex.Message}");
                }
            }
        }

        public void ChangeFont(RichTextBox richTextEditor)
        {
            var fontDialog = new FontDialog();
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                double fontSize = Convert.ToDouble(fontDialog.Font.Size);

                if (richTextEditor.Selection != null)
                {
                    richTextEditor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(fontDialog.Font.Name));
                    richTextEditor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
                }
            }
        }

        public void ChangeColor(RichTextBox richTextEditor)
        {
            try
            {
                using (ColorDialog colorDialog = new ColorDialog())
                {
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (!richTextEditor.Selection.IsEmpty)
                        {
                            richTextEditor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to change color.");
                MessageBox.Show($"Error changing color: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ItalicizeText(RichTextBox richTextEditor)
        {
            try
            {
                if (!richTextEditor.Selection.IsEmpty)
                {
                    var currentFontStyle = richTextEditor.Selection.GetPropertyValue(TextElement.FontStyleProperty);
                    var newFontStyle = FontStyles.Italic;
                    richTextEditor.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, newFontStyle);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to italicize text.");
                MessageBox.Show($"Error italicizing text: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddBulletList(RichTextBox richTextEditor)
        {
            try
            {
                var paragraph1 = new Paragraph(new Run("Bullet point 1"));
                var paragraph2 = new Paragraph(new Run("Bullet point 2"));

                var listItem1 = new ListItem(paragraph1);
                var listItem2 = new ListItem(paragraph2);

                var list = new List();
                list.ListItems.Add(listItem1);
                list.ListItems.Add(listItem2);

                richTextEditor.Document.Blocks.Add(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding bullet list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void InsertImage(RichTextBox richTextEditor)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                    DefaultExt = ".jpg"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string imagePath = openFileDialog.FileName;

                    var image = new System.Windows.Controls.Image
                    {
                        Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath)),
                        Width = 200,
                        Height = 200
                    };

                    var inline = new InlineUIContainer(image);
                    var paragraph = new Paragraph(inline);
                    richTextEditor.Document.Blocks.Add(paragraph);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inserting image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ChangeBackgroundColor(RichTextBox richTextEditor)
        {
            try
            {
                using (ColorDialog colorDialog = new ColorDialog())
                {
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        var selectedColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                            colorDialog.Color.A,
                            colorDialog.Color.R,
                            colorDialog.Color.G,
                            colorDialog.Color.B));

                        richTextEditor.Background = selectedColor;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing background color: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
