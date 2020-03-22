using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MookEditor.Utils
{
    class Util
    {
        static List<string> extensionName = new List<string> { "me", "txt", "yml", "mod", "info" };

        /// <summary>
        /// 扩展名 .me
        /// </summary>
        public static string extension = ".me";

        //加载可用文件扩展
        public static void LoadConfigFile()
        {
            if (System.IO.File.Exists(System.Environment.CurrentDirectory + "\\Config\\file.txt"))
            {
                FileStream fs = new FileStream(System.Environment.CurrentDirectory + "\\Config\\file.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                string[] tempStrs = sr.ReadToEnd().Trim().Split(',');
                extensionName = tempStrs.ToList();
            }
            else
            {
                if (!System.IO.Directory.Exists(System.Environment.CurrentDirectory + "\\Config"))
                {
                    System.IO.Directory.CreateDirectory(System.Environment.CurrentDirectory + "\\Config");
                }
                FileStream fs = new FileStream(System.Environment.CurrentDirectory + "\\Config\\file.txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                string tempStr = "";
                foreach (string str in extensionName)
                {
                    tempStr += str + ",";
                }
                sw.Write(tempStr);
                sw.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// 扩展名是否存在
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static bool IsExtensionName(string extension)
        {
            return extensionName.Contains(extension.Replace(".", ""));
        }

        /// <summary>
        /// 判断括号互相嵌套的正确性
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsBracketMatch(string str)
        {
            Stack<char> stack = new Stack<char>();
            for (int i = 0; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case '{':
                    case '[':
                    case '(':
                        stack.Push(str[i]);
                        break;
                    case '}':
                        if (stack.Count > 0 && stack.Pop() == '{')
                            break;
                        else
                            return false;
                    case ']':
                        if (stack.Count > 0 && stack.Pop() == '[')
                            break;
                        else
                            return false;
                    case ')':
                        if (stack.Count > 0 && stack.Pop() == '(')
                            break;
                        else
                            return false;
                    default:
                        continue;
                }
            }
            return stack.Count == 0;
        }

        /// <summary>
        /// 16进制整数转颜色
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        /// <summary>
        /// 复制或剪切文件到剪切板
        /// </summary>
        /// <param name="files">文件路径数组</param>
        /// <param name="cut">true:剪切；false:复制</param>
        public static void CopyToClipboard(string[] files, bool cut)
        {
            if (files == null) return;
            IDataObject data = new DataObject(DataFormats.FileDrop, files);
            MemoryStream memo = new MemoryStream(4);
            byte[] bytes = new byte[] { (byte)(cut ? 2 : 5), 0, 0, 0 };
            memo.Write(bytes, 0, bytes.Length);
            data.SetData("PreferredDropEffect", memo);
            Clipboard.SetDataObject(data, false);
        }
    }
}
