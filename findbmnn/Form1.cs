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
using Aspose.Words;
using Aspose.Words.Rendering;
using IFilterTextReader;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

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
                richTextBoxLog.ForeColor = Color.Green;
                richTextBoxLog.AppendText("- - - - - - - - - - - - - - - - - - - " + Environment.NewLine);
                richTextBoxLog.ScrollToCaret();
            }
            richTextBoxLog.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + value + Environment.NewLine);
            richTextBoxLog.ForeColor = Color.Black;
            richTextBoxLog.AppendText("- - - - - - - - - - - - - - - - - - - " + Environment.NewLine);
            richTextBoxLog.ScrollToCaret();
        }

        // xử lý nút tìm kiếm

        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();

            buttonResult.Visible = true;
            // kiểm tra đã tồn tại file kết quả load chưa
            string currentDirectory = Directory.GetCurrentDirectory();
            string fullpath = currentDirectory + @"\save_data\resultLoadFileWord.txt";
            
            if (!File.Exists(fullpath))
            {
                MessageBox.Show("Chưa tồn tại file để load dữ liệu", "Cảnh báo", MessageBoxButtons.OK);

            }
            else
            {
                List<string> listpathfileword = File.ReadAllLines(fullpath).ToList();
                // tạo đường dẫn chứa kết quả
                string pathResultSearchString = currentDirectory + @"\save_data\resultSearchString.txt";

                foreach (string pathfileword in listpathfileword)
                {
                    // Khởi tạo đối tượng Document từ tệp Word
                                       
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            Document doc = new Document(pathfileword);
                            string content = doc.GetText().ToLower();
                            Console.WriteLine(content);
                            bool detectkey = false;
                            foreach (string searchString in listKeys)
                            {
                                if (content.Contains(searchString))
                                {
                                    AppendTextBox("Tìm thấy chuỗi '" + searchString + "' trong file " + pathfileword, 1);
                                    // thiết lập file chứa kết quả
                                    detectkey = true; 
                                } else
                                {
                                    AppendTextBox("Không tìm thấy chuỗi '" + searchString + "' trong file " + pathfileword, 1);
                                }
                            }
                            if (detectkey)
                            {
                                File.AppendAllText(pathResultSearchString, pathfileword);
                            }
                        }
                        catch
                        {
                            // Do Nothing
                        }
                    }));

                }

                await Task.WhenAll(tasks);
                AppendTextBox("Đã tìm kiếm xong!", 1);

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
                        List<string> list1 = GetAllFilesFromFolder(d.Name, "*.doc", true);
                        List<string> list2 = GetAllFilesFromFolder(d.Name, "*.docx", true);
                        listFileDoc.AddRange(list1);
                        listFileDoc.AddRange(list2);
                    }
                    catch
                    {
                        // Do Nothing
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // tạo file lưu path 
            string subPath = CreateFolder();
            string filename = "resultLoadFileWord.txt";
            SaveAllText(filename, listFileDoc);
            buttonLoadDisk.Enabled = false;

        }
        #endregion



        #region vùng backend

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
                AppendTextBox("File đã tồn tại", 1);
                AppendTextBox("Đã lưu tiếp tại đường dẫn: " + fullpath, 1);
            }
        }
        private void CheckKeyWords()
        {
            string keywords = richTextBoxKey.Text.Trim().ToLower();
            string[] listKey = GetListKey(keywords);
            AppendTextBox("Tổng số key cần phân tích: " + listKey.Length.ToString(), 1);
            foreach (string key in listKey)
            {
                AppendTextBox(key, 0);
            }
            listKeys.AddRange(listKey);
        }

        // tao thu muc chua file .txt
        private string CreateFolder()
        {
            string pathCurr = Directory.GetCurrentDirectory();
            string subPath = Path.Combine(pathCurr, "save_data");
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

        // load toàn bộ file
        public List<string> GetAllFilesFromFolder(string root, string typeFile, bool searchSubfolders)
        {
            Queue<string> folders = new Queue<string>();
            List<string> files = new List<string>();
            folders.Enqueue(root);
            while (folders.Count != 0)
            {
                string currentFolder = folders.Dequeue();
                try
                {
                    string[] filesInCurrent = Directory.GetFiles(currentFolder, typeFile, SearchOption.TopDirectoryOnly);
                    foreach (string item in filesInCurrent)
                    {
                        AppendTextBox(item, 0);
                    }
                    files.AddRange(filesInCurrent);
                }
                catch
                {
                    // Do Nothing
                }
                try
                {
                    if (searchSubfolders)
                    {
                        string[] foldersInCurrent = Directory.GetDirectories(currentFolder, "*.*", SearchOption.TopDirectoryOnly);
                        foreach (string _current in foldersInCurrent)
                        {
                            folders.Enqueue(_current);
                        }
                    }
                }
                catch
                {
                    // Do Nothing
                }
            }
            return files;
        }
        #endregion

    }
}
