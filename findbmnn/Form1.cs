using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aspose.Words;
using IFilterTextReader;
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
        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }

            richTextBoxLog.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + value + Environment.NewLine);
            richTextBoxLog.AppendText("- - - - - - - - - - - - - - - - - - - " + Environment.NewLine);
            richTextBoxLog.ScrollToCaret();
        }

        // xử lý nút tìm kiếm
        private void CheckKeyWords()
        {
            string keywords = richTextBoxKey.Text.Trim().ToLower();
            string[] listKey = GetListKey(keywords);
            AppendTextBox("Tổng số key cần phân tích: " + listKey.Length.ToString());
            foreach (string key in listKey)
            {
                AppendTextBox(key);
            }
            listKeys.AddRange(listKey);
        }
        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            /* load all file txt
            string pathCurr = Directory.GetCurrentDirectory();
            string pathTxt = Path.Combine(pathCurr, "convertTxt\\");
            List<string> listFileTxt = GetAllFilesFromFolder(pathTxt, "*.txt", true);
            */

            List<Task> tasks = new List<Task>();

            buttonResult.Visible = true;

            // bat dau chien
            CheckKeyWords();
            string subPath = CreateFolder();

            /* convert sang txt
            foreach (string fileDoc in listFileDoc)
            {
                ConvertFileToTxt(fileDoc);
            }
            */

            // tim kiem theo tu khoa

            foreach (string pathFile in listFileDoc)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        
                        string contentFile = "";


                        using (TextReader reader = new FilterReader(pathFile))
                        {
                            contentFile = reader.ReadToEnd().ToLower();
                        }
                        AppendTextBox(contentFile);
                        
                        /*
                        foreach (string key in listKeys)
                        {
                            try
                            {
                                bool check = contentFile.Contains(key); // ham nay tra ve gia tri boolean  

                                if (check)
                                {
                                    AppendTextBox(pathFile);
                                }

                            }
                            catch
                            {
                                // Do Nothing
                            }
                        }*/

                    }
                    catch
                    {
                        // Do Nothing
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }




        private async void buttonLoadDisk_Click(object sender, EventArgs e)
        {

            DriveInfo[] allDrives = DriveInfo.GetDrives();
            List<Task> tasks = new List<Task>();

            // lấy thông tin và load file
            AppendTextBox("Lấy thông tin toàn bộ file word trên máy tính");
            foreach (DriveInfo d in allDrives)
            {
                AppendTextBox("Kết quả load file tại ổ: " + d.Name);
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        List<string> list1 = GetAllFilesFromFolder(d.Name, "*.doc", true);
                        //List<string> list2 = GetAllFilesFromFolder(d.Name, "*.docx", true);
                        listFileDoc.AddRange(list1);
                        //listFileDoc.AddRange(list2);
                    }
                    catch
                    {
                        // Do Nothing
                    }
                }));
            }

            await Task.WhenAll(tasks);

            buttonSearch.Visible = true;
            buttonLoadDisk.Enabled = false;

            /*Task t = Task.WhenAll(tasks);
            try
            {
                t.Wait();
            }
            catch { }

            if (t.Status == TaskStatus.RanToCompletion)
                AppendTextBox("Load file thành công!");
            else if (t.Status == TaskStatus.Faulted)
                AppendTextBox("Load file thất bại!"); */

        }
        #endregion



        #region vùng backend

        // tao thu muc chua file .txt
        private string CreateFolder()
        {
            string pathCurr = Directory.GetCurrentDirectory();
            string subPath = Path.Combine(pathCurr, "convertTxt");
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
                        AppendTextBox(item);
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
        //phân tích file theo thư viện aspose.word
        private void ConvertFileToTxt(string path)
        {
            try
            {
                Document doc = new Document(path);
                string filename = Path.GetFileNameWithoutExtension(path);
                string pathCurr = Directory.GetCurrentDirectory();

                string subPath = Path.Combine(pathCurr, "convertTxt\\");
                string pathFile = subPath + filename + ".txt";
                doc.Save(pathFile);
                AppendTextBox("Lưu file: " + pathFile);
            }
            catch
            {
                // Do Nothing
            }

        }


        private string AnalysisWithKey(string pathFile, List<string> listKey)
        {

            string contentFile = "";


            using (TextReader reader = new FilterReader(pathFile))
            {
                contentFile = reader.ReadToEnd().ToLower();
            }
            AppendTextBox(contentFile);

            foreach (string key in listKey)
            {
                try
                {
                    bool check = contentFile.Contains(key); // ham nay tra ve gia tri boolean  

                    if (check)
                    {
                        AppendTextBox(pathFile);
                        return pathFile;
                    }

                }
                catch
                {
                    // Do Nothing
                }
            }

            return "";
        }
        #endregion

    }
}
