using MahApps.Metro.Controls;
using MookEditor.Utils;
using MookEditor.Utils.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using YamlDotNet.Serialization;

namespace MookEditor.MOD
{
    class CodeForm
    {
        MainWindow mainWindow;
        EditorManager editorManager;
        public List<string> countryEvent = new List<string>();
        public string codeContent;
        //
        int codeStartLine = -1;
        int codeEndLine = -1;
        public CodeForm(string codePath, MainWindow mainWindow, EditorManager editorManager)
        {
            this.mainWindow = mainWindow;
            this.editorManager = editorManager;

            FileStream fs = new FileStream(codePath, FileMode.Open);
            StreamReader sr = new StreamReader(fs, new UTF8Encoding(false));
            codeContent = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            int pe = ParsingCode(codePath);

            if (pe == -1)
            {
                //OperationInterface.SetStatusText(LocalizedLangExtension.GetString("CodeFormatSuccessful"));
            }
            if (pe > -1)
            {
                OperationInterface.SetStatusText(string.Format("{0}: {1} {2}, {3}", LocalizedLangExtension.GetString("Error"), pe, LocalizedLangExtension.GetString("Line"), LocalizedLangExtension.GetString("MissingBraces")), OperationInterface.StatusText.Warning);
            }else if (pe == -2)
            {
                OperationInterface.SetStatusText(string.Format("{0}: {1}", LocalizedLangExtension.GetString("Error"), LocalizedLangExtension.GetString("MissingBraces")), OperationInterface.StatusText.Warning);
            }
        }

        /// <summary>
        /// 解析代码并格式化，返回出错行，返回-1则为无错
        /// </summary>
        int ParsingCode(string codeFilePath)
        {
            Regex regex = new Regex(@"\r*\n* *#+.*\r*\n");
            string noCommentStr = regex.Replace(codeContent, "\n");

            string[] lineStr = Regex.Split(noCommentStr, @"\n");
            for (int i = 0; i < lineStr.Length; i++)
            {
                lineStr[i] = new Regex(@"\s*#+.*\r*\n*").Replace(lineStr[i], "\n");
                Console.WriteLine(lineStr[i]);
            }
            //删除空字符串数组
            lineStr = lineStr.Where(s => !string.IsNullOrEmpty(s.Trim())).ToArray();

            Stack<char> stack = new Stack<char>();

            for (int i = 0; i < lineStr.Length; i++)
            {
                //去掉尾部空行
                lineStr[i] = lineStr[i].TrimEnd();
                lineStr[i] = Regex.Replace(lineStr[i], @" *= *{ *\n*", " = {\n");
                lineStr[i] = Regex.Replace(lineStr[i], @" *\n*} *\n*", "\n}\n");

                for (int stackIndex = 0; stackIndex < lineStr[i].Length; stackIndex++)
                {
                    switch (lineStr[i][stackIndex])
                    {
                        case '{':
                            stack.Push(lineStr[i][stackIndex]);
                            //找到括号且无开始行，设置开始行
                            if (codeStartLine == -1)
                            {
                                codeStartLine = i;
                            }
                            break;
                        case '}':
                            if (stack.Count > 0 && stack.Pop() == '{')
                            {
                                //完全匹配,且有开始行
                                if (stack.Count == 0 && codeStartLine > -1)
                                {
                                    codeEndLine = i;
                                    string ec = "";
                                    while (codeStartLine <= codeEndLine)
                                    {
                                        ec += lineStr[codeStartLine] + "\n";
                                        codeStartLine++;
                                    }
                                    countryEvent.Add(ec);
                                    codeStartLine = -1;
                                    codeEndLine = -1;

                                }
                                break;
                            }
                            else
                                return i;
                        default:
                            continue;
                    }

                }

                //当前行没有括号且正在匹配括号为0
                if (!Regex.IsMatch(lineStr[i], @"{+") && !Regex.IsMatch(lineStr[i], @"}+"))
                {
                    //该行没有括号且正在匹配括号为0
                    if (stack.Count == 0)
                    {
                        countryEvent.Add(lineStr[i] + "\n");
                    }
                }

            }

            if (stack.Count == 0)
            {
                editorManager.Clear();
                string str = "";
                foreach (var item in countryEvent)
                {
                    str += item + "\n";
                }
                editorManager.LoadDataString(str, codeFilePath);

                //解析为YAML
                OperationInterface.SetStatusText("解析代码中，请稍后……");
                var yaml = TxtResolveToYaml(codeFilePath);
                //editorManager.LoadDataString(yaml, codeFilePath);
                OperationInterface.SetStatusText("解码为YAML成功！");

                mainWindow.modClassTree.Items.Clear();
                new YAMLForm(Path.GetDirectoryName(codeFilePath) + "\\" + Path.GetFileNameWithoutExtension(codeFilePath) + Util.extension, mainWindow, editorManager);

                return -1;
            }
            else
            {
                //缺少右括号
                return -2;
            }
        }

