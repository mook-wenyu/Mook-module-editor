using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MookEditor.FileTree
{
    /// <summary>
    /// 树模型
    /// </summary>
    public class TreeModel : INotifyPropertyChanged
    {
        private ObservableCollection<TreeViewItem> _childNodes = new ObservableCollection<TreeViewItem>();


        public ObservableCollection<TreeViewItem> childNodes
        {
            get { return this._childNodes; }
            set { this._childNodes = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /*public TreeNodeModel searchTreeNode(ObservableCollection<TreeNodeModel> nodes, string searchKey)
        {
            TreeNodeModel returnTreeNodeMode = null;
            foreach (TreeNodeModel node in nodes)
            {
                if (node.Name.Contains(searchKey))
                {
                    return node;
                }
                returnTreeNodeMode = this.searchTreeNode(node.ChildNodes, searchKey);
            }
            return returnTreeNodeMode;
        }*/

    }
}
