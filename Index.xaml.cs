using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;  // 確保添加了這個命名空間
using System.Data.SqlClient;  // 对于SQL Server
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Controls;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;  // 確保這行代碼存在
using System.ComponentModel;
using Pinyin4net;
using Pinyin4net.Format;
using Microsoft.International.Converters.PinYinConverter;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using System.Reflection;
using ExcelDataReader;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace Karaoke_Kingpin
{
    /// <summary>
    /// Index.xaml 的互動邏輯
    /// </summary>
    /// 

    public partial class Index : Window
    {
        //private bool isInitialLoad = true; // 默认为true，表示处于初始化加载阶段
        private bool isFormFullyLoaded = false;
        private TcpClient client;
        private NetworkStream stream;
        private ObservableCollection<SongData> _songs = new ObservableCollection<SongData>();
        private List<string> pcs = new List<string>();
        public ObservableCollection<string> MarqueeItems { get; set; }
        private DispatcherTimer announcementTimer;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public Index()
        {
            InitializeComponent();
            //AllocConsole(); // 调用 AllocConsole 分配控制台

            //InitializeConnection();
            LoadSongsFromDatabase();

            //// 显式设置默认选中项
            //themeSelector.SelectedIndex = 0; // 或其他逻辑来选择默认项

            //// 将事件处理器附加到事件上
            //this.themeSelector.SelectionChanged += themeSelector_SelectionChanged;
            this.DataContext = new MainViewModel();

            LoadRoomsFromTextFile();
            PopulateRoomNumbersComboBox();
            PopulateHostComboBox();

            // 读取文件内容并绑定到 ListBox
            LoadMarqueeItems();

            // Register the event handler for the Add Data button
            AddDataButton.Click += btnAddData_Click;

            // 初始化并启动定时器
            InitializeAnnouncementTimer();
        }

        private void InitializeAnnouncementTimer()
        {
            announcementTimer = new DispatcherTimer();
            announcementTimer.Interval = TimeSpan.FromSeconds(30); // 每30秒触发一次
            announcementTimer.Tick += async (sender, args) => await SendAnnouncement();
            announcementTimer.Start();
        }

        private void btnAddData_Click(object sender, EventArgs e)
        {
            string target = RoomNumbersComboBox.SelectedItem.ToString();
            string color = ((ComboBoxItem)ColorComboBox.SelectedItem).Content.ToString();
            string content = ContentTextBox.Text;

            // Regular expression to check if content starts with {target}({color})-
            string pattern = $@"^{Regex.Escape(target)}\({Regex.Escape(color)}\)-";
            if (!Regex.IsMatch(content, pattern))
            {
                // If content does not start with the required format, add the prefix
                content = $"{target}({color})-{content}";
            }

            // Check if the new entry already exists
            if (!MarqueeItems.Contains(content))
            {
                // Add to marquee data list
                MarqueeItems.Add(content);

                // Append newEntry to marquee_items.txt
                string filePath = @"txt\marquee_items.txt";
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.WriteLine(content);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("該項目已經存在。");
            }

            // Clear input box
            //ContentTextBox.Text = string.Empty;
        }

        private void btnSaveData_Click(object sender, EventArgs e)
        {
            string target = RoomNumbersComboBox.SelectedItem.ToString();
            string color = ((ComboBoxItem)ColorComboBox.SelectedItem).Content.ToString();
            string content = ContentTextBox.Text;

            // Regular expression to check if content starts with {target}({color})-
            string prefixPattern = $@"^{Regex.Escape(target)}\({Regex.Escape(color)}\)-";
            string entryPattern = $@"^{Regex.Escape(target)}\(.+\)-(.+)";

            // Check if content starts with the required format
            if (!Regex.IsMatch(content, prefixPattern))
            {
                content = $"{target}({color})-{content}";
            }

            // Find the existing entry by checking the suffix part of the content
            string existingEntry = MarqueeItems.FirstOrDefault(item =>
                Regex.IsMatch(item, entryPattern) && Regex.Match(item, entryPattern).Groups[1].Value == content);

            if (existingEntry != null)
            {
                int index = MarqueeItems.IndexOf(existingEntry);
                MarqueeItems[index] = content;
            }
            else
            {
                MarqueeItems.Add(content);
            }

            // Save the updated list to marquee_items.txt
            string filePath = @"txt\marquee_items.txt";
            File.WriteAllLines(filePath, MarqueeItems);

            // Clear input box
            //ContentTextBox.Text = string.Empty;
        }


        private void btnDeleteData_Click(object sender, EventArgs e)
        {
            if (MarqueeListBox.SelectedItem != null)
            {
                string selectedItem = MarqueeListBox.SelectedItem.ToString();
                MarqueeItems.Remove(selectedItem);

                // Update marquee_items.txt
                string filePath = @"txt\marquee_items.txt";
                var lines = File.ReadAllLines(filePath).ToList();
                lines.Remove(selectedItem);
                File.WriteAllLines(filePath, lines);
            }
            else
            {
                System.Windows.MessageBox.Show("請先選擇一個項目來刪除。");
            }
        }

        private void LoadRoomsFromTextFile()
        {
            var lines = File.ReadAllLines(@"txt\room.txt");

            foreach (var line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length != 2) continue;

                string[] pcArray = parts[1].Split(',');

                foreach (var pc in pcArray)
                {
                    if (pc.Trim().StartsWith("pc"))
                    {
                        pcs.Add(pc.Trim());
                    }
                }
            }
        }

        private void CheckBox_Checked_Unchecked(object sender, RoutedEventArgs e)
        {
            // 处理复选框的选中和取消选中逻辑，例如更新UI或发送数据等
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked == true)
            {
                // 例如，发送开启的命令
            }
            else
            {
                // 例如，发送关闭的命令
            }
        }

        private void PopulateRoomNumbersComboBox()
        {
            // Create a new list and add the "全部" option at the beginning
            var allPcs = new List<string> { "全部" };
            allPcs.AddRange(pcs);

            // Create a new list for display
            var displayPcs = new List<string>(allPcs);

            // Modify displayPcs to "0" + the last three characters of allPcs
            for (int i = 1; i < displayPcs.Count; i++)
            {
                if (displayPcs[i].Length >= 3)
                {
                    displayPcs[i] = "0" + displayPcs[i].Substring(displayPcs[i].Length - 3);
                }
                else
                {
                    displayPcs[i] = "0" + displayPcs[i];
                }
            }

            RoomNumbersComboBox.ItemsSource = displayPcs;
        }

        private void RemoveItemFromMarqueeFile(string item, string filePath)
        {
            var lines = File.ReadAllLines(filePath).ToList();
            bool itemFound = false;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(item))
                {
                    lines.RemoveAt(i); // 删除整行
                    itemFound = true;
                    break; // 假设每个项目只出现一次，找到即删除
                }
            }

            if (itemFound)
            {
                File.WriteAllLines(filePath, lines);
            }
        }

        private void AddItemToMarqueeFile(string item, string filePath)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(item);
            }
        }

        private void PopulateHostComboBox()
        {
            hostComboBox.ItemsSource = pcs;
        }

        private void HostComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (hostComboBox.SelectedItem != null)
            {
                string selectedHost = hostComboBox.SelectedItem.ToString();
                DisplayLogFile(selectedHost, "songerror.txt"); // 默认显示点歌错误日志
            }
        }

        private void LoadMarqueeItems()
        {
            MarqueeItems = new ObservableCollection<string>();
            string filePath = "txt/marquee_items.txt";

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    MarqueeItems.Add(line);
                }
            }

            MarqueeListBox.ItemsSource = MarqueeItems;
        }

        private void MarqueeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MarqueeListBox.SelectedItem != null)
            {
                RoomNumbersComboBox.SelectedIndex = 0; // 选择 "全部"

                string selectedItem = MarqueeListBox.SelectedItem.ToString();

                // 假设颜色信息在项目字符串的括号内，如 "全部(紅色) - 內容..."
                int startIndex = selectedItem.IndexOf('(');
                int endIndex = selectedItem.IndexOf(')');
                if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
                {
                    string color = selectedItem.Substring(startIndex + 1, endIndex - startIndex - 1);

                    // 根据颜色字符串设置ColorComboBox的选项
                    foreach (ComboBoxItem item in ColorComboBox.Items)
                    {
                        if (item.Content.ToString() == color)
                        {
                            ColorComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        private void LoadMarqueeItem_Click(object sender, RoutedEventArgs e)
        {
            if (MarqueeListBox.SelectedItem != null)
            {
                string selectedText = MarqueeListBox.SelectedItem.ToString();
                ContentTextBox.Text = selectedText;
            }
            else
            {
                System.Windows.MessageBox.Show("請先選擇一個項目。");
            }
        }

        private void DisplayLogFile(string hostName, string logFileName)
        {
            string logFilePath = $@"\\{hostName}\superstar\{logFileName}";

            try
            {
                string logContent = File.ReadAllText(logFilePath);
                systemExceptionLogTextBox.Text = logContent;
                systemExceptionLogTextBox.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                systemExceptionLogTextBox.Text = $"無法讀取日志文件: {ex.Message}";
            }
        }

        private void InitializeConnection()
        {
            try
            {
                client = new TcpClient("192.168.88.52", 1000); // 使用VOD服务器的IP和端口
                stream = client.GetStream();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        private async void SendAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            await SendAnnouncement();
        }

        private int currentAnnouncementIndex = 0; // 记录当前公告索引

        private async Task SendAnnouncement()
        {
            if (MarqueeItems == null || MarqueeItems.Count == 0)
            {
                Console.WriteLine("沒有可發送的公告");
                return;
            }

            string announcement = MarqueeItems[currentAnnouncementIndex]; // 获取当前公告内容
            currentAnnouncementIndex = (currentAnnouncementIndex + 1) % MarqueeItems.Count; // 更新索引，循环发送

            Console.WriteLine($"Triggered Send Announcement: {announcement}"); // 确认事件触发

            // 使用正则表达式匹配主机信息
            string pattern = @"^(全部|\d{4})\((白色|紅色|綠色|黑色|藍色)\)-";
            Match match = Regex.Match(announcement, pattern);

            if (!match.Success)
            {
                System.Windows.MessageBox.Show("公告内容格式不正確！");
                return;
            }

            string targetRoom = match.Groups[1].Value;
            List<string> targetHostNames = new List<string>();

            // 填充targetHostNames列表
            if (targetRoom == "全部")
            {
                targetHostNames = pcs.ToList();
            }
            else
            {
                string targetRoomSuffix = targetRoom.Substring(targetRoom.Length - 3); // 获取目标房间号的后3位
                targetHostNames = pcs.Where(pc => pc.EndsWith(targetRoomSuffix)).ToList();
            }

            // 打印targetHostNames列表
            Console.WriteLine("Target Host Names:");
            foreach (var hostName in targetHostNames)
            {
                Console.WriteLine(hostName);
            }

            await Task.Run(async () =>
            {
                foreach (var hostName in targetHostNames)
                {
                    await SendAnnouncementAsync(hostName, announcement); // 在后台线程执行异步操作
                }
            }).ConfigureAwait(false); // 确保不捕获上下文
        }

        private const int MaxSendAttempts = 3; // Maximum number of send attempts
        private const int DelayBetweenSendAttempts = 1000; // Delay between send attempts in milliseconds

        private async Task SendAnnouncementAsync(string hostname, string message)
        {
            int attemptCount = 0;
            bool messageSent = false;
            Exception lastException = null;

            while (attemptCount < MaxSendAttempts && !messageSent)
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        await client.ConnectAsync(hostname, 1000).ConfigureAwait(false);
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] data = Encoding.UTF8.GetBytes(message + "\r\n");
                            await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                            // Ensure MessageBox is shown on the UI thread
                            //await Application.Current.Dispatcher.InvokeAsync(() => System.Windows.MessageBox.Show("Message sent successfully."));
                            messageSent = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogToFile($"Error on send attempt {attemptCount + 1}: {ex.Message}");
                    lastException = ex;
                }

                attemptCount++;
                if (!messageSent && attemptCount < MaxSendAttempts)
                {
                    await Task.Delay(DelayBetweenSendAttempts).ConfigureAwait(false);
                }
            }

            if (!messageSent)
            {
                LogToFile($"Failed to send the announcement after multiple attempts. Last error: {lastException?.Message}");
            }
        }

        private void LogToFile(string message)
        {
            string logFilePath = "log.txt"; // 设定日志文件的路径
            string logMessage = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {message}\r\n";

            // 使用 File.AppendAllText 异步写入日志信息，它会自动创建文件（如果文件不存在的话）
            File.AppendAllText(logFilePath, logMessage);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (stream != null)
            {
                stream.Close();
            }
            if (client != null)
            {
                client.Close();
            }
        }

        private void LoadSongsFromDatabase()
        {
            string connectionString = @"Data Source=KSongDatabase.db;";  // SQLite连接字符串
            string query = "SELECT * FROM SongLibrary";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(query, connection);
                try
                {
                    connection.Open();
                    SQLiteDataReader reader = command.ExecuteReader();

                    _songs = new ObservableCollection<SongData>();
                    while (reader.Read())
                    {
                        //int? dbAdjust = null; // 使用可空的整数类型

                        //// 在尝试转换之前，检查字段是否为空
                        //if (!reader.IsDBNull(reader.GetOrdinal("DB加減")) && !string.IsNullOrWhiteSpace(reader["DB加減"].ToString()))
                        //{
                        //    dbAdjust = Convert.ToInt32(reader["DB加減"]);
                        //}

                        int songNameLength = 0;

                        if (!int.TryParse(reader["歌名字數"].ToString(), out songNameLength))
                        {
                            System.Diagnostics.Debug.WriteLine($"歌名字數 conversion failed for value {reader["歌名字數"].ToString()}");
                        }

                        var song = new SongData(
                            reader["歌曲編號"].ToString(),
                            reader["歌曲名稱"].ToString(),
                            reader["歌星 A"].ToString(),
                            reader["歌星 B"].ToString(),
                            reader["路徑 1"].ToString(),
                            reader["路徑 2"].ToString(),
                            reader["歌曲檔名"].ToString(),
                            reader.IsDBNull(reader.GetOrdinal("新增日期")) ? DateTime.Now.ToString("yyyy/MM/dd") : Convert.ToDateTime(reader["新增日期"]).ToString("yyyy/MM/dd"),
                            reader["分類"].ToString(),
                            reader["歌曲注音"].ToString(),
                            reader["歌曲拼音"].ToString(),
                            reader["語別"].ToString(),
                            reader.IsDBNull(reader.GetOrdinal("點播次數")) ? 0 : Convert.ToInt32(reader["點播次數"]),
                            reader["版權01"].ToString(),
                            reader["版權02"].ToString(),
                            reader["版權03"].ToString(),
                            reader["版權04"].ToString(),
                            reader["版權05"].ToString(),
                            reader["版權06"].ToString(),
                            reader.IsDBNull(reader.GetOrdinal("狀態")) ? 0 : Convert.ToInt32(reader["狀態"]),
                            reader.IsDBNull(reader.GetOrdinal("歌名字數")) ? 0 : Convert.ToInt32(reader["歌名字數"]),
                            reader.IsDBNull(reader.GetOrdinal("人聲")) ? 0 : Convert.ToInt32(reader["人聲"]),
                            reader.IsDBNull(reader.GetOrdinal("狀態2")) ? 0 : Convert.ToInt32(reader["狀態2"]),
                            reader["情境"].ToString(),
                            reader["歌星A注音"].ToString(),
                            reader["歌星B注音"].ToString(),
                            reader["歌星A分類"].ToString(),
                            reader["歌星B分類"].ToString(),
                            reader["歌星A簡體"].ToString(),
                            reader["歌星B簡體"].ToString(),
                            reader["歌名簡體"].ToString(),
                            reader["歌星A拼音"].ToString(),
                            reader["歌星B拼音"].ToString()
                        );
                        _songs.Add(song);
                    }
                    SongsDataGrid.ItemsSource = _songs;  // 确保这里没有错误，你的 XAML 定义了 SongsDataGrid 控件
                    reader.Close();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("數據庫操作失敗: " + ex.Message);
                }
            }
        }

        private void SongsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SongsDataGrid.SelectedItem is SongData selectedSong)
            {
                // 更新 TextBox 控件
                // 這裡假設您的 TextBox 控件已經命名，如 txtSongNumber、txtSongName 等
                txtSongNumber.Text = selectedSong.SongNumber;
                txtSongName.Text = selectedSong.Song;
                txtArtistA.Text = selectedSong.ArtistA;
                txtArtistB.Text = selectedSong.ArtistB;
                txtPath1.Text = selectedSong.SongFilePathHost1;
                txtPath2.Text = selectedSong.SongFilePathHost2;
                txtSongFileName.Text = selectedSong.SongFileName;
                txtAddedDate.Text = selectedSong.AddedTime;
                txtCategory.Text = selectedSong.Category;
                txtSongPhonetic.Text = selectedSong.PhoneticNotation;
                txtSongPinyin.Text = selectedSong.PinyinNotation;
                txtArtistAPhonetic.Text = selectedSong.ArtistAPhonetic;
                txtArtistBPhonetic.Text = selectedSong.ArtistBPhonetic;
                // 繼續為其他字段更新值
                txtSongNameLength.Text = selectedSong.SongNameLength.ToString(); // Convert int to string
                txtPlays.Text = selectedSong.Plays.ToString();
                txtCopyright01.Text = selectedSong.Copyright01;
                txtCopyright02.Text = selectedSong.Copyright02;
                txtCopyright03.Text = selectedSong.Copyright03;
                txtCopyright04.Text = selectedSong.Copyright04;
                txtCopyright05.Text = selectedSong.Copyright05;
                txtCopyright06.Text = selectedSong.Copyright06;
                // 方法1: 使用ComboBoxItem的Name或Tag属性匹配
                foreach (ComboBoxItem item in artistACategoryComboBox.Items)
                {
                    if (item.Content.ToString() == selectedSong.ArtistACategory)
                    {
                        artistACategoryComboBox.SelectedItem = item;
                        break;
                    }
                }
                foreach (ComboBoxItem item in artistBCategoryComboBox.Items)
                {
                    if (item.Content.ToString() == selectedSong.ArtistBCategory)
                    {
                        artistBCategoryComboBox.SelectedItem = item;
                        break;
                    }
                }
                foreach (ComboBoxItem item in languageTypeComboBox.Items)
                {
                    if (item.Content.ToString() == selectedSong.LanguageType)
                    {
                        languageTypeComboBox.SelectedItem = item;
                        break;
                    }
                }
                foreach (ComboBoxItem item in statusComboBox.Items)
                {
                    if (int.Parse(item.Content.ToString()) == selectedSong.Status)
                    {
                        statusComboBox.SelectedItem = item;
                        break;
                    }
                }
                foreach (ComboBoxItem item in status2ComboBox.Items)
                {
                    if (int.Parse(item.Content.ToString()) == selectedSong.Status2)
                    {
                        status2ComboBox.SelectedItem = item;
                        break;
                    }
                }
                foreach (ComboBoxItem item in vocalComboBox.Items)
                {
                    if (int.Parse(item.Content.ToString()) == selectedSong.Vocal)
                    {
                        vocalComboBox.SelectedItem = item;
                        break;
                    }
                }
                foreach (ComboBoxItem item in situationsComboBox.Items)
                {
                    if (item.Content.ToString() == selectedSong.Situation)
                    {
                        situationsComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private int CalculateSongNameLength(string songName)
        {
            int length = 0;
            bool isEnglishWord = false;

            foreach (char ch in songName)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (isEnglishWord)
                    {
                        length++;
                        isEnglishWord = false;
                    }
                }
                else if (char.IsLetter(ch))
                {
                    if (ch >= 0x4E00 && ch <= 0x9FA5)
                    {
                        // Chinese character
                        length++;
                        isEnglishWord = false;
                    }
                    else
                    {
                        // English letter
                        isEnglishWord = true;
                    }
                }
            }

            // If the last character was part of an English word, count it as one word
            if (isEnglishWord)
            {
                length++;
            }

            return length;
        }

        private void SaveSongButton_Click(object sender, RoutedEventArgs e)
        {
            string songName = txtSongName.Text;

            var song = new SongData(
                txtSongNumber.Text,
                txtSongName.Text,
                txtArtistA.Text,
                txtArtistB.Text,
                txtPath1.Text,
                txtPath2.Text,
                txtSongFileName.Text,
                txtAddedDate.Text,
                txtCategory.Text,
                txtSongPhonetic.Text,
                txtSongPinyin.Text,
                ((ComboBoxItem)languageTypeComboBox.SelectedItem).Content.ToString(),
                int.Parse(txtPlays.Text),
                txtCopyright01.Text,
                txtCopyright02.Text,
                txtCopyright03.Text,
                txtCopyright04.Text,
                txtCopyright05.Text,
                txtCopyright06.Text,
                int.Parse((statusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()), // Replace with actual status if available
                int.Parse(txtSongNameLength.Text), // Use calculated song name length
                int.Parse((vocalComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()), // Replace with actual vocal if available
                int.Parse((status2ComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()), // Replace with actual status2 if available
                ((ComboBoxItem)situationsComboBox.SelectedItem).Content.ToString(),
                txtArtistAPhonetic.Text,
                txtArtistBPhonetic.Text,
                (artistACategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(), // Replace with actual artistACategory if available
                (artistBCategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(), // Replace with actual artistBCategory if available
                txtArtistASimplified.Text,
                txtArtistBSimplified.Text,
                txtSongSimplified.Text,
                txtArtistAPinyin.Text,
                txtArtistBPinyin.Text
            );

            _SaveSongToDatabase(song);
        }

        private void DeleteSongButton_Click(object sender, RoutedEventArgs e)
        {
            if (SongsDataGrid.SelectedItem is SongData selectedSong)
            {
                DeleteSongFromDatabase(selectedSong.SongNumber);
                _songs.Remove(selectedSong);  // Remove the song from the ObservableCollection
            }
        }

        private void DeleteSongFromDatabase(string songNumber)
        {
            string connectionString = @"Data Source=KSongDatabase.db;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                string query = "DELETE FROM SongLibrary WHERE 歌曲編號 = @SongNumber";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SongNumber", songNumber);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SaveSongToDatabase(SongData song)
        {
            string connectionString = @"Data Source=KSongDatabase.db;";
            string query = "INSERT INTO SongLibrary (歌曲編號, 歌曲名稱, [歌星 A], [歌星 B], [路徑 1], [路徑 2], 歌曲檔名, 新增日期, 分類, 歌曲注音, 歌曲拼音, 語別, 點播次數, 版權01, 版權02, 版權03, 版權04, 版權05, 版權06, 狀態, 歌名字數, 人聲, 狀態2, 情境, 歌星A注音, 歌星B注音, 歌星A分類, 歌星B分類, 歌星A簡體, 歌星B簡體, 歌名簡體) " +
                           "VALUES (@SongNumber, @Song, @ArtistA, @ArtistB, @SongFilePathHost1, @SongFilePathHost2, @SongFileName, @AddedTime, @Category, @PhoneticNotation, @PinyinNotation, @LanguageType, @Plays, @Copyright01, @Copyright02, @Copyright03, @Copyright04, @Copyright05, @Copyright06, @Status, @SongNameLength, @Vocal, @Status2, @Situation, @ArtistAPhonetic, @ArtistBPhonetic, @ArtistACategory, @ArtistBCategory, @ArtistASimplified, @ArtistBSimplified, @SongSimplified)";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@SongNumber", song.SongNumber);
                command.Parameters.AddWithValue("@Song", song.Song);
                command.Parameters.AddWithValue("@ArtistA", song.ArtistA);
                command.Parameters.AddWithValue("@ArtistB", song.ArtistB);
                command.Parameters.AddWithValue("@SongFilePathHost1", song.SongFilePathHost1);
                command.Parameters.AddWithValue("@SongFilePathHost2", song.SongFilePathHost2);
                command.Parameters.AddWithValue("@SongFileName", song.SongFileName);
                // Convert the date format from "yyyy/MM/dd" to "yyyy-MM-dd"
                string addedTimeFormatted = song.AddedTime;
                DateTime parsedDate;
                if (DateTime.TryParseExact(addedTimeFormatted, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    addedTimeFormatted = parsedDate.ToString("yyyy-MM-dd");
                }
                else if (DateTime.TryParseExact(addedTimeFormatted, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    addedTimeFormatted = parsedDate.ToString("yyyy-MM-dd");
                }
                command.Parameters.AddWithValue("@AddedTime", addedTimeFormatted);
                command.Parameters.AddWithValue("@Category", song.Category);
                command.Parameters.AddWithValue("@PhoneticNotation", song.PhoneticNotation);
                command.Parameters.AddWithValue("@PinyinNotation", song.PinyinNotation);
                command.Parameters.AddWithValue("@LanguageType", song.LanguageType);
                command.Parameters.AddWithValue("@Plays", song.Plays);
                command.Parameters.AddWithValue("@Copyright01", song.Copyright01);
                command.Parameters.AddWithValue("@Copyright02", song.Copyright02);
                command.Parameters.AddWithValue("@Copyright03", song.Copyright03);
                command.Parameters.AddWithValue("@Copyright04", song.Copyright04);
                command.Parameters.AddWithValue("@Copyright05", song.Copyright05);
                command.Parameters.AddWithValue("@Copyright06", song.Copyright06);
                command.Parameters.AddWithValue("@Status", song.Status);
                command.Parameters.AddWithValue("@SongNameLength", song.SongNameLength);
                command.Parameters.AddWithValue("@Vocal", song.Vocal);
                command.Parameters.AddWithValue("@Status2", song.Status2);
                command.Parameters.AddWithValue("@Situation", song.Situation);
                command.Parameters.AddWithValue("@ArtistAPhonetic", song.ArtistAPhonetic);
                command.Parameters.AddWithValue("@ArtistBPhonetic", song.ArtistBPhonetic);
                command.Parameters.AddWithValue("@ArtistACategory", song.ArtistACategory);
                command.Parameters.AddWithValue("@ArtistBCategory", song.ArtistBCategory);
                command.Parameters.AddWithValue("@ArtistASimplified", song.ArtistASimplified);
                command.Parameters.AddWithValue("@ArtistBSimplified", song.ArtistBSimplified);
                command.Parameters.AddWithValue("@SongSimplified", song.SongSimplified);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    System.Windows.MessageBox.Show("Song saved successfully to the database.");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Failed to save song to the database: " + ex.Message);
                }
            }

            // Add the song to the ObservableCollection if it's not already in it
            var existingSong = _songs.FirstOrDefault(s => s.SongNumber == song.SongNumber);
            if (existingSong == null)
            {
                _songs.Add(song);
            }
            else
            {
                // Update the existing song in the ObservableCollection
                int index = _songs.IndexOf(existingSong);
                _songs[index] = song;
            }
        }

        private void _SaveSongToDatabase(SongData song)
        {
            string connectionString = @"Data Source=KSongDatabase.db;";
            string query = "INSERT INTO SongLibrary (歌曲編號, 歌曲名稱, [歌星 A], [歌星 B], [路徑 1], [路徑 2], 歌曲檔名, 新增日期, 分類, 歌曲注音, 歌曲拼音, 語別, 點播次數, 版權01, 版權02, 版權03, 版權04, 版權05, 版權06, 狀態, 歌名字數, 人聲, 狀態2, 情境, 歌星A注音, 歌星B注音, 歌星A分類, 歌星B分類, 歌星A簡體, 歌星B簡體, 歌名簡體) " +
                           "VALUES (@SongNumber, @Song, @ArtistA, @ArtistB, @SongFilePathHost1, @SongFilePathHost2, @SongFileName, @AddedTime, @Category, @PhoneticNotation, @PinyinNotation, @LanguageType, @Plays, @Copyright01, @Copyright02, @Copyright03, @Copyright04, @Copyright05, @Copyright06, @Status, @SongNameLength, @Vocal, @Status2, @Situation, @ArtistAPhonetic, @ArtistBPhonetic, @ArtistACategory, @ArtistBCategory, @ArtistASimplified, @ArtistBSimplified, @SongSimplified)";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@SongNumber", song.SongNumber);
                command.Parameters.AddWithValue("@Song", song.Song);
                command.Parameters.AddWithValue("@ArtistA", song.ArtistA);
                command.Parameters.AddWithValue("@ArtistB", song.ArtistB);
                command.Parameters.AddWithValue("@SongFilePathHost1", song.SongFilePathHost1);
                command.Parameters.AddWithValue("@SongFilePathHost2", song.SongFilePathHost2);
                command.Parameters.AddWithValue("@SongFileName", song.SongFileName);
                // Convert the date format from "yyyy/MM/dd" to "yyyy-MM-dd"
                string addedTimeFormatted = song.AddedTime;
                DateTime parsedDate;
                if (DateTime.TryParseExact(addedTimeFormatted, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    addedTimeFormatted = parsedDate.ToString("yyyy-MM-dd");
                }
                command.Parameters.AddWithValue("@AddedTime", addedTimeFormatted);
                command.Parameters.AddWithValue("@Category", song.Category);
                command.Parameters.AddWithValue("@PhoneticNotation", song.PhoneticNotation);
                command.Parameters.AddWithValue("@PinyinNotation", song.PinyinNotation);
                command.Parameters.AddWithValue("@LanguageType", song.LanguageType);
                command.Parameters.AddWithValue("@Plays", song.Plays);
                command.Parameters.AddWithValue("@Copyright01", song.Copyright01);
                command.Parameters.AddWithValue("@Copyright02", song.Copyright02);
                command.Parameters.AddWithValue("@Copyright03", song.Copyright03);
                command.Parameters.AddWithValue("@Copyright04", song.Copyright04);
                command.Parameters.AddWithValue("@Copyright05", song.Copyright05);
                command.Parameters.AddWithValue("@Copyright06", song.Copyright06);
                command.Parameters.AddWithValue("@Status", song.Status);
                command.Parameters.AddWithValue("@SongNameLength", song.SongNameLength);
                command.Parameters.AddWithValue("@Vocal", song.Vocal);
                command.Parameters.AddWithValue("@Status2", song.Status2);
                command.Parameters.AddWithValue("@Situation", song.Situation);
                command.Parameters.AddWithValue("@ArtistAPhonetic", song.ArtistAPhonetic);
                command.Parameters.AddWithValue("@ArtistBPhonetic", song.ArtistBPhonetic);
                command.Parameters.AddWithValue("@ArtistACategory", song.ArtistACategory);
                command.Parameters.AddWithValue("@ArtistBCategory", song.ArtistBCategory);
                command.Parameters.AddWithValue("@ArtistASimplified", song.ArtistASimplified);
                command.Parameters.AddWithValue("@ArtistBSimplified", song.ArtistBSimplified);
                command.Parameters.AddWithValue("@SongSimplified", song.SongSimplified);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    //System.Windows.MessageBox.Show("Song saved successfully to the database.");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Failed to save song to the database: " + ex.Message);
                }
            }

            // Add the song to the ObservableCollection if it's not already in it
            var existingSong = _songs.FirstOrDefault(s => s.SongNumber == song.SongNumber);
            if (existingSong == null)
            {
                _songs.Add(song);
            }
            else
            {
                // Update the existing song in the ObservableCollection
                int index = _songs.IndexOf(existingSong);
                _songs[index] = song;
            }
        }

        private void LoveSongButton_Click(object sender, RoutedEventArgs e)
        {
            AddCategory("A1");
        }

        private void TalentShowButton_Click(object sender, RoutedEventArgs e)
        {
            AddCategory("B1");
        }

        private void MedleyButton_Click(object sender, RoutedEventArgs e)
        {
            AddCategory("C1");
        }

        private void NinetiesButton_Click(object sender, RoutedEventArgs e)
        {
            AddCategory("D1");
        }

        private void MemoriesButton_Click(object sender, RoutedEventArgs e)
        {
            AddCategory("E1");
        }

        private void MainlandButton_Click(object sender, RoutedEventArgs e)
        {
            AddCategory("F1");
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            txtCategory.Text = string.Empty;
        }

        private void AddCategory(string categoryCode)
        {
            if (string.IsNullOrEmpty(txtCategory.Text))
            {
                txtCategory.Text = $",{categoryCode},";
            }
            else
            {
                txtCategory.Text += $"{categoryCode},";
            }
        }

        private void UpdatePhoneticButton_Click(object sender, RoutedEventArgs e)
        {
            string songName = txtSongName.Text;
            if (!string.IsNullOrEmpty(songName) && ChineseChar.IsValidChar(songName[0]))
            {
                string pinyin = ConvertToPinyin(songName);
                string simplifiedPinyin = string.Concat(pinyin.Split(' ').Select(p => p[0])); // Get first letters only
                string zhuyin = PinyinToZhuyinConverter.ConvertPinyinToZhuyin(pinyin);
                string simplifiedZhuyin = string.Concat(zhuyin.Split(' ').Select(z => PinyinToZhuyinConverter.IsZhuyin(z[0]) ? z[0].ToString() : z.ToUpper()));

                txtSongPinyin.Text = simplifiedPinyin.ToUpper(); // Convert to uppercase
                txtSongPhonetic.Text = simplifiedZhuyin;
            }

            // Process artist fields
            ProcessArtistFields();
        }

        private void ProcessArtistFields()
        {
            // Process artistA
            string artistA = txtArtistA.Text;
            if (!string.IsNullOrEmpty(artistA))
            {
                string pinyin = ConvertToPinyin(artistA);
                string simplifiedPinyin = string.Concat(pinyin.Split(' ').Select(p => p[0]));
                string zhuyin = PinyinToZhuyinConverter.ConvertPinyinToZhuyin(pinyin);
                string simplifiedZhuyin = string.Concat(zhuyin.Split(' ').Select(z => PinyinToZhuyinConverter.IsZhuyin(z[0]) ? z[0].ToString() : z.ToUpper()));
                
                txtArtistAPinyin.Text = simplifiedPinyin;
                txtArtistAPhonetic.Text = simplifiedZhuyin;
            }
            else
            {
                txtArtistAPhonetic.Text = artistA;
            }

            // Process artistB
            string artistB = txtArtistB.Text;
            if (!string.IsNullOrEmpty(artistB))
            {
                string pinyin = ConvertToPinyin(artistB);
                string simplifiedPinyin = string.Concat(pinyin.Split(' ').Select(p => p[0]));
                string zhuyin = PinyinToZhuyinConverter.ConvertPinyinToZhuyin(pinyin);
                string simplifiedZhuyin = string.Concat(zhuyin.Split(' ').Select(z => PinyinToZhuyinConverter.IsZhuyin(z[0]) ? z[0].ToString() : z.ToUpper()));
                txtArtistBPhonetic.Text = simplifiedZhuyin;
            }
            else
            {
                txtArtistBPhonetic.Text = artistB;
            }
        }

        private string ConvertToPinyin(string chineseText)
        {
            StringBuilder pinyinBuilder = new StringBuilder();

            foreach (char ch in chineseText)
            {
                if (ChineseChar.IsValidChar(ch))
                {
                    ChineseChar chineseChar = new ChineseChar(ch);
                    string pinyin = chineseChar.Pinyins[0].ToLower(); // Get the first Pinyin and convert to lower case
                    pinyin = pinyin.Substring(0, pinyin.Length - 1); // Remove the tone number at the end
                    pinyinBuilder.Append(pinyin + " ");
                }
            }

            return pinyinBuilder.ToString().Trim();
        }

        private void TxtArtistA_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtArtistASimplified.Text = ConvertToSimplifiedChinese(txtArtistA.Text);
        }

        private void TxtArtistB_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtArtistBSimplified.Text = ConvertToSimplifiedChinese(txtArtistB.Text);
        }

        private void TxtSongName_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtSongSimplified.Text = ConvertToSimplifiedChinese(txtSongName.Text);
        }

        private string ConvertToSimplifiedChinese(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            string simplifiedText = string.Empty;

            foreach (char c in text)
            {
                if (ChineseChar.IsValidChar(c))
                {
                    simplifiedText += ChineseConverter.Convert(c.ToString(), ChineseConversionDirection.TraditionalToSimplified);
                }
                else
                {
                    simplifiedText += c;
                }
            }

            return simplifiedText;
        }

        private void ExportSongsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV file (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = "SongsExport.csv"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // 定义属性名称到中文名称的映射字典
                        Dictionary<string, string> propertyToChineseMap = new Dictionary<string, string>
                        {
                            { "SongNumber", "歌曲編號" },
                            { "Category", "分類" },
                            { "Song", "歌曲名稱" },
                            { "Plays", "點播次數" },
                            { "ArtistA", "歌星 A" },
                            { "ArtistB", "歌星 B" },
                            { "AddedTime", "新增日期" },
                            { "SongFilePathHost1", "路徑 1" },
                            { "SongFilePathHost2", "路徑 2" },
                            { "SongFileName", "歌曲檔名" },
                            { "PhoneticNotation", "歌曲注音" },
                            { "PinyinNotation", "歌曲拼音" },
                            { "LanguageType", "語別" },
                            { "Copyright01", "版權01" },
                            { "Copyright02", "版權02" },
                            { "Copyright03", "版權03" },
                            { "Copyright04", "版權04" },
                            { "Copyright05", "版權05" },
                            { "Copyright06", "版權06" },
                            { "Status", "狀態" },
                            { "SongNameLength", "歌名字數" },
                            { "Vocal", "人聲" },
                            { "Status2", "狀態2" },
                            { "Situation", "情境" },
                            { "ArtistAPhonetic", "歌星A注音" },
                            { "ArtistBPhonetic", "歌星B注音" },
                            { "ArtistACategory", "歌星A分類" },
                            { "ArtistBCategory", "歌星B分類" },
                            { "ArtistASimplified", "歌星A簡體" },
                            { "ArtistBSimplified", "歌星B簡體" },
                            { "SongSimplified", "歌名簡體" },
                            { "ArtistAPinyin", "歌星A拼音" },
                            { "ArtistBPinyin", "歌星B拼音" }
                        };

                        // 获取所有属性名
                        PropertyInfo[] properties = typeof(SongData).GetProperties();

                        // 写入标题行（映射到中文名称）
                        sw.WriteLine(string.Join(",", properties.Select(p => propertyToChineseMap.ContainsKey(p.Name) ? propertyToChineseMap[p.Name] : p.Name)));

                        // 遍历歌曲集合并写入文件
                        foreach (var song in _songs)
                        {
                            // 获取所有属性值
                            var values = properties.Select(p => p.GetValue(song, null)).Select(v => v?.ToString() ?? string.Empty);

                            // 写入一行
                            sw.WriteLine(string.Join(",", values));
                        }
                    }
                    System.Windows.MessageBox.Show("歌曲數據庫導出成功！");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("導出過程中發生錯誤: " + ex.Message);
            }
        }

        private void ImportSongsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx;*.xlsm",
                Title = "Select an Excel File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                List<SongData> songs = ImportSongsFromExcel(filePath);
                SaveSongsToDatabase(songs);
                System.Windows.MessageBox.Show("歌曲數據庫匯入成功！");
            }
        }

        private List<SongData> ImportSongsFromExcel(string filePath)
        {
            var songs = new List<SongData>();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var table = result.Tables[0];

                    for (int i = 1; i < table.Rows.Count; i++) // Skip header row
                    {
                        var row = table.Rows[i];
                        var song = new SongData
                        (
                            row[0].ToString(),
                            row[1].ToString(),
                            row[2].ToString(),
                            row[3].ToString(),
                            row[4].ToString(),
                            row[5].ToString(),
                            row[6].ToString(),
                            row[7].ToString(),
                            row[8].ToString(),
                            row[9].ToString(),
                            row[10].ToString(),
                            row[11].ToString(),
                            int.TryParse(row[12].ToString(), out int plays) ? plays : 0,
                            row[13].ToString(),
                            row[14].ToString(),
                            row[15].ToString(),
                            row[16].ToString(),
                            row[17].ToString(),
                            row[18].ToString(),
                            int.TryParse(row[19].ToString(), out int status) ? status : 0,
                            int.TryParse(row[20].ToString(), out int songNameLength) ? songNameLength : 0,
                            int.TryParse(row[21].ToString(), out int vocal) ? vocal : 0,
                            int.TryParse(row[22].ToString(), out int status2) ? status2 : 0,
                            row[23].ToString(),
                            row[24].ToString(),
                            row[25].ToString(),
                            row[26].ToString(),
                            row[27].ToString(),
                            row[28].ToString(),
                            row[29].ToString(),
                            row[30].ToString(),
                            row[31].ToString(),
                            row[32].ToString()
                        );

                        songs.Add(song);
                    }
                }
            }

            return songs;
        }

        private void SaveSongsToDatabase(List<SongData> songs)
        {
            // Implement this method to save the list of songs to your database
            foreach (var song in songs)
            {
                // Save each song to the database
                _SaveSongToDatabase(song);
            }
        }

        private void WelcomeMessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 获取文本框的当前内容
            var currentText = WelcomeMessageTextBox.Text;

            // 指定要写入的文件路径
            var filePath = "WelcomeMessage.txt";

            // 将内容写入文件
            File.WriteAllText(filePath, currentText);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Now that the Window is loaded, set the flag to true
            isFormFullyLoaded = true;

            // You can now safely access controls like txtNewSongLimit and txtHotSongLimit here if needed
        }

        private void IntegerUpDown_ValueChanged(object sender, TextChangedEventArgs e)
        {
            // Ensure the form components are fully initialized before processing any changes.
            if (!isFormFullyLoaded)
                return;

            // Ensure both controls are initialized.
            if (txtNewSongLimit != null && txtHotSongLimit != null)
            {
                if (int.TryParse(txtNewSongLimit.Text, out int newSongLimit) && int.TryParse(txtHotSongLimit.Text, out int hotSongLimit))
                {
                    // Safely access the value since it's known to be non-null here.
                    // Since we're reacting to changes on both controls, we should handle file saving here.
                    SaveLimitsToFile(newSongLimit, hotSongLimit);
                }
                else
                {
                    // Handle invalid input, e.g., show a message or reset the value to a default
                    System.Windows.MessageBox.Show("請輸入有效的整數。");
                }
            }
        }

        private void SaveLimitsToFile(int newSongLimit, int hotSongLimit)
        {
            string filePath = "SongLimitsSettings.txt"; // Update with the path where you want to save the file

            try
            {
                using (StreamWriter file = new StreamWriter(filePath, false)) // 'false' to overwrite the file if it exists
                {
                    file.WriteLine("NewSongLimit: " + newSongLimit);
                    file.WriteLine("HotSongLimit: " + hotSongLimit);
                }
            }
            catch (Exception ex)
            {
                // Handle any errors here, possibly logging them or informing the user
                Console.WriteLine("An error occurred while writing to the file: " + ex.Message);
            }
        }

        private void SearchTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 如果日期查詢被選中，你可能想要更改 TextBox 的行為
            if (searchTypeComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content.ToString() == "日期查詢")
            {
                // 設定 TextBox 為日期格式，或準備接受日期輸入
                searchTextBox.Text = String.Empty;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // 根據選中的查詢類型和 TextBox 中的值進行查詢
            if (searchTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                List<SongData> filteredSongs = new List<SongData>();
                switch (selectedItem.Content.ToString())
                {
                    case "編號查詢":
                        filteredSongs = PerformNumberSearch(searchTextBox.Text);
                        break;
                    case "歌星查詢":
                        filteredSongs = PerformArtistSearch(searchTextBox.Text);
                        break;
                    case "歌名查詢":
                        filteredSongs = PerformSongNameSearch(searchTextBox.Text);
                        break;
                    case "日期查詢":
                        filteredSongs = PerformDateSearch(searchTextBox.Text);
                        break;
                    case "語別查詢":
                        filteredSongs = PerformLanguageTypeSearch(searchTextBox.Text);
                        break;
                        // 其他查詢類型...
                }

                SongsDataGrid.ItemsSource = filteredSongs;

                // Update the result count label
                resultCountLabel.Content = $"查詢筆數: {filteredSongs.Count}筆";
            }
        }

        // Perform a search based on song number
        private List<SongData> PerformNumberSearch(string numberInput)
        {
            return _songs.Where(song => song.SongNumber.Contains(numberInput)).ToList();
        }

        private List<SongData> PerformDateSearch(string dateInput)
        {
            DateTime searchDate;
            string[] fullDateFormats = { "yyyy/MM/dd" };
            string[] yearMonthFormats = { "yyyy/MM" };

            if (DateTime.TryParseExact(dateInput, fullDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out searchDate))
            {
                return _songs.Where(song =>
                    DateTime.TryParseExact(song.AddedTime, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime addedDate) &&
                    addedDate.Date == searchDate.Date
                ).ToList();
            }
            else if (DateTime.TryParseExact(dateInput, yearMonthFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out searchDate))
            {
                return _songs.Where(song =>
                    DateTime.TryParseExact(song.AddedTime, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime addedDate) &&
                    addedDate.Year == searchDate.Year && addedDate.Month == searchDate.Month
                ).ToList();
            }
            else if (!string.IsNullOrWhiteSpace(dateInput))
            {
                return _songs.Where(song =>
                    song.AddedTime.StartsWith(dateInput) ||
                    song.AddedTime.Replace("-", "/").StartsWith(dateInput)
                ).ToList();
            }
            else
            {
                return _songs.ToList();
            }
        }

        private List<SongData> PerformArtistSearch(string artistInput)
        {
            return _songs.Where(song =>
                song.ArtistA.IndexOf(artistInput, StringComparison.OrdinalIgnoreCase) >= 0 ||
                song.ArtistB.IndexOf(artistInput, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();
        }

        private List<SongData> PerformSongNameSearch(string songNameInput)
        {
            return _songs.Where(song => song.Song.IndexOf(songNameInput, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        private List<SongData> PerformLanguageTypeSearch(string languageTypeInput)
        {
            return _songs.Where(song => song.LanguageType.IndexOf(languageTypeInput, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            // 获取触发事件的按钮
            Button button = sender as Button;
            // 假设我们使用按钮的 Tag 属性来引用相关联的 TextBox
            // 例如, button.Tag = "valueTextBox";
            TextBox textBox = this.FindName(button.Tag.ToString()) as TextBox;
            if (textBox != null)
            {
                // 将文本框的值增加
                int value = int.Parse(textBox.Text);
                value++;
                textBox.Text = value.ToString();
            }
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            // 与Increase_Click类似的逻辑，但是减少值
            TextBox textBox = this.FindName(button.Tag.ToString()) as TextBox;
            if (textBox != null)
            {
                int value = int.Parse(textBox.Text);
                value--;
                textBox.Text = value.ToString();
            }
        }

        private List<string> GetRoomConfigurations()
        {
            List<string> roomConfigurations = new List<string>();

            try
            {
                // 指定配置文件路径
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RoomConfigurations.txt");

                // 检查文件是否存在
                if (File.Exists(filePath))
                {
                    // 逐行读取文件
                    string[] lines = File.ReadAllLines(filePath);

                    // 将读取的每一行添加到列表中
                    roomConfigurations.AddRange(lines);
                }
                else
                {
                    System.Windows.MessageBox.Show("包厢配置文件不存在。", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"讀取包厢配置文件時發生錯誤: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return roomConfigurations;
        }

        private void SongSelectionError_Click(object sender, RoutedEventArgs e)
        {
            // 处理系统异常
            string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logfile.txt"); // 这是您的日志文件路径
            try
            {
                // 读取日志文件的内容
                string logContent = File.ReadAllText(logPath);
                // 将内容显示在 TextBox 中
                systemExceptionLogTextBox.Text = logContent;
                // 最后，设置 TextBox 为可见
                systemExceptionLogTextBox.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                // 如果读取文件时出错，显示错误信息
                systemExceptionLogTextBox.Text = $"讀取日誌文件時出錯: {ex.Message}";
            }
        }

        private void LoginError_Click(object sender, RoutedEventArgs e)
        {
            // 处理登录错误
            systemExceptionLogTextBox.Visibility = Visibility.Collapsed;
        }

        private void PlaybackFailure_Click(object sender, RoutedEventArgs e)
        {
            // 处理播放失败
            systemExceptionLogTextBox.Visibility = Visibility.Collapsed;
        }

        private void SystemException_Click(object sender, RoutedEventArgs e)
        {
            // 处理系统异常
            string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mainlog.txt"); // 这是您的日志文件路径
            try
            {
                // 读取日志文件的内容
                string logContent = File.ReadAllText(logPath);
                // 将内容显示在 TextBox 中
                systemExceptionLogTextBox.Text = logContent;
                // 最后，设置 TextBox 为可见
                systemExceptionLogTextBox.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                // 如果读取文件时出错，显示错误信息
                systemExceptionLogTextBox.Text = $"讀取日誌文件時出錯: {ex.Message}";
            }
        }

        //private void BrowseStandbyTrack_Click(object sender, RoutedEventArgs e)
        //{
        //    // 待機播放的逻辑代码
        //    var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
        //    // 顯示對話框
        //    System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

        //    // 如果用戶選擇了文件夾
        //    if (result == System.Windows.Forms.DialogResult.OK)
        //    {
        //        // 將選擇的文件夾路徑設定到TextBox中
        //        standbyPlaybackPathTextBox.Text = folderBrowserDialog.SelectedPath;
        //    }
        //}

        //private void BrowseCacheLocation_Click(object sender, RoutedEventArgs e)
        //{
        //    // 在这里实现打开文件（或目录）选择对话框，让用户选择快取存放位置的逻辑
        //    var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
        //    // 顯示對話框
        //    System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

        //    // 如果用戶選擇了文件夾
        //    if (result == System.Windows.Forms.DialogResult.OK)
        //    {
        //        // 將選擇的文件夾路徑設定到TextBox中
        //        cacheLocationTextBox.Text = folderBrowserDialog.SelectedPath;
        //    }
        //}


        private void SettingHint_Click(object sender, RoutedEventArgs e)
        {
            // 這裡添加按鈕點擊時的處理代碼
            System.Windows.MessageBox.Show("這裡是設定提示信息。");
        }

        //private void themeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    // 獲取當前選中的 ComboBoxItem
        //    var comboBox = sender as ComboBox;
        //    var selectedItem = comboBox.SelectedItem as ComboBoxItem;
        //    if (selectedItem != null)
        //    {
        //        try
        //        {
        //            string selectedTheme = selectedItem.Content.ToString();
        //            // 移除 "./" 如果存在
        //            selectedTheme = selectedTheme.Replace("./", "");
        //            //string imagePath = $@"{Directory.GetCurrentDirectory()}/{selectedTheme}/0.png";
        //            string imagePath = $@"C:/Users/Administrator/KSongloverNET/{selectedTheme}/0.png";
        //            themeImage.Source = new BitmapImage(new Uri(imagePath));
        //            themeImage.Visibility = Visibility.Visible;

        //            // 创建或覆盖描述文件，并写入选中的主题
        //            string descriptionFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme_description.txt");
        //            using (StreamWriter writer = new StreamWriter(descriptionFilePath))
        //            {
        //                writer.WriteLine($"Selected Theme: {selectedTheme}");
        //                writer.WriteLine($"Image Path: {imagePath}");
        //                //writer.WriteLine(selectedTheme.ToString());
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            System.Windows.MessageBox.Show($"无法加载图片: {ex.Message}");
        //        }
        //    }
        //}
    }
}