        /// <summary>
        /// TXT解析为YAML
        /// </summary>
        /// <param name="codeFilePath"></param>
        public string TxtResolveToYaml(string codeFilePath)
        {
            List<ModClass> modClassParent = new List<ModClass>();
            ModClass modClass = new ModClass();
            
            LoopPars(countryEvent, ref modClass, 0);
            modClassParent = modClass.child;

            FileStream fs = new FileStream(Path.GetDirectoryName(codeFilePath) + "\\" + Path.GetFileNameWithoutExtension(codeFilePath) + ".me", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));
            //
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(modClassParent);
            sw.Write(yaml);
            sw.Close();
            fs.Close();

            return yaml;
        }

        /// <summary>
        /// 循环解析
        /// </summary>
        /// <param name="codeParent"></param>
        /// <param name="modClassParent"></param>
        /// <param name="level"></param>
        public void LoopPars(List<string> codeParent, ref ModClass modClassParent, int level)
        {
            int startLine = -1;
            int endLine = -1;
            Stack<char> stack = new Stack<char>();
            for (int i = 0; i < codeParent.Count; i++)
            {
                List<string> lineClassChild = new List<string>();
                
                for (int stackIndex = 0; stackIndex < codeParent[i].Length; stackIndex++)
                {
                    switch (codeParent[i][stackIndex])
                    {
                        case '{':
                            stack.Push(codeParent[i][stackIndex]);
                            //找到括号且无开始行，设置开始行
                            if (startLine == -1)
                            {
                                startLine = i;
                            }
                            break;
                        case '}':
                            if (stack.Count > 0 && stack.Pop() == '{')
                            {
                                //完全匹配,且有开始行
                                if (stack.Count == 0 && startLine > -1)
                                {
                                    ModClass modClass = new ModClass();
                                    string[] strs = codeParent[startLine].Split('=');
                                    modClass.level = level;
                                    modClass.id = i;
                                    modClass.type = Type.function;
                                    modClass.name = strs[0].Trim();
                                    int lv = modClass.level + 1;
                                    modClassParent.child.Add(modClass);

                                    endLine = i;
                                    while (startLine <= endLine)
                                    {
                                        lineClassChild.Add(codeParent[startLine]);
                                        startLine++;
                                    }
                                    startLine = -1;
                                    endLine = -1;
                                    //删除空字符串数组
                                    lineClassChild = lineClassChild.Where(s => !string.IsNullOrEmpty(s.Trim())).ToList();
                                    //
                                    List<string> tempLine = new List<string>();
                                    for (int ii = 0; ii < lineClassChild.Count; ii++)
                                    {
                                        string[] ss = lineClassChild[ii].Split('\n');
                                        foreach (var item in ss)
                                        {
                                            tempLine.Add(item);
                                        }
                                    }
                                    tempLine = tempLine.Where(s => !string.IsNullOrEmpty(s.Trim())).ToList();
                                    tempLine.RemoveAt(0);
                                    tempLine.RemoveAt(tempLine.Count - 1);

                                    LoopPars(tempLine, ref modClass, lv);
                                }
                                break;
                            }
                            else
                                return;
                        default:
                            continue;
                    }
                }

                //当前行没有括号
                if (!Regex.IsMatch(codeParent[i], @" *= *{+ *\n*") && !Regex.IsMatch(codeParent[i], @" *\n*}+ *\n*"))
                {
                    //该行没有括号且正在匹配括号为0
                    if (stack.Count == 0)
                    {
                        ModClass modClass = new ModClass();
                        string[] strs = new string[2];
                        if (Regex.IsMatch(codeParent[i], @">+"))
                        {
                            strs = codeParent[i].Split('>');
                            modClass.op = ">";
                        }
                        else if (Regex.IsMatch(codeParent[i], @"<+"))
                        {
                            strs = codeParent[i].Split('<');
                            modClass.op = "<";
                        }
                        else if (Regex.IsMatch(codeParent[i], @"=+"))
                        {
                            strs = codeParent[i].Split('=');
                            modClass.op = "=";
                        }
                        else
                        {
                            strs = codeParent[i].Split(' ');
                            strs = strs.Where(s => !string.IsNullOrEmpty(s.Trim())).ToArray();
                        }

                        if (string.IsNullOrWhiteSpace(modClass.op))
                        {
                            //空
                            foreach (var item in strs)
                            {
                                modClass.name += item + " ";
                            }
                            modClass.level = level;
                            modClass.id = i;
                            modClass.type = Type.variable;
                            modClass.name = modClass.name.Trim();
                            modClassParent.child.Add(modClass);
                        }
                        else
                        {
                            modClass.level = level;
                            modClass.id = i;
                            modClass.type = Type.variable;
                            modClass.name = strs[0].Trim();
                            modClass.val = strs[1].Trim();
                            modClassParent.child.Add(modClass);

                        }

                    }

                }
            }


            
        }
    }
}
