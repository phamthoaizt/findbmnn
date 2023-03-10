using BitMiracle.Docotic.Pdf;
using Org.BouncyCastle.Ocsp;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using Tesseract;


namespace TestOrcPDF
{
    
    internal class Program
    {
        private string OrcFilePDF(string path_file)
        {
            var documentText = new StringBuilder();
            using (var pdf = new PdfDocument(path_file))
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                path = Path.Combine(path, "data");
                path = path.Replace("file:\\", "");

                System.Console.WriteLine(path);
                using (var engine = new TesseractEngine(path, "vie", EngineMode.LstmOnly))
                {
                    for (int i = 0; i < pdf.PageCount; ++i)
                    {
                        if (documentText.Length > 0)
                            documentText.Append("\r\n\r\n");

                        PdfPage page = pdf.Pages[i];
                        string searchableText = page.GetText();

                        // Simple check if the page contains searchable text.
                        // We do not need to perform OCR in that case.
                        if (!string.IsNullOrEmpty(searchableText.Trim()))
                        {
                            documentText.Append(searchableText);
                            continue;
                        }

                        // This page is not searchable.
                        // Save PDF page as a high-resolution image.
                        PdfDrawOptions options = PdfDrawOptions.Create();
                        options.BackgroundColor = new PdfRgbColor(255, 255, 255);
                        options.HorizontalResolution = 200;
                        options.VerticalResolution = 200;

                        string pageImage = $"page_{i}.png";
                        page.Save(pageImage, options);

                        // Perform OCR
                        using (Pix img = Pix.LoadFromFile(pageImage))
                        {
                            using (Page recognizedPage = engine.Process(img))
                            {
                                Console.WriteLine($"Mean confidence for page #{i}: {recognizedPage.GetMeanConfidence()}");

                                string recognizedText = recognizedPage.GetText();
                                documentText.Append(recognizedText);
                            }
                        }

                        File.Delete(pageImage);
                    }
                }
            }
            return documentText.ToString();
        }
        public static void Main()
        {
            

           
        }
    }
}
/*//Input File Path
string input_path = @"C:\input.pdf";
//Output File Path
string output_path = @"C:\output";
//Tessdata Folder
string training_data = @"C:\tessdata";

PdfReader pdf = new PdfReader(input_path);
PdfDocument pdfDoc = new PdfDocument(pdf);
int n = pdfDoc.GetNumberOfPages();
pdf.Close();
using (IResultRenderer renderer = Tesseract.PdfResultRenderer.CreatePdfRenderer(output_path, training_data, false))
{
    using (renderer.BeginDocument("Serachablepdftest"))
    {
        for (int i = 1; i <= 3; i++)
        {

            GhostscriptWrapper.GeneratePageThumbs(input_path, "example" + i + ".jpg", i, n, 200, 200);
            string configurationFilePath = training_data;
            string configfile = Path.Combine(training_data, "pdf.ttf");
            using (TesseractEngine engine = new TesseractEngine(configurationFilePath, "vie", EngineMode.TesseractAndLstm, configfile))
            {
                using (var img = Pix.LoadFromFile("example" + i + ".jpg"))
                {
                    using (var page = engine.Process(img, "Serachablepdftest"))
                    {
                        renderer.AddPage(page);

                    }
                }
            }
            Console.WriteLine("Page " + i + "done\n");
        }
    }
}
}


}
}
*/