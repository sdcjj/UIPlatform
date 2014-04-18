using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace UIPlatform
{
    public partial class UCSolutionExplorer : UserControl
    {
        public UCSolutionExplorer()
        {
            InitializeComponent();
            SystemHelper.SolutionItemAdded += new GenericEventHandler<SolutionItem, SolutionItem>(SystemHelper_SolutionItemAdded);
            SystemHelper.LoadSolution += new GenericEventHandler<string>(SystemHelper_LoadSolution);
            SystemHelper.CloseDesign += new GenericEventHandler<object>(SystemHelper_CloseDesign);
        }

        public static string BasePath;
        public SolutionItem RootItem { get; protected set; }
        public SolutionItem SelectItem = null;

        #region 销毁
        void SystemHelper_CloseDesign(object e)
        {
            DisposeSolution();
        }

        protected void DisposeSolution()
        {
            treeView1.Nodes.Clear();
            RootItem = null;
            SelectItem = null;
            BasePath = null;
        }
        #endregion

        void SystemHelper_LoadSolution(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            int fileNameIndex = path.LastIndexOf("\\");
            BasePath = path.Substring(0, fileNameIndex);
            XDocument doc = XDocument.Load(path);
            RootItem = getSolutionList(doc.Root, null, BasePath)[0];
            InitUI();
            treeView1.SelectedNode = treeView1.Nodes[0];
            SelectItem = RootItem = treeView1.Nodes[0].Tag as SolutionItem;
        }

        void SystemHelper_SolutionItemAdded(SolutionItem parentItem, SolutionItem element)
        {
            if (element == null) return;
            if (parentItem == null && element is RootSolutionItem)
            {
                RootItem = element;
            }
            else
            {
                if (SelectItem == null) return;
                SelectItem.Add(element);
                lock (SystemHelper.lockobject)
                {
                    element.SaveConfig(BasePath);

                    if (element.Parent != null && element.Parent.Parent != null)
                    {
                        element.Parent.SaveConfig(BasePath);
                    }
                }
            }
            RootItem.Save(BasePath);
            InitUI();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void InitUI()
        {
            treeView1.Nodes.Clear();
            treeView1.Nodes.AddRange(getSolutionTreeNodes(RootItem).ToArray());

            treeView1.ExpandAll();
        }

        /// <summary>
        /// 获取绑定数据
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="imageIndex"></param>
        /// <returns></returns>
        public List<TreeNode> getSolutionTreeNodes(SolutionItem item)
        {
            List<TreeNode> treeNodes = new List<TreeNode>();
            if (item != null)
            {
                TreeNode node = item.ConvertToTreeNode();
                if (item.Items != null)
                {
                    item.Items.ForEach(p =>
                    {
                        node.Nodes.AddRange(getSolutionTreeNodes(p).ToArray());
                    });
                }
                treeNodes.Add(node);
            }
            return treeNodes;
        }


        /// <summary>
        /// 获取绑定数据
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="imageIndex"></param>
        /// <returns></returns>
        public List<SolutionItem> getSolutionList(XElement xElement, SolutionItem par,string basePath)
        {
            List<SolutionItem> treeNodes = new List<SolutionItem>();
            if (xElement != null)
            {
                string type = xElement.Name.ToString();
                string id = xElement.Attribute("Id").Value;
                string name = xElement.Attribute("Name").Value;
                string path = xElement.Attribute("Path").Value;
                int imageIndex = -1;
                XAttribute attr = xElement.Attribute("Icon");
                if (attr != null)
                {
                    int.TryParse(attr.Value, out imageIndex);
                }
                if (string.IsNullOrEmpty(name.Trim()))
                    name = xElement.Name.ToString();
                SolutionItem item = SolutionItemFactory.CreateSolutionItem(type, new object[] { id, path, name, imageIndex }) as SolutionItem;
                item.Parent = par;
                if (item != null)
                {
                    if (xElement.HasElements)
                    {
                        xElement.Elements().ToList().ForEach(p =>
                        {
                            item.AddRange(getSolutionList(p, item, basePath));
                        });
                    }
                    else if (item is ProjectSolutionItem)
                    {
                        XDocument doc = XDocument.Load(basePath + "\\" + item.GetRelativePath());
                        doc.Root.Elements().ToList().ForEach(p =>
                        {
                            item.AddRange(getSolutionList(p, item, basePath));
                        });
                    }
                    treeNodes.Add(item);
                }
            }
            return treeNodes;
        }
        
        private void barRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!string.IsNullOrEmpty(BasePath) && RootItem != null)
            {
                SystemHelper_LoadSolution(BasePath + "\\"+RootItem.GetFullPath());
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectItem = e.Node.Tag as SolutionItem;
        }

        //双击打开设计视图
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            object selectNodeTag = e.Node.Tag;
            if (selectNodeTag is FormSolutionItem)
            {
                FormSolutionItem formItem = selectNodeTag as FormSolutionItem;
                SystemHelper.OpenDesignView(formItem);
            }
        }
    }
}
