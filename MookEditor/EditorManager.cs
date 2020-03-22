using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MookEditor.Utils;
using MookEditor.Utils.Localization;
using ScintillaNET;

namespace MookEditor
{
    public class EditorManager
    {
		public MainWindow mainWindow;
		public Scintilla codeEditor;
		public string currentPath;

        public EditorManager(MainWindow mainWindow ,Scintilla codeEditor)
        {
			this.mainWindow = mainWindow;
			this.codeEditor = codeEditor;
            // 基础配置
            this.codeEditor.TextChanged += CodeEditor_TextChanged;

            //初始化视图配置
            this.codeEditor.WrapMode = WrapMode.None;
            this.codeEditor.IndentationGuides = IndentView.LookBoth;

			//样式
			InitStyles();
			InitColors();
            InitSyntaxColoring();

			// 边距数
			InitNumberMargin();

			// 书签边距
			//InitBookmarkMargin();

			// 代码折叠边距
			InitCodeFolding();

			// 托拽
			InitDragDropFile();

			// 默认文件
			//LoadDataFromFile("../../EditorManager.cs");

			// 初始化热键
			InitHotkeys();
		}

        private void CodeEditor_TextChanged(object sender, EventArgs e)
        {
            //Console.WriteLine((sender as Scintilla).Text);
        }

		private void InitStyles()
		{
			this.codeEditor.CaretForeColor = Util.IntToColor(0xD4D4D4);
			this.codeEditor.BorderStyle = BorderStyle.None;
		}

        private void InitColors()
        {
            codeEditor.SetSelectionBackColor(true, Util.IntToColor(0x114D9C));
        }

        private void InitSyntaxColoring()
        {

            // 配置默认样式
            codeEditor.StyleResetDefault();
            codeEditor.Styles[Style.Default].Font = "Consolas";
            codeEditor.Styles[Style.Default].Size = 10;
            codeEditor.Styles[Style.Default].BackColor = Util.IntToColor(0x1E1E1E);
			codeEditor.Styles[Style.Default].ForeColor = Util.IntToColor(0xD4D4D4);
			codeEditor.StyleClearAll();
			
			// 配置 CPP (C#) 词法分析程序风格
			//codeEditor.Styles[Style.Cpp.Identifier].ForeColor = Util.IntToColor(0xD0DAE2);
			codeEditor.Styles[Style.Cpp.Identifier].ForeColor = Util.IntToColor(0xD4D4D4); // 标识符 变量D4D4D4 9CDCFE
			codeEditor.Styles[Style.Cpp.Comment].ForeColor = Util.IntToColor(0x2FAE35); // /*
            codeEditor.Styles[Style.Cpp.CommentLine].ForeColor = Util.IntToColor(0x40BF57);// //
			codeEditor.Styles[Style.Cpp.CommentDoc].ForeColor = Util.IntToColor(0x2FAE35);// /**/
            codeEditor.Styles[Style.Cpp.Number].ForeColor = Util.IntToColor(0xB5CEA8); // 数字
            codeEditor.Styles[Style.Cpp.String].ForeColor = Util.IntToColor(0xD69D85); // 字符串""
			codeEditor.Styles[Style.Cpp.Character].ForeColor = Util.IntToColor(0xD69D85); // 字符''
			codeEditor.Styles[Style.Cpp.Preprocessor].ForeColor = Util.IntToColor(0x57A64A);// #
            codeEditor.Styles[Style.Cpp.Operator].ForeColor = Util.IntToColor(0xB4B4B4); //操作符 =+-*/
            codeEditor.Styles[Style.Cpp.Regex].ForeColor = Util.IntToColor(0xff00ff);
            codeEditor.Styles[Style.Cpp.CommentLineDoc].ForeColor = Util.IntToColor(0x608B4E); // ///
			codeEditor.Styles[Style.Cpp.Word].ForeColor = Util.IntToColor(0x48A8EE); //保留字
			codeEditor.Styles[Style.Cpp.Word2].ForeColor = Util.IntToColor(0x4EC9B0); // 保留字首字母大写
            codeEditor.Styles[Style.Cpp.CommentDocKeyword].ForeColor = Util.IntToColor(0xB3D991);
            codeEditor.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = Util.IntToColor(0xFF0000);
            codeEditor.Styles[Style.Cpp.GlobalClass].ForeColor = Util.IntToColor(0x48A8EE);

			codeEditor.Lexer = Lexer.Cpp;

			//codeEditor.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
			//codeEditor.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");
			codeEditor.SetKeywords(0, "name tags version path supported_version remote_file_id ");
		}

