using MookEditor.FileTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MookEditor.Utils
{
    public class TVItems
    {
        /// <summary>
        /// TreeView子项
        /// </summary>
        public TreeViewItem ViewItem
        {
            get; set;
        }

        /// <summary>
        /// 该子项的路径
        /// </summary>
        public string path
        {
            get; set;
        }

        /// <summary>
        /// 该子项的是否为文件夹
        /// </summary>
        public bool isFolder
        {
            get; set;
        }

        public TVItems()
        {
        }
    }

}
