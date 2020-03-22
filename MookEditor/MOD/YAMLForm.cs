using MookEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using YamlDotNet.Serialization;

namespace MookEditor.MOD
{
    public class YAMLForm
    {
        public MainWindow mainWindow;
        public List<ModItems> modItemsParent = new List<ModItems>();
        public EditorManager editorManager;
        public string fvPath;
        List<ModClass> modClassParent = new List<ModClass>();

        public YAMLForm(string fvPath, MainWindow mainWindow, EditorManager editorManager)
        {
            this.mainWindow = mainWindow;
            this.fvPath = fvPath;
            this.editorManager = editorManager;
            this.editorManager.Clear();
            
            FileStream fs = new FileStream(fvPath, FileMode.Open);
            StreamReader sr = new StreamReader(fs, new UTF8Encoding(false));
            var deserializer = new DeserializerBuilder().Build();
            modClassParent = deserializer.Deserialize<List<ModClass>>(sr);
            sr.Close();
            fs.Close();
            
            mainWindow.modClassTree.Items.Clear();
            LoadYamlCode(modClassParent, null, mainWindow.modClassTree);
            
        }


        public void LoadYamlCode(List<ModClass> modClassParent, TreeViewItem modCTIParent = null, TreeView rootModCTIParent = null)
        {
            for (int i = 0; i < modClassParent.Count; i++)
            {
                if (modClassParent[i].type == Type.variable)
                {
                    string op;
                    if (modClassParent[i].op == ">")
                    {
                        op = ">";
                    }
                    else if (modClassParent[i].op == "<")
                    {
                        op = "<";
                    }
                    else if (modClassParent[i].op == "=")
                    {
                        op = "=";
                    }
                    else
                    {
                        op = " ";
                    }

                    TreeViewItem tviChild;

                    if (string.IsNullOrWhiteSpace(op))
                    {
                        //空
                        tviChild = mainWindow.SetModTreeViewItem("Resources/Images/FX.png", string.Format("{0} {1} {2}", modClassParent[i].name, op, modClassParent[i].val));
                        ModItems modItemsChild = new ModItems();
                        modItemsChild.ViewItem = tviChild;
                        modItemsChild.Level = modClassParent[i].level;
                        modItemsChild.Id = modClassParent[i].id;
                        modItemsChild.Type = modClassParent[i].type;
                        modItemsChild.Name = modClassParent[i].name;
                        modItemsChild.Op = modClassParent[i].op;
                        modItemsChild.Val = modClassParent[i].val;
                        modItemsParent.Add(modItemsChild);

                    }
                    else
                    {
                        tviChild = mainWindow.SetModTreeViewItem("Resources/Images/FX.png", string.Format("{0} {1} {2}", modClassParent[i].name, op, modClassParent[i].val));
                        ModItems modItemsChild = new ModItems();
                        modItemsChild.ViewItem = tviChild;
                        modItemsChild.Level = modClassParent[i].level;
                        modItemsChild.Id = modClassParent[i].id;
                        modItemsChild.Type = modClassParent[i].type;
                        modItemsChild.Name = modClassParent[i].name;
                        modItemsChild.Op = modClassParent[i].op;
                        modItemsChild.Val = modClassParent[i].val;
                        modItemsParent.Add(modItemsChild);
                    }
                    
                    if (modCTIParent == null)
                    {
                        rootModCTIParent.Items.Add(tviChild);
                        tviChild.IsExpanded = true;
                    }
                    else
                    {
                        modCTIParent.Items.Add(tviChild);
                    }
                }
                else if (modClassParent[i].type == Type.function)
                {
                    TreeViewItem tviChild = mainWindow.SetModTreeViewItem("Resources/Images/Trigger.png", string.Format("{0}", modClassParent[i].name));
                    ModItems modItemsChild = new ModItems();
                    modItemsChild.ViewItem = tviChild;
                    modItemsChild.Level = modClassParent[i].level;
                    modItemsChild.Id = modClassParent[i].id;
                    modItemsChild.Type = modClassParent[i].type;
                    modItemsChild.Name = modClassParent[i].name;
                    modItemsChild.Op = modClassParent[i].op;
                    modItemsChild.Val = modClassParent[i].val;
                    modItemsParent.Add(modItemsChild);

                    if (modCTIParent == null)
                    {
                        rootModCTIParent.Items.Add(tviChild);
                        tviChild.IsExpanded = true;
                    }
                    else
                    {
                        modCTIParent.Items.Add(tviChild);
                    }

                    LoadYamlCode(modClassParent[i].child, tviChild);
                }
                
            }

        }
    }
}