		private void InitHotkeys()
		{
			//new HotKey(ModifierKeys.Windows | ModifierKeys.Alt, Keys.Left, mainWindow).HotKeyPressed += (k) => Console.Beep();

			//new HotKey(ModifierKeys.Control, Keys.F, mainWindow).HotKeyPressed += (k) => Console.Beep();
			//new HotKey(ModifierKeys.Control, Keys.H, mainWindow).HotKeyPressed += (k) => Console.Beep();
			new HotKey(ModifierKeys.Control, Keys.U, mainWindow).HotKeyPressed += (k) => Uppercase();
			new HotKey(ModifierKeys.Control, Keys.L, mainWindow).HotKeyPressed += (k) => Lowercase();
			new HotKey(ModifierKeys.Control, Keys.Oemplus, mainWindow).HotKeyPressed += (k) => ZoomIn();
			new HotKey(ModifierKeys.Control, Keys.OemMinus, mainWindow).HotKeyPressed += (k) => ZoomOut();
			new HotKey(ModifierKeys.Control, Keys.D0, mainWindow).HotKeyPressed += (k) => ZoomDefault();
			//new HotKey(ModifierKeys.Control, Keys.Escape, mainWindow).HotKeyPressed += (k) => Console.Beep();
			// register the hotkeys with the form
			/*HotKeyManager.AddHotKey(this, OpenSearch, Keys.F, true);
			HotKeyManager.AddHotKey(this, OpenFindDialog, Keys.F, true, false, true);
			HotKeyManager.AddHotKey(this, OpenReplaceDialog, Keys.R, true);
			HotKeyManager.AddHotKey(this, OpenReplaceDialog, Keys.H, true);
			HotKeyManager.AddHotKey(this, Uppercase, Keys.U, true);
			HotKeyManager.AddHotKey(this, Lowercase, Keys.L, true);
			HotKeyManager.AddHotKey(this, ZoomIn, Keys.Oemplus, true);
			HotKeyManager.AddHotKey(this, ZoomOut, Keys.OemMinus, true);
			HotKeyManager.AddHotKey(this, ZoomDefault, Keys.D0, true);
			HotKeyManager.AddHotKey(this, CloseSearch, Keys.Escape);*/

			// 从 scintilla 移除冲突热键
			codeEditor.ClearCmdKey(Keys.Control | Keys.F);
			codeEditor.ClearCmdKey(Keys.Control | Keys.R);
			codeEditor.ClearCmdKey(Keys.Control | Keys.H);
			codeEditor.ClearCmdKey(Keys.Control | Keys.L);
			codeEditor.ClearCmdKey(Keys.Control | Keys.U);

		}

		public void Clear()
		{
			currentPath = "";
			codeEditor.ClearAll();
			mainWindow.Title = LocalizedLangExtension.GetString("MookEditor");
		}

		#region Numbers, Bookmarks, Code Folding

		/// <summary>
		/// 文本区域的背景颜色
		/// </summary>
		//private const int BACK_COLOR = 0x2A211C;
		private const int BACK_COLOR = 0x313131;

		/// <summary>
		/// 文本区域的默认文本颜色
		/// </summary>
		private const int FORE_COLOR = 0xB7B7B7;

		/// <summary>
		/// 将其更改为您希望显示行号的任何边距
		/// </summary>
		private const int NUMBER_MARGIN = 1;

		/// <summary>
		/// 将其更改为您希望书签/断点显示的任何边距
		/// </summary>
		private const int BOOKMARK_MARGIN = 2;
		private const int BOOKMARK_MARKER = 2;

		/// <summary>
		/// 将其更改为您希望代码折叠树(+/-)显示的任何边距
		/// </summary>
		private const int FOLDING_MARGIN = 3;

		/// <summary>
		/// 将此设置为true以显示用于代码折叠的圆形按钮(页边距上的[+]和[-]按钮)
		/// </summary>
		private const bool CODEFOLDING_CIRCULAR = false;

		// 初始化边距数
		private void InitNumberMargin()
		{

			codeEditor.Styles[Style.LineNumber].BackColor = Util.IntToColor(BACK_COLOR);
			codeEditor.Styles[Style.LineNumber].ForeColor = Util.IntToColor(FORE_COLOR);
			codeEditor.Styles[Style.IndentGuide].ForeColor = Util.IntToColor(FORE_COLOR);
			codeEditor.Styles[Style.IndentGuide].BackColor = Util.IntToColor(BACK_COLOR);

			var nums = codeEditor.Margins[NUMBER_MARGIN];
			nums.Width = 40;
			nums.Type = MarginType.Number;
			nums.Sensitive = true;
			nums.Mask = 0;

			codeEditor.MarginClick += codeEditor_MarginClick;
		}

