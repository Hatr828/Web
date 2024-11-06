using System.IO;
using System.Windows.Documents;
using System.Windows.Markup;
using Serilog;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Packaging;
using Body = DocumentFormat.OpenXml.Wordprocessing.Body;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Document = DocumentFormat.OpenXml.Wordprocessing.Document;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace WpfApp444
{
    public class FileManager
    {
        private const string _folder = "Texts";

        public FileManager()
        {
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
                Log.Information($"Directory '{_folder}' was created.");
            }
        }

        public void SaveDocumentXaml(string fileName, FlowDocument document)
        {
            string filePath = Path.Combine(_folder, fileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                XamlWriter.Save(document, fs);
            }
            Log.Information($"Document '{fileName}' was successfully saved as XAML.");
        }

        public void SaveDocumentText(string fileName, RichTextBox richTextEditor)
        {
            string filePath = Path.Combine(_folder, fileName);
            TextRange textRange = new TextRange(richTextEditor.Document.ContentStart, richTextEditor.Document.ContentEnd);
            File.WriteAllText(filePath, textRange.Text);
            Log.Information($"Document {fileName} was successfully saved as TXT.");
        }

        public void SaveDocumentPdf(string fileName, RichTextBox richTextEditor)
        {
            string filePath = Path.Combine(_folder, fileName);
            TextRange textRange = new TextRange(richTextEditor.Document.ContentStart, richTextEditor.Document.ContentEnd);

            using (var pdfWriter = new PdfWriter(filePath))
            {
                using (var pdfDoc = new PdfDocument(pdfWriter))
                {
                    using (var document = new iText.Layout.Document(pdfDoc))
                    {
                        document.Add(new iText.Layout.Element.Paragraph(textRange.Text));
                    }
                }
            }

            Log.Information($"Document {fileName} was successfully saved as PDF.");
        }

        public void SaveDocumentDocx(string fileName, RichTextBox richTextEditor)
        {
            string filePath = Path.Combine(_folder, fileName);
            TextRange textRange = new TextRange(richTextEditor.Document.ContentStart, richTextEditor.Document.ContentEnd);

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                body.AppendChild(new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(textRange.Text))));
            }

            Log.Information($"Document {fileName} was successfully saved as DOCX.");
        }

        public FlowDocument LoadDocumentXaml(string fileName)
        {
            string filePath = Path.Combine(_folder, fileName);
            if (!File.Exists(filePath))
            {
                Log.Warning($"Attempted to load document '{fileName}', but it was not found.");
                throw new FileNotFoundException($"Document '{fileName}' not found.");
            }
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                return (FlowDocument)XamlReader.Load(fs);
            }
        }

        public void SaveFile(string fileName, string content)
        {
            string filePath = Path.Combine(_folder, fileName);
            File.WriteAllText(filePath, content);
            Log.Information($"File '{fileName}' was successfully saved.");
        }

        public string LoadFile(string fileName)
        {
            string filePath = Path.Combine(_folder, fileName);
            if (!File.Exists(filePath))
            {
                Log.Warning($"File '{fileName}' not found.");
                throw new FileNotFoundException();
            }
            string content = File.ReadAllText(filePath);
            Log.Information($"File '{fileName}' was successfully read.");
            return content;
        }

        public void DeleteFile(string fileName)
        {
            string filePath = Path.Combine(_folder, fileName);
            if (!File.Exists(filePath))
            {
                Log.Warning($"File '{fileName}' not found for deletion.");
                throw new FileNotFoundException();
            }
            File.Delete(filePath);
            Log.Information($"File '{fileName}' was successfully deleted.");
        }

        public void RenameFile(string oldFileName, string newFileName)
        {
            string oldFilePath = Path.Combine(_folder, oldFileName);
            string newFilePath = Path.Combine(_folder, newFileName);

            if (!File.Exists(oldFilePath))
            {
                Log.Warning($"File '{oldFileName}' not found for renaming.");
                throw new FileNotFoundException();
            }

            File.Move(oldFilePath, newFilePath);
            Log.Information($"File renamed from '{oldFileName}' to '{newFileName}'.");
        }

        public string GetFileByName(string fileName)
        {
            string filePath = Path.Combine(_folder, fileName);

            try
            {
                if (!File.Exists(filePath))
                {
                    Log.Warning($"Attempted to read a file that does not exist: '{fileName}'.");
                    throw new FileNotFoundException("File not found.");
                }

                string content = File.ReadAllText(filePath);
                Log.Information($"File '{fileName}' was successfully read.");
                return content;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error when opening file '{fileName}'.");
                throw;
            }
        }
    }
}