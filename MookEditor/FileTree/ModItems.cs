using MookEditor.FileTree;
using MookEditor.MOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MookEditor.Utils
{
    public class ModItems
    {
        /// <summary>
        /// TreeView子项
        /// </summary>
        public TreeViewItem ViewItem
        {
            get; set;
        }

        /// <summary>
        /// 层级
        /// </summary>
        public int Level
        {
            get; set;
        }

        /// <summary>
        /// 编号
        /// </summary>
        public int Id
        {
            get; set;
        }

        /// <summary>
        /// 类型
        /// </summary>
        public MOD.Type Type
        {
            get; set;
        }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// > < 默认空= 运算符
        /// </summary>
        public string Op
        {
            get; set;
        }

        /// <summary>
        /// 是函数有值
        /// </summary>
        public string Val
        {
            get; set;
        }

        public ModItems()
        {
        }
    }

}
