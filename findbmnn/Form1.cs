using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IFilterTextReader;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

// lib thao tac file word
using Aspose.Words;
using System.Diagnostics;

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

        private void Init()
        {
            listFileDoc = new List<string>();
            listKeys = new List<string>();
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
                // lấy keyword để tìm
                CheckKeyWords();
                List<string> listpathfileword = File.ReadAllLines(fullpath).ToList();
                // tạo đường dẫn chứa kết quả
                string pathResultSearchString = currentDirectory + @"\save_data\resultSearchString.txt";
                if (File.Exists(pathResultSearchString))
                {
                    File.Delete(pathResultSearchString);
                    File.Create(pathResultSearchString);
                }
                else
                {
                    File.Create(pathResultSearchString);
                }
                // tạo folder chứa kết quả convert file txt
                string pathsavefoldertxt = CreateFolder("txt_convert");
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
                                if (content.Contains(searchString))
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
                                File.AppendAllText(pathResultSearchString, pathfileword + @"\n");
                            }
                        }
                        catch (Exception ex)
                        {
                            AppendTextBox("Không đọc được file: " + ex.ToString(), 3);
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
                    }
                    catch
                    {
                        // Do Nothing
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // tạo file lưu path 
            List<string> uniquepathfileword = listFileDoc.Distinct().ToList();


            string subPath = CreateFolder("save_data");
            string filename = "resultLoadFileWord.txt";
            SaveAllText(filename, uniquepathfileword);
            buttonLoadDisk.Enabled = false;

        }

        private void buttonResult_Click(object sender, EventArgs e)
        {
            // Lấy thư mục của User trên hệ thống
            string currentDirectory = Directory.GetCurrentDirectory();
            string pathResultSearchString = currentDirectory + @"\save_data\resultSearchString.txt";
            if (File.Exists(pathResultSearchString))
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
                // File đã tồn tại - nối thêm nội dung
                File.AppendAllLines(fullpath, contentfiles);
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
                listFileDoc.AddRange(files);

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
