using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MookEditor.MOD;
using MookEditor.Utils;
using MookEditor.Utils.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic.Devices;
using MahApps.Metro.Controls;
using System.Text.RegularExpressions;
using MookEditor.FileTree;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using YamlDotNet.Serialization;

namespace MookEditor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public EditorManager editorManager;
        public string gameModFilePath;
        public string gameModRootPath;
        public ModConfig modConfig;
        YAMLForm modClass;
        public List<TVItems> listItems = new List<TVItems>();//定义的一个全局泛型，用于存所有TVItem对象和路径

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            //加载可用扩展名
            Util.LoadConfigFile();
            new OperationInterface(this);
            editorManager = new EditorManager(this,scintillaEditor);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show(LocalizedLangExtension.GetString("CloseProgram"), LocalizedLangExtension.GetString("Tip"), MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }

        #region 文件树相关

        //文件树被更改
        private void FileTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = (TreeViewItem)e.NewValue;   // 获取到选中的TreeViewItem对象
            TVItems tVItems = listItems.Find(delegate (TVItems tv) {
                return tv.ViewItem.Equals(item);
            });

            if (tVItems == null)
            {
                return;
            }
            
            if (!tVItems.isFolder && File.Exists(tVItems.path))
            {
                // 后缀名匹配则解析，否则直接载入
                if (Util.IsExtensionName(Path.GetExtension(tVItems.path)) && Path.GetExtension(tVItems.path) == Util.extension)
                {
                    // 加载模组文件
                    modClassTree.Items.Clear();
                    modClass = new YAMLForm(tVItems.path, this, editorManager);
                    editorManager.Clear();
                    editorManager.LoadDataFromFile(tVItems.path);

                }else if (Util.IsExtensionName(Path.GetExtension(tVItems.path)) && Path.GetExtension(tVItems.path) == ".txt")
                {
                    //解析代码并格式化为YAML，加载
                    modClassTree.Items.Clear();
                    new CodeForm(tVItems.path, this, editorManager);
                }
                else
                {
                    editorManager.LoadDataFromFile(tVItems.path);
                }

                /*try
                {
                    // 后缀名匹配则解析，否则直接载入
                    if (Util.IsExtensionName(Path.GetExtension(tVItems.path)) && Path.GetExtension(tVItems.path) == ".txt")
                    {
                        //解析代码并格式化为YAML，加载
                        modClassTree.Items.Clear();
                        new CodeForm(tVItems.path, this, editorManager);
                    }
                    else
                    {
                        editorManager.LoadDataFromFile(tVItems.path);
                    }
                }
                catch (Exception ex)
                {
                    //解析错误，直接载入
                    editorManager.LoadDataFromFile(tVItems.path);
                    OperationInterface.SetStatusText(ex.Message, OperationInterface.StatusText.Warning);
                }*/

            }

        }

        //向上搜索 树
        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }

        //鼠标右键单击文件树
        private void fileTree_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }

            TVItems tVItems = listItems.Find(delegate (TVItems tv) {
                return tv.ViewItem.Equals(treeViewItem);
            });

            if (tVItems == null)
            {
                return;
            }
            if (tVItems.isFolder)
            {
                FileTreeContextMenu(treeViewItem, tVItems);

            }
            else
            {
                FileTreeContextMenu(treeViewItem, tVItems, false);
            }
        }

        //文件树上下文菜单
        void FileTreeContextMenu(TreeViewItem treeViewItem, TVItems tVItems, bool isFolder = true)
        {
            ContextMenu contextMenu = new ContextMenu();
            CompositeCollection contextMenuBase = new CompositeCollection();
            CompositeCollection compositeCollection = new CompositeCollection();
            CollectionContainer collectionContainer = new CollectionContainer();

            MenuItem cmCut = new MenuItem();
            cmCut.Header = LocalizedLangExtension.GetString("Cut");
            cmCut.Click += (misender, mie) => CMCut_Click(treeViewItem, tVItems);

            MenuItem cmCopy = new MenuItem();
            cmCopy.Header = LocalizedLangExtension.GetString("Copy");
            cmCopy.Click += (misender, mie) => CMCopy_Click(treeViewItem, tVItems);

            MenuItem cmPaste = new MenuItem();
            cmPaste.Header = LocalizedLangExtension.GetString("Paste");
            //cmPaste.Click += (misender, mie) => CMRename_Click(treeViewItem, tVItems);

            MenuItem cmRename = new MenuItem();
            cmRename.Header = LocalizedLangExtension.GetString("Rename");
            cmRename.Click += (misender, mie) => CMRename_Click(treeViewItem, tVItems);

            MenuItem cmDelete = new MenuItem();
            cmDelete.Header = LocalizedLangExtension.GetString("Delete");
            cmDelete.Click += (misender, mie) => CMDelete_Click(treeViewItem, tVItems);


            MenuItem cmNewFolder = new MenuItem();
            cmNewFolder.Header = LocalizedLangExtension.GetString("NewFolder");
            cmNewFolder.Click += (misender, mie) => CMNewFolder_Click(treeViewItem, tVItems);

            MenuItem cmNewFile = new MenuItem();
            cmNewFile.Header = LocalizedLangExtension.GetString("NewFile");
            cmNewFile.Click += (misender, mie) => CMNewFile_Click(treeViewItem, tVItems);

            cmCut.IsEnabled = false;
            cmPaste.IsEnabled = false;
            cmNewFolder.IsEnabled = false;
            cmNewFile.IsEnabled = false;
            if (isFolder)
            {
                if (Clipboard.GetDataObject() == null)
                {
                    cmPaste.IsEnabled = false;
                }
                else
                {
                    //cmPaste.IsEnabled = true;
                }
                cmNewFolder.IsEnabled = true;
                cmNewFile.IsEnabled = true;
            }

            contextMenuBase.Add(cmCut);
            contextMenuBase.Add(cmCopy);
            contextMenuBase.Add(cmPaste);
            contextMenuBase.Add(cmDelete);
            contextMenuBase.Add(new Separator());
            contextMenuBase.Add(cmRename);
            contextMenuBase.Add(new Separator());
            contextMenuBase.Add(cmNewFolder);
            contextMenuBase.Add(cmNewFile);
            
            collectionContainer.Collection = contextMenuBase;
            compositeCollection.Add(collectionContainer);
            contextMenu.ItemsSource = compositeCollection;
            contextMenu.IsOpen = true;
        }

        //新建文件
        private void CMNewFile_Click(TreeViewItem tvi, TVItems itm)
        {
            if (itm.isFolder)
            {
                TreeViewItem tempchildNode = SetTreeViewItem("Resources/Images/DefaultFile.png", "", "");
                TVItems itemFile = new TVItems()
                {
                    ViewItem = tempchildNode,
                    path = itm.path + "\\",
                    isFolder = false
                };
                listItems.Add(itemFile);
                tvi.Items.Insert(0, tempchildNode);
                tvi.IsExpanded = true;
                new ContextMenu().IsOpen = true;
                CMRename_Click(tempchildNode, itemFile);
            }
        }

        //新建文件夹
        private void CMNewFolder_Click(TreeViewItem tvi, TVItems itm)
        {
            if (itm.isFolder)
            {
                TreeViewItem tempchildNode = SetTreeViewItem("Resources/Images/OpenFolder.png", "", "");
                TVItems itemFolder = new TVItems()
                {
                    ViewItem = tempchildNode,
                    path = itm.path + "\\",
                    isFolder = true
                };
                listItems.Add(itemFolder);
                tvi.Items.Insert(0, tempchildNode);
                tvi.IsExpanded = true;
                new ContextMenu().IsOpen = true;
                CMRename_Click(tempchildNode, itemFolder);
            }
        }

        //剪切
        private void CMCut_Click(TreeViewItem tvi, TVItems itm)
        {
            List<string> strs = new List<string>
            {
                itm.path
            };
            Util.CopyToClipboard(strs.ToArray(), true);
        }

        //复制
        private void CMCopy_Click(TreeViewItem tvi, TVItems itm)
        {
            List<string> strs = new List<string>
            {
                itm.path
            };
            Util.CopyToClipboard(strs.ToArray(), false);
        }

        //重命名
        private void CMRename_Click(TreeViewItem tvi, TVItems itm)
        {
            StackPanel stackPanel = tvi.Header as StackPanel;
            TextBlock textBlock = stackPanel.Children[1] as TextBlock;
            TextBox textBox = stackPanel.Children[2] as TextBox;
            textBox.KeyDown += TextBox_KeyDown;
            tvi.Focus();
            if (textBlock.Visibility == Visibility.Visible)
            {
                textBlock.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Visible;
                textBox.Focus();
                textBox.SelectAll();
            }
            else
            {
                textBlock.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Collapsed;
            }
        }

        //
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.KeyStates == System.Windows.Input.Keyboard.GetKeyStates(Key.Enter))
            {
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            }
        }

        //删除
        private void CMDelete_Click(TreeViewItem tvi, TVItems itm)
        {
            if (itm.isFolder)
            {
                DeleteFileAndDirectory(itm.path);
                DeleteFileTree(tvi);
            }
            else
            {
                File.Delete(itm.path);
                TVItems tVItemsd = listItems.Find(delegate (TVItems tv) {
                    return tv.path.Equals(itm.path);
                });
                if (tVItemsd != null)
                {
                    listItems.Remove(tVItemsd);
                }
                if (editorManager.currentPath == itm.path)
                {
                    editorManager.Clear();
                }
                DeleteFileTree(tvi);
            }

        }

        //删除文件树
        void DeleteFileTree(TreeViewItem tvi)
        {
            if ((tvi.Parent as TreeViewItem) == null)
            {
                fileTree.Items.Clear();
                listItems.Clear();
                editorManager.Clear();
                if (!string.IsNullOrEmpty(gameModFilePath))
                {
                    File.Delete(gameModFilePath);
                }
                gameModFilePath = "";
                gameModRootPath = "";
            }
            else
            {
                (tvi.Parent as TreeViewItem).Items.Remove(tvi);
            }
            
        }

        #endregion



        #region File...

        //新建文件
        private void menuNewFile_Click(object sender, RoutedEventArgs e)
        {
            editorManager.Clear();
        }

        //
        List<string> newTags = new List<string>();
        //新建模组
        private void menuNewMod_Click(object sender, RoutedEventArgs e)
        {
            MetroWindow win = new MetroWindow();
            win.Title = LocalizedLangExtension.GetString("NewModule");
            win.Width = 500;
            win.Height = 520;
            win.ShowMaxRestoreButton = false;
            win.ShowMinButton = false;
            win.WindowStyle = WindowStyle.None;
            win.WindowTitleBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C3C3C"));
            win.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            StackPanel stackPanelV = new StackPanel();
            StackPanel stackPanelNameAndVersion = new StackPanel();
            stackPanelNameAndVersion.Orientation = Orientation.Horizontal;

            StackPanel stackPanelName = new StackPanel();
            TextBlock textBlockName = new TextBlock();
            textBlockName.Text = LocalizedLangExtension.GetString("Name");
            textBlockName.Margin = new Thickness(32, 24, 0, 0);
            TextBox textBoxName = new TextBox();
            textBoxName.Width = 200;
            textBoxName.Margin = new Thickness(32, 8, 16, 0);
            stackPanelName.Children.Add(textBlockName);
            stackPanelName.Children.Add(textBoxName);

            StackPanel stackPanelVersion = new StackPanel();
            TextBlock textBlockVersion = new TextBlock();
            textBlockVersion.Text = LocalizedLangExtension.GetString("Version");
            textBlockVersion.Margin = new Thickness(24, 24,0, 0);
            TextBox textBoxVersion = new TextBox();
            textBoxVersion.Width = 200;
            textBoxVersion.Margin = new Thickness(24, 8, 16, 0);
            stackPanelVersion.Children.Add(textBlockVersion);
            stackPanelVersion.Children.Add(textBoxVersion);

            stackPanelNameAndVersion.Children.Add(stackPanelName);
            stackPanelNameAndVersion.Children.Add(stackPanelVersion);

            StackPanel stackPanelDirectory = new StackPanel();
            TextBlock textBlockDirectory = new TextBlock();
            textBlockDirectory.Text = LocalizedLangExtension.GetString("Directory");
            textBlockDirectory.Margin = new Thickness(32, 24, 0, 0);
            TextBox textBoxDirectory = new TextBox();
            textBoxDirectory.Text = "mod/";
            textBoxDirectory.Margin = new Thickness(32, 8, 24, 0);
            stackPanelDirectory.Children.Add(textBlockDirectory);
            stackPanelDirectory.Children.Add(textBoxDirectory);

            StackPanel stackPanelTag = new StackPanel();
            TextBlock textBlockTag = new TextBlock();
            textBlockTag.Text = LocalizedLangExtension.GetString("Tag");
            textBlockTag.Margin = new Thickness(32, 24, 0, 0);

            StackPanel stackPanelTagLeft = new StackPanel();
            stackPanelTagLeft.Children.Add(SetGridCheckBox("Alternative History"));
            stackPanelTagLeft.Children.Add(SetGridCheckBox("Events"));
            stackPanelTagLeft.Children.Add(SetGridCheckBox("Gameplay"));
            stackPanelTagLeft.Children.Add(SetGridCheckBox("Historical"));
            stackPanelTagLeft.Children.Add(SetGridCheckBox("Map"));
            stackPanelTagLeft.Children.Add(SetGridCheckBox("National Focuses"));
            stackPanelTagLeft.Children.Add(SetGridCheckBox("Technologies"));
            stackPanelTagLeft.Children.Add(SetGridCheckBox("Utilities"));
            stackPanelTagLeft.Margin = new Thickness(32, 8, 24, 0);
            stackPanelTagLeft.HorizontalAlignment = HorizontalAlignment.Left;

            StackPanel stackPanelTagRight = new StackPanel();
            stackPanelTagRight.Children.Add(SetGridCheckBox("Balance"));
            stackPanelTagRight.Children.Add(SetGridCheckBox("Fixes"));
            stackPanelTagRight.Children.Add(SetGridCheckBox("Graphics"));
            stackPanelTagRight.Children.Add(SetGridCheckBox("Ideologies"));
            stackPanelTagRight.Children.Add(SetGridCheckBox("Military"));
            stackPanelTagRight.Children.Add(SetGridCheckBox("Sound"));
            stackPanelTagRight.Children.Add(SetGridCheckBox("Translation"));
            stackPanelTagRight.Margin = new Thickness(80, 8, 24, 0);
            stackPanelTagRight.HorizontalAlignment = HorizontalAlignment.Right;
            StackPanel stackPanelTagChild = new StackPanel();
            stackPanelTagChild.Orientation = Orientation.Horizontal;
            stackPanelTagChild.Children.Add(stackPanelTagLeft);
            stackPanelTagChild.Children.Add(stackPanelTagRight);
            stackPanelTag.Children.Add(textBlockTag);
            stackPanelTag.Children.Add(stackPanelTagChild);

            StackPanel stackPanelCreate = new StackPanel();
            stackPanelCreate.Orientation = Orientation.Horizontal;
            Button buttonCreate = new Button();
            buttonCreate.SetValue(Button.StyleProperty, Application.Current.Resources["AccentedSquareButtonStyle"]);
            buttonCreate.Content = string.Format("  {0}  ",LocalizedLangExtension.GetString("CreateModule"));
            buttonCreate.Margin = new Thickness(40, 24, 0, 0);
            buttonCreate.Click += (s, ee) => ButtonCreate_Click(textBoxName, textBoxVersion, textBoxDirectory, win);
            Button buttonCancel = new Button();
            buttonCancel.Content = string.Format("       {0}       ", LocalizedLangExtension.GetString("Cancel"));
            buttonCancel.Margin = new Thickness(268, 24, 40, 0);
            buttonCancel.Click += (s,ee) => ButtonCancel_Click(win);
            stackPanelCreate.Children.Add(buttonCreate);
            stackPanelCreate.Children.Add(buttonCancel);

            stackPanelV.Children.Add(stackPanelNameAndVersion);
            stackPanelV.Children.Add(stackPanelDirectory);
            stackPanelV.Children.Add(stackPanelTag);
            stackPanelV.Children.Add(stackPanelCreate);

            win.Content = stackPanelV;

            win.ShowDialog();
        }

        private void ButtonCreate_Click(TextBox textBoxName, TextBox textBoxVersion, TextBox textBoxDirectory, MetroWindow win)
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show(LocalizedLangExtension.GetString("PleaseEnterName"));
                return;
            }
            if (string.IsNullOrWhiteSpace(textBoxVersion.Text))
            {
                MessageBox.Show(LocalizedLangExtension.GetString("PleaseEnterVersion"));
                return;
            }
            if (string.IsNullOrWhiteSpace(textBoxDirectory.Text.Replace("mod/", "")))
            {
                MessageBox.Show(LocalizedLangExtension.GetString("PleaseEnterDirectory"));
                return;
            }
            if (newTags.Count == 0)
            {
                MessageBox.Show(LocalizedLangExtension.GetString("PleaseSelectTag"));
                return;
            }
            
            string hoi4 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Paradox Interactive" + "\\" + "Hearts of Iron IV";
            string hoi4ModFile = hoi4 + "\\" + textBoxDirectory.Text + ".mod";
            string hoi4ModFolder = hoi4 + "\\" + textBoxDirectory.Text;

            //整合模组配置字符串
            string strDec = "";
            string strMod = "";
            strMod += string.Format("name=\"{0}\"\n", textBoxName.Text);
            strMod += string.Format("version=\"{0}\"\n", textBoxVersion.Text);
            strMod += "tags={\n";
            foreach (var item in newTags)
            {
                strMod += string.Format("\t\"{0}\"\n", item);
            }
            strMod += "}\n";
            strDec = strMod;
            strMod += string.Format("path=\"{0}\"\n", hoi4ModFolder.Replace("\\", "/"));

            FileStream fs = new FileStream(hoi4ModFile, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));
            sw.Write(strMod);
            sw.Close();
            fs.Close();

            Directory.CreateDirectory(hoi4ModFolder);

            FileStream fs1 = new FileStream(hoi4ModFolder + "\\" + "descriptor.mod", FileMode.Create);
            StreamWriter sw1 = new StreamWriter(fs1, new UTF8Encoding(false));
            sw1.Write(strDec);
            sw1.Close();
            fs1.Close();

            //读取模组后关闭此窗口
            LoadMod(hoi4ModFile);

            win.Close();
        }

        private void ButtonCancel_Click(MetroWindow win)
        {
            win.Close();
        }

        //设置复选框数据列表
        StackPanel SetGridCheckBox(string text)
        {
            StackPanel stackPanelTag = new StackPanel();
            stackPanelTag.Orientation = Orientation.Horizontal;

            CheckBox checkBox0 = new CheckBox();
            checkBox0.Content = text;
            checkBox0.Margin = new Thickness(8, 8, 0, 0);
            checkBox0.Click += CheckBox0_Click;


            stackPanelTag.Children.Add(checkBox0);
            
            return stackPanelTag;
        }

        private void CheckBox0_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked == true)
            {
                newTags.Add(checkBox.Content.ToString());
            }
            else
            {
                newTags.Remove(checkBox.Content.ToString());
            }
        }

        //打开文件
        private void menuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cofd = new CommonOpenFileDialog();
            cofd.IsFolderPicker = false;
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                gameModFilePath = "";
                gameModRootPath = "";

                fileTree.Items.Clear();
                listItems.Clear();
                editorManager.Clear();

                try
                {
                    // 后缀名匹配则解析，否则直接载入
                    if (Util.IsExtensionName(Path.GetExtension(cofd.FileName)))
                    {
                        //解析代码并格式化
                        new CodeForm(cofd.FileName, this, editorManager);
                    }
                    else
                    {
                        editorManager.LoadDataFromFile(cofd.FileName);
                    }
                }
                catch (Exception ex)
                {
                    //解析错误，直接载入
                    editorManager.LoadDataFromFile(cofd.FileName);
                    OperationInterface.SetStatusText(ex.Message, OperationInterface.StatusText.Warning);
                }
            }
        }
        
        // 打开文件夹
        private void menuOpenOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cofd = new CommonOpenFileDialog();
            cofd.IsFolderPicker = true;
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                new Task(() => {
                    this.BeginInvoke(new Action(() => {
                        gameModFilePath = "";
                        gameModRootPath = cofd.FileName;

                        fileTree.Items.Clear();
                        listItems.Clear();
                        editorManager.Clear();

                        TreeViewItem parentNode = SetTreeViewItem("Resources/Images/OpenFolder.png", Path.GetFileNameWithoutExtension(cofd.FileName), Path.GetFileName(gameModRootPath));

                        TVItems items = new TVItems()
                        {
                            ViewItem = parentNode,
                            path = gameModRootPath,
                            isFolder = true
                        };
                        listItems.Add(items);

                        LoadFileDirectory(gameModRootPath, parentNode);
                        parentNode.IsExpanded = true;//子项展开
                        fileTree.Items.Add(parentNode);
                    }));
                }).Start();
                
            }
        }

        
        // 打开模组
        private void menuOpenMod_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //ofd.InitialDirectory = @"D:\";
            ofd.Filter = "MOD模组|*.mod|所有文件|*.*";
            if (ofd.ShowDialog() == true)
            {
                //如果该目录不存在则返回
                if (!Directory.Exists(Path.GetDirectoryName(ofd.FileName) + "\\" + Path.GetFileNameWithoutExtension(ofd.FileName)))
                {
                    return;
                }
                //
                LoadMod(ofd.FileName);
            }

        }

        //读取模组
        public void LoadMod(string modPath)
        {
            new Task(() => {
                this.BeginInvoke(new Action(() => {
                    gameModFilePath = modPath;
                    gameModRootPath = Path.GetDirectoryName(modPath) + "\\" + Path.GetFileNameWithoutExtension(modPath);

                    fileTree.Items.Clear();
                    listItems.Clear();
                    editorManager.Clear();

                    FileStream fs = new FileStream(gameModFilePath, FileMode.Open);
                    StreamReader sr = new StreamReader(fs, new UTF8Encoding(false));
                    string mc = sr.ReadToEnd();
                    sr.Close();
                    fs.Close();
                    //模组配置
                    modConfig = new ModConfig(mc);
                    string name = "";
                    if (string.IsNullOrWhiteSpace(modConfig.name.Replace("\"", "")))
                    {
                        name = Path.GetFileNameWithoutExtension(gameModFilePath);
                    }
                    else
                    {
                        name = modConfig.name.Replace("\"", "");
                    }
                    Console.WriteLine(modConfig.name);
                    TreeViewItem parentNode = SetTreeViewItem("Resources/Images/OpenFolder.png", name, Path.GetFileName(gameModRootPath));
                    TVItems items = new TVItems()
                    {
                        ViewItem = parentNode,
                        path = gameModRootPath,
                        isFolder = true
                    };
                    listItems.Add(items);
                    LoadFileDirectory(gameModRootPath, parentNode);
                    parentNode.IsExpanded = true;//子项展开
                    fileTree.Items.Add(parentNode);
                    //
                    editorManager.LoadDataFromFile(gameModFilePath);
                }));
            }).Start();
        }

        //保存
        private void menuSave_Click(object sender, RoutedEventArgs e)
        {

        }

        //另存为
        private void menuSaveas_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "ME模组编辑文件|*.me|所有文件|*.*";
            if (sfd.ShowDialog() == true)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));
                sw.Write(editorManager.codeEditor.Text);
                sw.Close();
                fs.Close();
            }
        }

        //关闭项目
        private void menuCloseProject_Click(object sender, RoutedEventArgs e)
        {
            editorManager.Clear();
            fileTree.Items.Clear();
            listItems.Clear();
        }

        //模组设置
        private void menuModSetting_Click(object sender, RoutedEventArgs e)
        {
            editorManager.LoadDataFromFile(gameModFilePath);
        }

        //退出程序
        private void menuQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region CodeEditor ...

        //打开或关闭代码编辑器
        private void menuCodeEditor_Click(object sender, RoutedEventArgs e)
        {
            if (scintillaFormsHost.Visibility == Visibility.Visible)
            {
                scintillaFormsHost.Visibility = Visibility.Hidden;
            }else if (scintillaFormsHost.Visibility == Visibility.Hidden)
            {
                scintillaFormsHost.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region MOD文件树

        public TreeViewItem SetModTreeViewItem(string imagePath, string text)
        {
            TreeViewItem tvi = new TreeViewItem();
            StackPanel headerSP = new StackPanel();
            headerSP.Height = 20;
            headerSP.Orientation = Orientation.Horizontal;

            //0
            Image image = new Image();
            image.Height = 17;
            image.Source = new BitmapImage(new Uri("pack://application:,,,/" + imagePath));

            //1
            TextBlock textBlock = new TextBlock();
            //textBlock.FontSize = 12;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Margin = new Thickness(4, 0, 0, 0);
            textBlock.Text = text;

            headerSP.Children.Add(image);
            headerSP.Children.Add(textBlock);
            tvi.Header = headerSP;

            return tvi;
        }

        #endregion

        #region 设置文件树

        public TreeViewItem SetTreeViewItem(string imagePath, string text, string fileName)
        {
            TreeViewItem tvi = new TreeViewItem();
            StackPanel headerSP = new StackPanel();
            headerSP.Height = 20;
            headerSP.Orientation = Orientation.Horizontal;

            //0
            Image image = new Image();
            image.Height = 17;
            image.Source = new BitmapImage(new Uri("pack://application:,,,/" + imagePath));

            //1
            TextBlock textBlock = new TextBlock();
            //textBlock.FontSize = 12;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Margin = new Thickness(4,0,0,0);
            textBlock.Text = text;

            //2
            TextBox textBox = new TextBox();
            //textBox.FontSize = 12;
            textBox.VerticalAlignment = VerticalAlignment.Center;
            textBox.Margin = new Thickness(4, 0, 0, 0);
            textBox.Text = fileName;
            textBox.TextChanged += TextBox_TextChanged;
            textBox.LostFocus += TextBox_LostFocus;
            textBox.Visibility = Visibility.Collapsed;

            headerSP.Children.Add(image);
            headerSP.Children.Add(textBlock);
            headerSP.Children.Add(textBox);
            tvi.Header = headerSP;

            return tvi;
        }

        //文本更改
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            TextBlock textBlock = ((sender as TextBox).Parent as StackPanel).Children[1] as TextBlock;
            TreeViewItem tempTreeViewItem = ((sender as TextBox).Parent as StackPanel).Parent as TreeViewItem;
            TVItems tVItems = listItems.Find(delegate (TVItems tv) {
                return tv.ViewItem.Equals(tempTreeViewItem);
            });
            TVItems tVItems1 = listItems.Find(delegate (TVItems tv) {
                return tv.path.Equals(Path.GetDirectoryName(tVItems.path) + "\\" + textBox.Text);
            });

            if (tVItems1 != null && tVItems1 != tVItems)
            {
                StackPanel stackPanel = new StackPanel();
                TextBlock textBlock1 = new TextBlock();
                textBlock1.Foreground = Brushes.Red;
                textBlock1.Text = "该文件或文件夹已存在！";
                stackPanel.Children.Add(textBlock1);
                textBox.ToolTip = stackPanel;
            }
            else if (textBox.Text.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                StackPanel stackPanel = new StackPanel();
                TextBlock textBlock1 = new TextBlock();
                textBlock1.Foreground = Brushes.Red;
                textBlock1.Text = "文件名无效！";
                stackPanel.Children.Add(textBlock1);
                textBox.ToolTip = stackPanel;
            }
            else
            {
                textBox.ToolTip = null;
            }
        }

        //失去焦点
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Image image = ((sender as TextBox).Parent as StackPanel).Children[0] as Image;
            TextBlock textBlock = ((sender as TextBox).Parent as StackPanel).Children[1] as TextBlock;
            TreeViewItem tempTreeViewItem = ((sender as TextBox).Parent as StackPanel).Parent as TreeViewItem;
            TVItems tVItems = listItems.Find(delegate (TVItems tv) {
                return tv.ViewItem.Equals(tempTreeViewItem);
            });
            //文件或文件夹是否存在
            bool isFF = !string.IsNullOrWhiteSpace(Path.GetFileName(tVItems.path));
            
            //新建
            if (!isFF)
            {
                TVItems tVItems1 = listItems.Find(delegate (TVItems tv) {
                    return tv.path.Equals(Path.GetDirectoryName(tVItems.path) + "\\" + textBox.Text);
                });

                //当前节点是空白，则删除 || 重复文件名 || 文件名无效
                if (string.IsNullOrWhiteSpace(textBox.Text) || tVItems1 != null || textBox.Text.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    (tempTreeViewItem.Parent as TreeViewItem).Items.Remove(tempTreeViewItem);
                    listItems.Remove(tVItems);
                }
                else
                {
                    if (!tVItems.isFolder)
                    {
                        string str = textBox.Text.Replace(Util.extension, "");
                        textBox.Text = str + ".me";
                    }

                    tVItems.path = Path.GetDirectoryName(tVItems.path) + "\\" + textBox.Text;
                    textBox.Text = Path.GetFileName(tVItems.path);
                    if (tVItems.isFolder)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(tVItems.path) + "\\" + textBox.Text);
                        textBlock.Text = LocalizedLangExtension.GetString(Path.GetFileNameWithoutExtension(tVItems.path));
                    }
                    else
                    {
                        FileStream fs = new FileStream(Path.GetDirectoryName(tVItems.path) + "\\" + textBox.Text, FileMode.Create);
                        StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));

                        var serializer = new SerializerBuilder().Build();
                        var yaml = serializer.Serialize(new List<ModClass>());
                        sw.Write(yaml);

                        sw.Close();
                        fs.Close();
                        textBlock.Text = Path.GetFileName(tVItems.path);

                        //后缀
                        if (Path.GetExtension(tVItems.path) == ".me")
                        {
                            image.Source = new BitmapImage(new Uri("pack://application:,,,/" + "Resources/Images/CodeFile.png"));
                        }
                    }

                    //选中更改
                    tempTreeViewItem.IsSelected = false;
                    tempTreeViewItem.IsSelected = true;
                }

            }
            else
            {
                //重命名
                if (textBox.Text != Path.GetFileName(tVItems.path) && !(textBox.Text.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    try
                    {
                        //上级目录
                        string pd = Path.GetDirectoryName(tVItems.path) + "\\";
                        //要修改的路径
                        string md = tVItems.path;
                        //
                        Computer computer = new Computer();
                        if (tVItems.isFolder)
                        {
                            computer.FileSystem.RenameDirectory(tVItems.path, textBox.Text);
                        }
                        else
                        {
                            string str = textBox.Text.Replace(Util.extension, "");
                            textBox.Text = str + ".me";
                            computer.FileSystem.RenameFile(tVItems.path, textBox.Text);
                        }
                        //遍历相同的路径并更改
                        foreach (TVItems itm in listItems)
                        {
                            itm.path = itm.path.Replace(md, pd + textBox.Text);
                        }
                        tVItems.path = pd + textBox.Text;
                        if (tVItems.isFolder)
                        {
                            textBlock.Text = LocalizedLangExtension.GetString(Path.GetFileNameWithoutExtension(tVItems.path));
                        }
                        else
                        {
                            textBlock.Text = Path.GetFileName(tVItems.path);

                            //后缀
                            if (Path.GetExtension(tVItems.path) == ".me")
                            {
                                image.Source = new BitmapImage(new Uri("pack://application:,,,/" + "Resources/Images/CodeFile.png"));
                            }
                        }
                        textBox.Text = Path.GetFileName(tVItems.path);

                        string tmd = md.Replace("\\", "\\\\");
                        //是否匹配已打开的文件,匹配则重载
                        if (Regex.IsMatch(editorManager.currentPath, @tmd))
                        {
                            //选中更改
                            tempTreeViewItem.IsSelected = false;
                            tempTreeViewItem.IsSelected = true;
                        }
                    }
                    catch (Exception)
                    {
                        textBox.Text = Path.GetFileName(tVItems.path);
                    }
                    
                }
                else
                {
                    textBox.Text = Path.GetFileName(tVItems.path);
                }
            }

            textBox.Visibility = Visibility.Collapsed;
            textBlock.Visibility = Visibility.Visible;

        }

        #endregion

        #region ModClass

        //更改
        private void modClassTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)e.NewValue;   // 获取到选中的TreeViewItem对象

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }


            Console.WriteLine(treeViewItem);
        }

        //右键弹起
        private void modClass_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }


            
        }

        //双击
        private void modClass_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }



        }


        #endregion


        /// <summary>
        /// 加载文件目录树
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tvi"></param>
        //TreeViewItem tvi
        private void LoadFileDirectory(string path, TreeViewItem parentNode)
        {
            DirectoryInfo theFolder = new DirectoryInfo(@path);

            //遍历文件夹
            foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
            {
                TreeViewItem childNode = SetTreeViewItem("Resources/Images/OpenFolder.png", LocalizedLangExtension.GetString(NextFolder.Name), NextFolder.Name);
                TVItems itemDir = new TVItems()
                {
                    ViewItem = childNode,
                    path = path + "\\" + NextFolder.Name,
                    isFolder = true
                };
                listItems.Add(itemDir);
                parentNode.Items.Add(childNode);
                LoadFileDirectory(NextFolder.FullName, childNode);
            }

            //遍历文件
            foreach (FileInfo NextFile in theFolder.GetFiles())
            {
                TreeViewItem childNode;
                if (NextFile.Extension == Util.extension)
                {
                    childNode = SetTreeViewItem("Resources/Images/CodeFile.png", NextFile.Name, NextFile.Name);
                }
                else
                {
                    childNode = SetTreeViewItem("Resources/Images/DefaultFile.png", NextFile.Name, NextFile.Name);

                }
                TVItems itemFile = new TVItems()
                {
                    ViewItem = childNode,
                    path = path + "\\" + NextFile.Name,
                    isFolder = false
                };
                listItems.Add(itemFile);
                parentNode.Items.Add(childNode);
            }

        }

        /// <summary>
        /// 遍历文件夹和文件夹并删除
        /// </summary>
        /// <param name="path"></param>
        private void DeleteFileAndDirectory(string path)
        {
            DirectoryInfo theFolder = new DirectoryInfo(@path);

            //遍历文件
            foreach (FileInfo NextFile in theFolder.GetFiles())
            {
                File.Delete(NextFile.FullName);
                TVItems tVItems = listItems.Find(delegate (TVItems tv) {
                    return tv.path.Equals(NextFile.FullName);
                });
                if (tVItems != null)
                {
                    listItems.Remove(tVItems);
                }
            }

            //遍历文件夹
            foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
            {
                DeleteFileAndDirectory(NextFolder.FullName);
                //Directory.Delete(NextFolder.FullName);
                TVItems tVItems = listItems.Find(delegate (TVItems tv) {
                    return tv.path.Equals(NextFolder.FullName);
                });
                if (tVItems != null)
                {
                    listItems.Remove(tVItems);
                }
            }

            Directory.Delete(theFolder.FullName);
            TVItems tVItemsd = listItems.Find(delegate (TVItems tv) {
                return tv.path.Equals(theFolder.FullName);
            });
            if (tVItemsd != null)
            {
                listItems.Remove(tVItemsd);
            }
            if (editorManager.currentPath == theFolder.FullName)
            {
                editorManager.Clear();
            }
        }

        
    }
}