		//初始化书签边距
		private void InitBookmarkMargin()
		{

			//codeEditor.SetFoldMarginColor(true, Util.IntToColor(BACK_COLOR));

			var margin = codeEditor.Margins[BOOKMARK_MARGIN];
			margin.Width = 20;
			margin.Sensitive = true;
			margin.Type = MarginType.Symbol;
			margin.Mask = (1 << BOOKMARK_MARKER);
			//margin.Cursor = MarginCursor.Arrow;

			var marker = codeEditor.Markers[BOOKMARK_MARKER];
			marker.Symbol = MarkerSymbol.Circle;
			marker.SetBackColor(Util.IntToColor(0xFF003B));
			marker.SetForeColor(Util.IntToColor(0x000000));
			marker.SetAlpha(100);

		}

		//初始化代码折叠
		private void InitCodeFolding()
		{

			codeEditor.SetFoldMarginColor(true, Util.IntToColor(BACK_COLOR));
			codeEditor.SetFoldMarginHighlightColor(true, Util.IntToColor(BACK_COLOR));

			// 启用代码折叠
			codeEditor.SetProperty("fold", "1");
			codeEditor.SetProperty("fold.compact", "1");

			// 配置页边距以显示可折叠符号
			codeEditor.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
			codeEditor.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
			codeEditor.Margins[FOLDING_MARGIN].Sensitive = true;
			codeEditor.Margins[FOLDING_MARGIN].Width = 20;

			// 设置所有折叠标记的颜色
			for (int i = 25; i <= 31; i++)
			{
				codeEditor.Markers[i].SetForeColor(Util.IntToColor(BACK_COLOR)); // [+]和[-]的样式
				codeEditor.Markers[i].SetBackColor(Util.IntToColor(FORE_COLOR)); // [+]和[-]的样式
			}

			// 使用相应的符号配置折叠标记
			codeEditor.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
			codeEditor.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
			codeEditor.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
			codeEditor.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
			codeEditor.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
			codeEditor.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
			codeEditor.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

			// 启用自动折叠
			codeEditor.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

		}

		private void codeEditor_MarginClick(object sender, MarginClickEventArgs e)
		{
			if (e.Margin == BOOKMARK_MARGIN)
			{
				// 这条线有标记吗?
				const uint mask = (1 << BOOKMARK_MARKER);
				var line = codeEditor.Lines[codeEditor.LineFromPosition(e.Position)];
				if ((line.MarkerGet() & mask) > 0)
				{
					// 删除现有的书签
					line.MarkerDelete(BOOKMARK_MARKER);
				}
				else
				{
					// 添加书签
					line.MarkerAdd(BOOKMARK_MARKER);
				}
			}
		}

		#endregion

		#region Drag & Drop File

		public void InitDragDropFile()
		{

			codeEditor.AllowDrop = true;
			codeEditor.DragEnter += delegate (object sender, DragEventArgs e) {
				if (e.Data.GetDataPresent(DataFormats.FileDrop))
					e.Effect = DragDropEffects.Copy;
				else
					e.Effect = DragDropEffects.None;
			};
			codeEditor.DragDrop += delegate (object sender, DragEventArgs e) {

				// get file drop
				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{

					Array a = (Array)e.Data.GetData(DataFormats.FileDrop);
					if (a != null)
					{

						string path = a.GetValue(0).ToString();

						LoadDataFromFile(path);

					}
				}
			};

		}

