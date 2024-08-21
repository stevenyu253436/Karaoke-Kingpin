using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karaoke_Kingpin
{
    public static class PinyinToZhuyinConverter
    {
        private static readonly Dictionary<string, string> PinyinToZhuyinMap = new Dictionary<string, string>
        {
            { "a", "ㄚ" }, { "o", "ㄛ" }, { "e", "ㄜ" }, { "ai", "ㄞ" }, { "ei", "ㄟ" },
            { "ao", "ㄠ" }, { "ou", "ㄡ" }, { "an", "ㄢ" }, { "en", "ㄣ" }, { "ang", "ㄤ" },
            { "eng", "ㄥ" }, { "yi", "ㄧ" }, { "ya", "ㄧㄚ" }, { "yo", "ㄧㄛ" }, { "ye", "ㄧㄝ" },
            { "yai", "ㄧㄞ" }, { "yao", "ㄧㄠ" }, { "you", "ㄧㄡ" }, { "yan", "ㄧㄢ" }, { "yin", "ㄧㄣ" },
            { "yang", "ㄧㄤ" }, { "ying", "ㄧㄥ" }, { "wu", "ㄨ" }, { "wa", "ㄨㄚ" }, { "wo", "ㄨㄛ" },
            { "wai", "ㄨㄞ" }, { "wei", "ㄨㄟ" }, { "wan", "ㄨㄢ" }, { "wen", "ㄨㄣ" }, { "wang", "ㄨㄤ" },
            { "weng", "ㄨㄥ" }, { "yu", "ㄩ" }, { "yue", "ㄩㄝ" }, { "yuan", "ㄩㄢ" }, { "yun", "ㄩㄣ" },
            { "ba", "ㄅㄚ" }, { "bo", "ㄅㄛ" }, { "bai", "ㄅㄞ" }, { "bei", "ㄅㄟ" }, { "bao", "ㄅㄠ" },
            { "ban", "ㄅㄢ" }, { "ben", "ㄅㄣ" }, { "bang", "ㄅㄤ" }, { "beng", "ㄅㄥ" }, { "bi", "ㄅㄧ" },
            { "bie", "ㄅㄧㄝ" }, { "biao", "ㄅㄧㄠ" }, { "bian", "ㄅㄧㄢ" }, { "bin", "ㄅㄧㄣ" }, { "bing", "ㄅㄧㄥ" },
            { "bu", "ㄅㄨ" }, { "pa", "ㄆㄚ" }, { "po", "ㄆㄛ" }, { "pai", "ㄆㄞ" }, { "pei", "ㄆㄟ" },
            { "pao", "ㄆㄠ" }, { "pou", "ㄆㄡ" }, { "pan", "ㄆㄢ" }, { "pen", "ㄆㄣ" }, { "pang", "ㄆㄤ" },
            { "peng", "ㄆㄥ" }, { "pi", "ㄆㄧ" }, { "pie", "ㄆㄧㄝ" }, { "piao", "ㄆㄧㄠ" }, { "pian", "ㄆㄧㄢ" },
            { "pin", "ㄆㄧㄣ" }, { "ping", "ㄆㄧㄥ" }, { "pu", "ㄆㄨ" }, { "ma", "ㄇㄚ" }, { "mo", "ㄇㄛ" },
            { "me", "ㄇㄜ" }, { "mai", "ㄇㄞ" }, { "mei", "ㄇㄟ" }, { "mao", "ㄇㄠ" }, { "mou", "ㄇㄡ" },
            { "man", "ㄇㄢ" }, { "men", "ㄇㄣ" }, { "mang", "ㄇㄤ" }, { "meng", "ㄇㄥ" }, { "mi", "ㄇㄧ" },
            { "mie", "ㄇㄧㄝ" }, { "miao", "ㄇㄧㄠ" }, { "mian", "ㄇㄧㄢ" }, { "min", "ㄇㄧㄣ" }, { "ming", "ㄇㄧㄥ" },
            { "mu", "ㄇㄨ" }, { "fa", "ㄈㄚ" }, { "fo", "ㄈㄛ" }, { "fei", "ㄈㄟ" }, { "fou", "ㄈㄡ" },
            { "fan", "ㄈㄢ" }, { "fen", "ㄈㄣ" }, { "fang", "ㄈㄤ" }, { "feng", "ㄈㄥ" }, { "fu", "ㄈㄨ" },
            { "da", "ㄉㄚ" }, { "de", "ㄉㄜ" }, { "dai", "ㄉㄞ" }, { "dei", "ㄉㄟ" }, { "dao", "ㄉㄠ" },
            { "dou", "ㄉㄡ" }, { "dan", "ㄉㄢ" }, { "den", "ㄉㄣ" }, { "dang", "ㄉㄤ" }, { "deng", "ㄉㄥ" },
            { "di", "ㄉㄧ" }, { "die", "ㄉㄧㄝ" }, { "diao", "ㄉㄧㄠ" }, { "diu", "ㄉㄧㄡ" }, { "dian", "ㄉㄧㄢ" },
            { "ding", "ㄉㄧㄥ" }, { "du", "ㄉㄨ" }, { "duo", "ㄉㄨㄛ" }, { "dui", "ㄉㄨㄟ" }, { "duan", "ㄉㄨㄢ" },
            { "dun", "ㄉㄨㄣ" }, { "dong", "ㄉㄨㄥ" }, { "ta", "ㄊㄚ" }, { "te", "ㄊㄜ" }, { "tai", "ㄊㄞ" },
            { "tao", "ㄊㄠ" }, { "tou", "ㄊㄡ" }, { "tan", "ㄊㄢ" }, { "tang", "ㄊㄤ" }, { "teng", "ㄊㄥ" },
            { "ti", "ㄊㄧ" }, { "tie", "ㄊㄧㄝ" }, { "tiao", "ㄊㄧㄠ" }, { "tian", "ㄊㄧㄢ" }, { "ting", "ㄊㄧㄥ" },
            { "tu", "ㄊㄨ" }, { "tuo", "ㄊㄨㄛ" }, { "tui", "ㄊㄨㄟ" }, { "tuan", "ㄊㄨㄢ" }, { "tun", "ㄊㄨㄣ" },
            { "tong", "ㄊㄨㄥ" }, { "na", "ㄋㄚ" }, { "ne", "ㄋㄜ" }, { "nai", "ㄋㄞ" }, { "nei", "ㄋㄟ" },
            { "nao", "ㄋㄠ" }, { "nou", "ㄋㄡ" }, { "nan", "ㄋㄢ" }, { "nen", "ㄋㄣ" }, { "nang", "ㄋㄤ" },
            { "neng", "ㄋㄥ" }, { "ni", "ㄋㄧ" }, { "nie", "ㄋㄧㄝ" }, { "niao", "ㄋㄧㄠ" }, { "niu", "ㄋㄧㄡ" },
            { "nian", "ㄋㄧㄢ" }, { "nin", "ㄋㄧㄣ" }, { "niang", "ㄋㄧㄤ" }, { "ning", "ㄋㄧㄥ" }, { "nu", "ㄋㄨ" },
            { "nuo", "ㄋㄨㄛ" }, { "nuan", "ㄋㄨㄢ" }, { "nun", "ㄋㄨㄣ" }, { "nong", "ㄋㄨㄥ" }, { "nv", "ㄋㄩ" },
            { "nue", "ㄋㄩㄝ" },
            { "la", "ㄌㄚ" }, { "le", "ㄌㄜ" }, { "lai", "ㄌㄞ" }, { "lei", "ㄌㄟ" },
            { "lao", "ㄌㄠ" }, { "lou", "ㄌㄡ" }, { "lan", "ㄌㄢ" }, { "lang", "ㄌㄤ" }, { "leng", "ㄌㄥ" },
            { "li", "ㄌㄧ" }, { "lia", "ㄌㄧㄚ" }, { "lie", "ㄌㄧㄝ" }, { "liao", "ㄌㄧㄠ" },
            { "liu", "ㄌㄧㄡ" }, { "lian", "ㄌㄧㄢ" }, { "lin", "ㄌㄧㄣ" }, { "liang", "ㄌㄧㄤ" }, { "ling", "ㄌㄧㄥ" },
            { "lu", "ㄌㄨ" }, { "luo", "ㄌㄨㄛ" }, { "luan", "ㄌㄨㄢ" }, { "lun", "ㄌㄨㄣ" }, { "long", "ㄌㄨㄥ" },
            { "lv", "ㄌㄩ" }, { "lve", "ㄌㄩㄝ" },
            { "ga", "ㄍㄚ" }, { "ge", "ㄍㄜ" }, { "gai", "ㄍㄞ" }, { "gei", "ㄍㄟ" },
            { "gao", "ㄍㄠ" }, { "gou", "ㄍㄡ" }, { "gan", "ㄍㄢ" }, { "gen", "ㄍㄣ" }, { "gang", "ㄍㄤ" },
            { "geng", "ㄍㄥ" }, { "gu", "ㄍㄨ" }, { "gua", "ㄍㄨㄚ" }, { "guo", "ㄍㄨㄛ" },
            { "guai", "ㄍㄨㄞ" }, { "gui", "ㄍㄨㄟ" }, { "guan", "ㄍㄨㄢ" }, { "gun", "ㄍㄨㄣ" }, { "guang", "ㄍㄨㄤ" },
            { "gong", "ㄍㄨㄥ" },
            { "ka", "ㄎㄚ" }, { "ke", "ㄎㄜ" }, { "kai", "ㄎㄞ" }, { "kei", "ㄎㄟ" },
            { "kao", "ㄎㄠ" }, { "kou", "ㄎㄡ" }, { "kan", "ㄎㄢ" }, { "ken", "ㄎㄣ" }, { "kang", "ㄎㄤ" },
            { "keng", "ㄎㄥ" }, { "ku", "ㄎㄨ" }, { "kua", "ㄎㄨㄚ" }, { "kuo", "ㄎㄨㄛ" },
            { "kuai", "ㄎㄨㄞ" }, { "kui", "ㄎㄨㄟ" }, { "kuan", "ㄎㄨㄢ" }, { "kun", "ㄎㄨㄣ" }, { "kuang", "ㄎㄨㄤ" },
            { "kong", "ㄎㄨㄥ" },
            { "ha", "ㄏㄚ" }, { "he", "ㄏㄜ" }, { "hai", "ㄏㄞ" }, { "hei", "ㄏㄟ" },
            { "hao", "ㄏㄠ" }, { "hou", "ㄏㄡ" }, { "han", "ㄏㄢ" }, { "hen", "ㄏㄣ" }, { "hang", "ㄏㄤ" },
            { "heng", "ㄏㄥ" }, { "hu", "ㄏㄨ" }, { "hua", "ㄏㄨㄚ" }, { "huo", "ㄏㄨㄛ" },
            { "huai", "ㄏㄨㄞ" }, { "hui", "ㄏㄨㄟ" }, { "huan", "ㄏㄨㄢ" }, { "hun", "ㄏㄨㄣ" }, { "huang", "ㄏㄨㄤ" },
            { "hong", "ㄏㄨㄥ" },
            { "ji", "ㄐ一" }, { "jia", "ㄐㄧㄚ" }, { "jie", "ㄐㄧㄝ" }, { "jiao", "ㄐㄧㄠ" },
            { "jiu", "ㄐ一ㄡ" }, { "jian", "ㄐ一ㄢ" }, { "jin", "ㄐ一ㄣ" }, { "jiang", "ㄐ一ㄤ" }, { "jing", "ㄐ一ㄥ" },
            { "ju", "ㄐㄩ" }, { "jue", "ㄐㄩㄝ" }, { "juan", "ㄐㄩㄢ" }, { "jun", "ㄐㄩㄣ" },
            { "jiong", "ㄐㄩㄥ" },
            { "qi", "ㄑ一" }, { "qia", "ㄑㄧㄚ" }, { "qie", "ㄑㄧㄝ" }, { "qiao", "ㄑㄧㄠ" },
            { "qiu", "ㄑ一ㄡ" }, { "qian", "ㄑ一ㄢ" }, { "qin", "ㄑ一ㄣ" }, { "qiang", "ㄑ一ㄤ" }, { "qing", "ㄑ一ㄥ" },
            { "qu", "ㄑㄩ" }, { "que", "ㄑㄩㄝ" }, { "quan", "ㄑㄩㄢ" }, { "qun", "ㄑㄩㄣ" },
            { "qiong", "ㄑㄩㄥ" },
            { "xi", "ㄒ一" }, { "xia", "ㄒㄧㄚ" }, { "xie", "ㄒㄧㄝ" }, { "xiao", "ㄒㄧㄠ" },
            { "xiu", "ㄒ一ㄡ" }, { "xian", "ㄒ一ㄢ" }, { "xin", "ㄒ一ㄣ" }, { "xiang", "ㄒ一ㄤ" }, { "xing", "ㄒ一ㄥ" },
            { "xu", "ㄒㄩ" }, { "xue", "ㄒㄩㄝ" }, { "xuan", "ㄒㄩㄢ" }, { "xun", "ㄒㄩㄣ" },
            { "xiong", "ㄒㄩㄥ" },
            { "zha", "ㄓㄚ" }, { "zhe", "ㄓㄜ" }, { "zhai", "ㄓㄞ" }, { "zhei", "ㄓㄟ" },
            { "zhao", "ㄓㄠ" }, { "zhou", "ㄓㄡ" }, { "zhan", "ㄓㄢ" }, { "zhen", "ㄓㄣ" }, { "zhang", "ㄓㄤ" },
            { "zheng", "ㄓㄥ" }, { "zhu", "ㄓㄨ" }, { "zhua", "ㄓㄨㄚ" }, { "zhuo", "ㄓㄨㄛ" },
            { "zhuai", "ㄓㄨㄞ" }, { "zhui", "ㄓㄨㄟ" }, { "zhuan", "ㄓㄨㄢ" }, { "zhun", "ㄓㄨㄣ" }, { "zhuang", "ㄓㄨㄤ" },
            { "zhong", "ㄓㄨㄥ" },
            { "cha", "ㄔㄚ" }, { "che", "ㄔㄜ" }, { "chai", "ㄔㄞ" }, { "chao", "ㄔㄠ" },
            { "chou", "ㄔㄡ" }, { "chan", "ㄔㄢ" }, { "chen", "ㄔㄣ" }, { "chang", "ㄔㄤ" }, { "cheng", "ㄔㄥ" },
            { "chu", "ㄔㄨ" }, { "chua", "ㄔㄨㄚ" }, { "chuo", "ㄔㄨㄛ" }, { "chuai", "ㄔㄨㄞ" }, { "chui", "ㄔㄨㄟ" },
            { "chuan", "ㄔㄨㄢ" }, { "chun", "ㄔㄨㄣ" }, { "chuang", "ㄔㄨㄤ" }, { "chong", "ㄔㄨㄥ" },
            { "sha", "ㄕㄚ" }, { "she", "ㄕㄜ" }, { "shai", "ㄕㄞ" }, { "shao", "ㄕㄠ" }, { "shou", "ㄕㄡ" },
            { "shan", "ㄕㄢ" }, { "shen", "ㄕㄣ" }, { "shang", "ㄕㄤ" }, { "sheng", "ㄕㄥ" }, { "shu", "ㄕㄨ" },
            { "shua", "ㄕㄨㄚ" }, { "shuo", "ㄕㄨㄛ" }, { "shuai", "ㄕㄨㄞ" }, { "shui", "ㄕㄨㄟ" }, { "shuan", "ㄕㄨㄢ" },
            { "shun", "ㄕㄨㄣ" }, { "shuang", "ㄕㄨㄤ" }, { "shong", "ㄕㄨㄥ" },
            { "re", "ㄖㄜ" }, { "rai", "ㄖㄞ" }, { "rao", "ㄖㄠ" }, { "rou", "ㄖㄡ" }, { "ran", "ㄖㄢ" },
            { "ren", "ㄖㄣ" }, { "rang", "ㄖㄤ" }, { "reng", "ㄖㄥ" }, { "ru", "ㄖㄨ" }, { "ruo", "ㄖㄨㄛ" },
            { "rui", "ㄖㄨㄟ" }, { "ruan", "ㄖㄨㄢ" }, { "run", "ㄖㄨㄣ" }, { "rong", "ㄖㄨㄥ" },
            { "za", "ㄗㄚ" }, { "ze", "ㄗㄜ" }, { "zai", "ㄗㄞ" }, { "zei", "ㄗㄟ" }, { "zao", "ㄗㄠ" },
            { "zou", "ㄗㄡ" }, { "zan", "ㄗㄢ" }, { "zen", "ㄗㄣ" }, { "zang", "ㄗㄤ" }, { "zeng", "ㄗㄥ" },
            { "zu", "ㄗㄨ" }, { "zuo", "ㄗㄨㄛ" }, { "zuan", "ㄗㄨㄢ" }, { "zun", "ㄗㄨㄣ" }, { "zong", "ㄗㄨㄥ" },
            { "zui", "ㄗㄨㄟ" },
            { "ca", "ㄘㄚ" }, { "ce", "ㄘㄜ" }, { "cai", "ㄘㄞ" }, { "cao", "ㄘㄠ" }, { "cou", "ㄘㄡ" },
            { "can", "ㄘㄢ" }, { "cen", "ㄘㄣ" }, { "cang", "ㄘㄤ" }, { "ceng", "ㄘㄥ" }, { "cu", "ㄘㄨ" },
            { "cuo", "ㄘㄨㄛ" }, { "cuan", "ㄘㄨㄢ" }, { "cun", "ㄘㄨㄣ" }, { "cong", "ㄘㄨㄥ" },
            { "cui", "ㄘㄨㄟ" },
            { "sa", "ㄙㄚ" }, { "se", "ㄙㄜ" }, { "sai", "ㄙㄞ" }, { "sao", "ㄙㄠ" }, { "sou", "ㄙㄡ" },
            { "san", "ㄙㄢ" }, { "sen", "ㄙㄣ" }, { "sang", "ㄙㄤ" }, { "seng", "ㄙㄥ" }, { "su", "ㄙㄨ" },
            { "suo", "ㄙㄨㄛ" }, { "suan", "ㄙㄨㄢ" }, { "sun", "ㄙㄨㄣ" }, { "song", "ㄙㄨㄥ" },
            { "sui", "ㄙㄨㄟ" },
            { "zi", "ㄗ" }, { "ci", "ㄘ" }, { "si", "ㄙ" }, { "zhi", "ㄓ" }, { "chi", "ㄔ" }, { "shi", "ㄕ" },
            { "ri", "ㄖ" },
            // Add more mappings as needed
        };

        public static string ConvertPinyinToZhuyin(string pinyin)
        {
            StringBuilder zhuyin = new StringBuilder();
            string[] pinyinWords = pinyin.Split(' ');

            foreach (string pinyinWord in pinyinWords)
            {
                string key = pinyinWord; // Use the whole pinyin word
                if (PinyinToZhuyinMap.TryGetValue(key, out string zhuyinSymbol))
                {
                    zhuyin.Append(zhuyinSymbol + " ");
                }
                else
                {
                    zhuyin.Append(pinyinWord + " "); // Fallback to original pinyin if not found in the map
                }
            }

            return zhuyin.ToString().Trim(); // Trim the trailing space
        }

        public static bool IsZhuyin(char ch)
        {
            // Define the range of Unicode characters for Zhuyin
            return ch >= 'ㄅ' && ch <= 'ㄩ';
        }
    }
}
