using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
// lib thao tac file word
using Aspose.Words;
using System.Diagnostics;
using System.Text.RegularExpressions;
using BitMiracle.Docotic.Pdf;
using Org.BouncyCastle.Ocsp;
using System.Reflection;
using System.Text;
using Tesseract;

namespace findbmnn
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent();
            Init();
        }

        List<string> listFileDoc;
        List<string> listKeys;
        List<string> listFilePDF;
        //Tessdata Folder
        string training_data = Directory.GetCurrentDirectory() + @"data\vie.traineddata";

        private void Init()
        {
            listFileDoc = new List<string>();
            listKeys = new List<string>();
            listFilePDF = new List<string>();
        }


        #region code xử lý giao diện

        // hàm hiển thị log
        public void AppendTextBox(string value, int stylevalue)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string, int>(AppendTextBox), new object[] { value, stylevalue });
                return;
            }

            if (stylevalue == 1)
            {
                richTextBoxLog.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + value + Environment.NewLine);
                richTextBoxLog.AppendText("- - - - - - - - - - - - - - - - - - - " + Environment.NewLine);
                richTextBoxLog.ForeColor = Color.Green;
                richTextBoxLog.ScrollToCaret();
            }
            if (stylevalue == 0)
            {
                richTextBoxLog.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + value + Environment.NewLine);
                richTextBoxLog.AppendText("- - - - - - - - - - - - - - - - - - - " + Environment.NewLine);
                richTextBoxLog.ForeColor = Color.Black;
                richTextBoxLog.ScrollToCaret();
            }
            if (stylevalue == 3)
            {
                richTextBoxLog.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + value + Environment.NewLine);
                richTextBoxLog.AppendText("- - - - - - - - - - - - - - - - - - - " + Environment.NewLine);
                richTextBoxLog.ForeColor = Color.Red;
                richTextBoxLog.ScrollToCaret();
            }
        }
        // xử lý nút tìm kiếm file pdf 
        private async void buttonSearchpdf_Click(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();

            buttonSearchpdf.Enabled = false;
            // kiểm tra đã tồn tại file kết quả load chưa
            string currentDirectory = Directory.GetCurrentDirectory();
            string fullpath = currentDirectory + @"\save_data\resultLoadFilePDF.txt";

            if (!File.Exists(fullpath))
            {
                MessageBox.Show("Chưa tồn tại file để load dữ liệu", "Cảnh báo", MessageBoxButtons.OK);

            }
            else
            {
                // lấy keyword để tìm kiếm
                CheckKeyWords();
                List<string> listpathfilepdf = File.ReadAllLines(fullpath).ToList();
                // tạo đường dẫn chứa kết quả
                string pathResultSearchString = currentDirectory + @"\save_data\resultSearchStringInPDF" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt";
                File.Create(pathResultSearchString);

                foreach (string pathfilepdf in listpathfilepdf)
                {
                    // khởi tạo task đối với từng file word
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            string content = FindContentPDF(pathfilepdf);
                            bool detectkey = false;
                            foreach (string searchString in listKeys)
                            {
                                Regex myRegex = new Regex(searchString);
                                var results = myRegex.Matches(content);
                                if (results.Count > 0)
                                {
                                    AppendTextBox("Tìm thấy chuỗi '" + searchString + "' trong file " + pathfilepdf, 1);
                                    // thiết lập file chứa kết quả
                                    detectkey = true;
                                }
                                else
                                {
                                    AppendTextBox("Không tìm thấy chuỗi '" + searchString + "' trong file " + pathfilepdf, 0);
                                }
                            }
                            if (detectkey)
                            {
                                File.AppendAllText(pathResultSearchString, pathfilepdf + Environment.NewLine);
                            }
                        }
                        catch
                        {
                            AppendTextBox("Không đọc được file: " + pathfilepdf, 3);
                        }
                    }));

                }

                await Task.WhenAll(tasks);
                AppendTextBox("Đã tìm kiếm xong!", 1);
                buttonResult.Visible = true;
                buttonSearchpdf.Enabled = true;
            }
        }

        // xử lý nút tìm kiếm

        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();

            buttonSearch.Enabled = false;
            // kiểm tra đã tồn tại file kết quả load chưa
            string currentDirectory = Directory.GetCurrentDirectory();
            string fullpath = currentDirectory + @"\save_data\resultLoadFileWord.txt";

            if (!File.Exists(fullpath))
            {
                MessageBox.Show("Chưa tồn tại file để load dữ liệu", "Cảnh báo", MessageBoxButtons.OK);

            }
            else
            {
                // lấy keyword để tìm kiếm
                CheckKeyWords();
                List<string> listpathfileword = File.ReadAllLines(fullpath).ToList();
                // tạo đường dẫn chứa kết quả
                string pathResultSearchString = currentDirectory + @"\save_data\resultSearchStringInWord " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt";
                File.Create(pathResultSearchString);

                // tạo folder chứa kết quả convert file txt
                string pathsavefoldertxt = CreateFolder("txt_convert_word");
                DeleleAllFile(pathsavefoldertxt);

                foreach (string pathfileword in listpathfileword)
                {
                    // khởi tạo task đối với từng file word
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            Document doc = new Document(pathfileword);
                            string pathfiletxt = pathsavefoldertxt + @"\" + Path.GetFileNameWithoutExtension(pathfileword) + ".txt";
                            AppendTextBox("Đã tạo file " + pathfiletxt, 1);
                            doc.Save(pathfiletxt);
                            string content = File.ReadAllText(pathfiletxt).ToLower();
                            bool detectkey = false;
                            foreach (string searchString in listKeys)
                            {
                                Regex myRegex = new Regex(searchString);
                                var results = myRegex.Matches(content);
                                if (results.Count > 0)
                                {
                                    AppendTextBox("Tìm thấy chuỗi '" + searchString + "' trong file " + pathfileword, 1);
                                    // thiết lập file chứa kết quả
                                    detectkey = true;
                                }
                                else
                                {
                                    AppendTextBox("Không tìm thấy chuỗi '" + searchString + "' trong file " + pathfileword, 0);
                                }
                            }
                            if (detectkey)
                            {
                                File.AppendAllText(pathResultSearchString, pathfileword + Environment.NewLine);
                            }
                        }
                        catch
                        {
                            AppendTextBox("Không đọc được file: " + pathfileword, 3);
                        }
                    }));

                }

                await Task.WhenAll(tasks);
                AppendTextBox("Đã tìm kiếm xong!", 1);
                buttonResult.Visible = true;
                buttonSearch.Enabled = true;
            }
        }


        private async void buttonLoadDisk_Click(object sender, EventArgs e)
        {

            DriveInfo[] allDrives = DriveInfo.GetDrives();
            List<Task> tasks = new List<Task>();

            // lấy thông tin và load file
            AppendTextBox("Lấy thông tin toàn bộ file word trên máy tính", 1);
            foreach (DriveInfo d in allDrives)
            {
                AppendTextBox("Kết quả load file tại ổ: " + d.Name, 1);
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        FindDocs(d.Name, "*.doc", true);
                        FindDocs(d.Name, "*.docx", true);
                        FindDocs(d.Name, "*.pdf", true);
                    }
                    catch
                    {
                        // Do Nothing
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // tạo file lưu path 
            string subPath = CreateFolder("save_data");
            string filenameword = "resultLoadFileWord.txt";
            string filenamepdf = "resultLoadFilePDF.txt";
            List<string> resultFileLoaded = listFileDoc;
            List<string> resultFileLoadPDF = listFilePDF;
            SaveAllText(filenameword, resultFileLoaded);
            SaveAllText(filenamepdf, resultFileLoadPDF);

            buttonLoadDisk.Enabled = false;
        }

        private void buttonResult_Click(object sender, EventArgs e)
        {
            // Lấy thư mục của User trên hệ thống
            string currentDirectory = Directory.GetCurrentDirectory();
            string pathResultSearchString = currentDirectory + @"\save_data";
            if (!Directory.Exists(pathResultSearchString))
            {
                Process.Start(pathResultSearchString);
            }
            else
            {
                AppendTextBox("Không tìm thấy file kết quả", 3);
            }

        }
        #endregion



        #region vùng backend
        // đọc nội dung file pdf
        private string FindContentPDF(string input_path)
        {
            var documentText = new StringBuilder();
            using (var pdf = new PdfDocument(input_path))
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

        // xóa toàn bộ file trong folder
        private void DeleleAllFile(string path_name)
        {
            DirectoryInfo dir = new DirectoryInfo(path_name);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                DeleleAllFile(di.FullName);
                di.Delete();
            }
        }

        // lưu file sau khi load
        private void SaveAllText(string filename, List<string> contentfile)
        {
            // Lấy thư mục của User trên hệ thống
            string currentDirectory = Directory.GetCurrentDirectory();
            string subPath = Path.Combine(currentDirectory, "save_data");
            var fullpath = Path.Combine(subPath, filename);
            if (!Directory.Exists(fullpath))
            {
                using (StreamWriter writer = new StreamWriter(fullpath, true))
                {
                    foreach (string content in contentfile)
                    {
                        writer.WriteLine(content);
                    }
                }
                AppendTextBox("Đã lưu file tại: " + fullpath, 1);
            }
            else
            {
                string[] contentfiles = contentfile.ToArray();
                // File đã tồn tại - xóa, thêm nội dung
                File.Delete(fullpath);
                File.Create(fullpath);
                using (StreamWriter writer = new StreamWriter(fullpath, true))
                {
                    foreach (string content in contentfiles)
                    {
                        writer.WriteLine(content);
                    }
                }
                AppendTextBox("File đã tồn tại", 3);
                AppendTextBox("Đã lưu tiếp tại đường dẫn: " + fullpath, 3);
            }
        }
        private void CheckKeyWords()
        {
            string keywords = richTextBoxKey.Text.Trim().ToLower();
            string[] listKey = GetListKey(keywords);
            AppendTextBox("Tổng số key cần phân tích: " + richTextBoxKey.Text.Trim().ToLower(), 1);
            foreach (string key in listKey)
            {
                AppendTextBox(key, 0);
            }
            listKeys.AddRange(listKey);
        }

        // tao thu muc chua file .txt
        private string CreateFolder(string newname)
        {
            string pathCurr = Directory.GetCurrentDirectory();
            string subPath = Path.Combine(pathCurr, newname);
            if (!Directory.Exists(subPath))
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(subPath);
                return di.FullName;
            }
            return subPath;
        }
        // load list keyword
        private string[] GetListKey(string param)
        {
            string[] arrListStr = param.Split(',');
            return arrListStr;
        }
        // lấy toàn bộ danh sách file
        private void FindDocs(string path, string typeFile, bool searchSubfolders)
        {
            try
            {
                // Lấy danh sách các tệp tin .doc trong thư mục hiện tại
                string[] files = Directory.GetFiles(path, typeFile);
                foreach (string file in files)
                {
                    // Xử lý file .doc tại đây
                    AppendTextBox(file, 0);
                }
                // thêm vào danh sách trả về
                if (typeFile == "*.pdf")
                {
                    listFilePDF.AddRange(files);
                }
                else if (typeFile == "*.doc" || typeFile == "*.docx")
                {
                    listFileDoc.AddRange(files);
                }
                // Duyệt qua tất cả các thư mục con và tìm kiếm các tệp tin .doc
                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    FindDocs(directory, typeFile, searchSubfolders);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                AppendTextBox("Không thể truy cập thư mục " + path + ": " + ex.Message, 3);
            }
            catch (Exception ex)
            {
                AppendTextBox("Lỗi: " + ex.Message, 1);
            }
        }
        #endregion


    }
}
