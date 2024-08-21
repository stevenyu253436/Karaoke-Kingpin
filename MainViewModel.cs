using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;  // 添加這行
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Karaoke_Kingpin
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _cacheImage;
        private string _filmImage;
        private string _bugImage;
        private string _userImage;
        private string _imagesImage;
        private ObservableCollection<string> _marquee1Items;
        private ObservableCollection<string> _marquee2Items;
        private string selectedRoom;
        private string selectedColor;
        private string contentText;

        public string CacheImage
        {
            get { return _cacheImage; }
            set
            {
                _cacheImage = value;
                OnPropertyChanged(nameof(CacheImage));
            }
        }

        public string FilmImage
        {
            get { return _filmImage; }
            set
            {
                _filmImage = value;
                OnPropertyChanged(nameof(FilmImage));
            }
        }

        public string BugImage
        {
            get { return _bugImage; }
            set
            {
                _bugImage = value;
                OnPropertyChanged(nameof(BugImage));
            }
        }

        public string UserImage
        {
            get { return _userImage; }
            set
            {
                _userImage = value;
                OnPropertyChanged(nameof(UserImage));
            }
        }

        public string ImagesImage
        {
            get { return _imagesImage; }
            set
            {
                _imagesImage = value;
                OnPropertyChanged(nameof(ImagesImage));
            }
        }

        public ObservableCollection<string> Marquee1Items
        {
            get { return _marquee1Items; }
            set
            {
                _marquee1Items = value;
                OnPropertyChanged(nameof(Marquee1Items));
            }
        }

        public ObservableCollection<string> Marquee2Items
        {
            get { return _marquee2Items; }
            set
            {
                _marquee2Items = value;
                OnPropertyChanged(nameof(Marquee2Items));
            }
        }
        public string SelectedRoom
        {
            get => selectedRoom;
            set
            {
                if (selectedRoom != value)
                {
                    selectedRoom = value;
                    OnPropertyChanged();
                    UpdateContentTextRoom();
                }
            }
        }

        public string SelectedColor
        {
            get => selectedColor;
            set
            {
                if (selectedColor != value)
                {
                    selectedColor = value;
                    OnPropertyChanged();
                    UpdateContentTextColor();
                }
            }
        }

        public string ContentText
        {
            get => contentText;
            set
            {
                if (contentText != value)
                {
                    contentText = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
            // Initialize with the image filename
            CacheImage = "cache.jpg";
            FilmImage = "film.jpg";
            BugImage = "bug.jpg";
            UserImage = "user.jpg";
            ImagesImage = "images.jpg";

            // Initialize Marquee Items
            Marquee1Items = new ObservableCollection<string>(File.ReadAllLines(@"txt\marquee1_items.txt"));
            Marquee2Items = new ObservableCollection<string>(File.ReadAllLines(@"txt\marquee2_items.txt"));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateContentTextColor()
        {
            if (!string.IsNullOrEmpty(ContentText) && ContentText.Length >= 5)
            {
                // 正则表达式模式，匹配"全部"或4位数字 + 颜色 + "-"
                string pattern = @"^(全部|\d{4})\((白色|紅色|綠色|黑色|藍色)\)-";
                Match match = Regex.Match(ContentText, pattern);

                if (match.Success)
                {
                    // 提取颜色部分
                    string colorName = selectedColor;

                    // 构建新的 ContentText，只在右边添加括号
                    ContentText = ContentText.Substring(0, match.Groups[1].Length + 1) +
                                  colorName +
                                  ")" +
                                  ContentText.Substring(match.Groups[1].Length + 1 + colorName.Length + 1);

                    // 通知 ContentText 属性已更新
                    OnPropertyChanged(nameof(ContentText));
                }
            }
        }

        private void UpdateContentTextRoom()
        {
            if (!string.IsNullOrEmpty(ContentText) && !string.IsNullOrEmpty(SelectedRoom))
            {
                // 正则表达式模式，匹配"全部"或4位数字 + 颜色 + "-"
                string pattern = @"^(全部|\d{4})\((白色|紅色|綠色|黑色|藍色)\)-";
                Match match = Regex.Match(ContentText, pattern);

                if (match.Success)
                {
                    // 构建新的 ContentText，将 "全部" 或房号替换为 SelectedRoom
                    ContentText = SelectedRoom + ContentText.Substring(match.Groups[1].Length);

                    // 通知 ContentText 属性已更新
                    OnPropertyChanged(nameof(ContentText));
                }
            }
        }
    }
}