		public void LoadDataFromFile(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					if (Util.IsExtensionName(Path.GetExtension(path)))
					{
						FileStream fs = new FileStream(path, FileMode.Open);
						StreamReader sr = new StreamReader(fs, new UTF8Encoding(false));
						mainWindow.Title = LocalizedLangExtension.GetString("MookEditor") + "  -  " + Path.GetFileName(path);
						currentPath = path;
						codeEditor.Text = sr.ReadToEnd();
						sr.Close();
						fs.Close();
					}
					else
					{
						return;
					}
					
				}
				catch (Exception)
				{
					MessageBox.Show("该文件不可读写，请取消只读");
				}
				
				
			}
		}

		/// <summary>
		/// 只允许修改后重载
		/// </summary>
		/// <param name="str"></param>
		public void LoadDataString(string str, string path)
		{
			mainWindow.Title = LocalizedLangExtension.GetString("MookEditor") + "  -  " + Path.GetFileName(path);
			currentPath = path;
			codeEditor.Text = str;
		}

		#endregion

		#region Main Menu Commands

		/*private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				LoadDataFromFile(openFileDialog.FileName);
			}
		}

		private void findToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenSearch();
		}

		private void findDialogToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFindDialog();
		}

		private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenReplaceDialog();
		}

		private void cutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			codeEditor.Cut();
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			codeEditor.Copy();
		}

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			codeEditor.Paste();
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			codeEditor.SelectAll();
		}

		private void selectLineToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Line line = codeEditor.Lines[codeEditor.CurrentLine];
			codeEditor.SetSelection(line.Position + line.Length, line.Position);
		}

		private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			codeEditor.SetEmptySelection(0);
		}

		private void indentSelectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Indent();
		}

		private void outdentSelectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Outdent();
		}

		private void uppercaseSelectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Uppercase();
		}

		private void lowercaseSelectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Lowercase();
		}

		private void wordWrapToolStripMenuItem1_Click(object sender, EventArgs e)
		{

			// toggle word wrap
			wordWrapItem.Checked = !wordWrapItem.Checked;
			codeEditor.WrapMode = wordWrapItem.Checked ? WrapMode.Word : WrapMode.None;
		}

		private void indentGuidesToolStripMenuItem_Click(object sender, EventArgs e)
		{

			// toggle indent guides
			indentGuidesItem.Checked = !indentGuidesItem.Checked;
			codeEditor.IndentationGuides = indentGuidesItem.Checked ? IndentView.LookBoth : IndentView.None;
		}

		private void hiddenCharactersToolStripMenuItem_Click(object sender, EventArgs e)
		{

			// toggle view whitespace
			hiddenCharactersItem.Checked = !hiddenCharactersItem.Checked;
			codeEditor.ViewWhitespace = hiddenCharactersItem.Checked ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;
		}*/

		private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ZoomIn();
		}

		private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ZoomOut();
		}

		private void zoom100ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ZoomDefault();
		}

		private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			codeEditor.FoldAll(FoldAction.Contract);
		}

		private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			codeEditor.FoldAll(FoldAction.Expand);
		}


		#endregion

		#region Uppercase / Lowercase

		private void Lowercase()
		{

			// save the selection
			int start = codeEditor.SelectionStart;
			int end = codeEditor.SelectionEnd;

			// modify the selected text
			codeEditor.ReplaceSelection(codeEditor.GetTextRange(start, end - start).ToLower());

			// preserve the original selection
			codeEditor.SetSelection(start, end);
		}

		private void Uppercase()
		{

			// save the selection
			int start = codeEditor.SelectionStart;
			int end = codeEditor.SelectionEnd;

			// modify the selected text
			codeEditor.ReplaceSelection(codeEditor.GetTextRange(start, end - start).ToUpper());

			// preserve the original selection
			codeEditor.SetSelection(start, end);
		}

		#endregion


		#region Zoom

		private void ZoomIn()
		{
			codeEditor.ZoomIn();
		}

		private void ZoomOut()
		{
			codeEditor.ZoomOut();
		}

		private void ZoomDefault()
		{
			codeEditor.Zoom = 0;
		}


		#endregion

		#region Quick Search Bar

		/*bool SearchIsOpen = false;

		private void OpenSearch()
		{

			SearchManager.SearchBox = TxtSearch;
			SearchManager.codeEditor = codeEditor;

			if (!SearchIsOpen)
			{
				SearchIsOpen = true;
				InvokeIfNeeded(delegate () {
					PanelSearch.Visible = true;
					TxtSearch.Text = SearchManager.LastSearch;
					TxtSearch.Focus();
					TxtSearch.SelectAll();
				});
			}
			else
			{
				InvokeIfNeeded(delegate () {
					TxtSearch.Focus();
					TxtSearch.SelectAll();
				});
			}
		}
		private void CloseSearch()
		{
			if (SearchIsOpen)
			{
				SearchIsOpen = false;
				InvokeIfNeeded(delegate () {
					PanelSearch.Visible = false;
					//CurBrowser.GetBrowser().StopFinding(true);
				});
			}
		}

		private void BtnClearSearch_Click(object sender, EventArgs e)
		{
			CloseSearch();
		}

		private void BtnPrevSearch_Click(object sender, EventArgs e)
		{
			SearchManager.Find(false, false);
		}
		private void BtnNextSearch_Click(object sender, EventArgs e)
		{
			SearchManager.Find(true, false);
		}
		private void TxtSearch_TextChanged(object sender, EventArgs e)
		{
			SearchManager.Find(true, true);
		}

		private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (HotKeyManager.IsHotkey(e, Keys.Enter))
			{
				SearchManager.Find(true, false);
			}
			if (HotKeyManager.IsHotkey(e, Keys.Enter, true) || HotKeyManager.IsHotkey(e, Keys.Enter, false, true))
			{
				SearchManager.Find(false, false);
			}
		}*/

		#endregion

		#region Find & Replace Dialog

		private void OpenFindDialog()
		{

		}
		private void OpenReplaceDialog()
		{


		}

		#endregion

    }
}
