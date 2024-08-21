using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Karaoke_Kingpin
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            // 檢查是否按下 Enter 鍵
            if (e.Key == Key.Enter)
            {
                AttemptLogin();
            }
        }

        private void AttemptLogin()
        {
            string userName = txtUserName.Text;
            string password = txtPassword.Text;
            string hashedPassword = HashHelper.ComputeSha256Hash(password);
            string storedHashedPassword = "05e8335be671f28dbeeca81756ee91c4c9aa3917fb6c50a326c2d58ce91bd6eb";

            if (userName == "262622584" && hashedPassword == storedHashedPassword)
            {
                Index index = new Index();
                index.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("輸入用戶名或密碼不正確");
            }
        }
    }

    public static class HashHelper
    {
        public static string ComputeSha256Hash(string rawData)
        {
            // 使用SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // 计算输入字符串的哈希值
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // 将字节数组转换为字符串
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
