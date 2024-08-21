using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karaoke_Kingpin
{
    public class SongData
    {
        public string SongNumber { get; set; } // 添加歌曲编号属性
        public string Song { get; set; }
        public string ArtistA { get; set; }
        public string ArtistB { get; set; }
        public string SongFilePathHost1 { get; set; } // 添加属性以存储歌曲文件路径
        public string SongFilePathHost2 { get; set; }
        public string SongFileName { get; set; } // Property for the song filename
        public string AddedTime { get; set; } // 新增加入时间属性
        public string Category { get; set; }
        public string PhoneticNotation { get; set; } // Add a new property for phonetic notation
        public string PinyinNotation { get; set; } // 拼音字段
        public string LanguageType { get; set; } // 歌曲的语言类别
        public int Plays { get; set; }
        public string Copyright01 { get; set; }
        public string Copyright02 { get; set; }
        public string Copyright03 { get; set; }
        public string Copyright04 { get; set; }
        public string Copyright05 { get; set; }
        public string Copyright06 { get; set; }
        public int Status { get; set; }
        //public int DbAdjust { get; set; }
        public int SongNameLength { get; set; }
        public int Vocal { get; set; }
        public int Status2 { get; set; }
        public string Situation { get; set; }
        public string ArtistAPhonetic { get; set; } // 新增歌手A注音
        public string ArtistBPhonetic { get; set; } // 新增歌手B注音
        public string ArtistACategory { get; set; }
        public string ArtistBCategory { get; set; }
        public string ArtistASimplified { get; set; } // 新增歌手A簡體中文
        public string ArtistBSimplified { get; set; } // 新增歌手B簡體中文
        public string SongSimplified { get; set; } // 新增歌曲簡體中文
        public string ArtistAPinyin { get; set; } // 新增歌手A拼音
        public string ArtistBPinyin { get; set; } // 新增歌手B拼音

        // 更新构造函数以包括歌曲文件路径参数
        // 更新构造函数以包括新参数
        public SongData(
            string songNumber, string song, string artistA, string artistB,
            string songFilePathHost1, string songFilePathHost2, string songFileName,
            string addedTime, string category, string phoneticNotation, string pinyinNotation,
            string languageType, int plays,
            string copyright01, string copyright02, string copyright03,
            string copyright04, string copyright05, string copyright06,
            int status, int songNameLength, int vocal,
            int status2, string situation,
            string artistAPhonetic, string artistBPhonetic, string artistACategory, string artistBCategory,
            string artistASimplified, string artistBSimplified, string songSimplified, string artistAPinyin, string artistBPinyin)
        {
            // ...现有属性赋值...
            SongNumber = songNumber;
            Song = song;
            ArtistA = artistA;
            ArtistB = artistB;
            SongFilePathHost1 = songFilePathHost1;
            SongFilePathHost2 = songFilePathHost2;
            SongFileName = songFileName;
            AddedTime = addedTime;
            Category = category;
            PhoneticNotation = phoneticNotation;
            PinyinNotation = pinyinNotation;
            LanguageType = languageType;
            Plays = plays;
            // 新属性赋值
            Copyright01 = copyright01;
            Copyright02 = copyright02;
            Copyright03 = copyright03;
            Copyright04 = copyright04;
            Copyright05 = copyright05;
            Copyright06 = copyright06;
            Status = status;
            SongNameLength = songNameLength;
            Vocal = vocal;
            Status2 = status2;
            Situation = situation;
            ArtistAPhonetic = artistAPhonetic;
            ArtistBPhonetic = artistBPhonetic;
            ArtistACategory = artistACategory;
            ArtistBCategory = artistBCategory;
            ArtistASimplified = artistASimplified;
            ArtistBSimplified = artistBSimplified;
            SongSimplified = songSimplified;
            ArtistAPinyin = artistAPinyin;
            ArtistBPinyin = artistBPinyin;
        }

        // Adjusted ToString
        public override string ToString()
        {
            return String.Format("Artist A: {0}, Artist B: {1}, Song: {2}", this.ArtistA, this.ArtistB, this.Song);
        }
    }
}
