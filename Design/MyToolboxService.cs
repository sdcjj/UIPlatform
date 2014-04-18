using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace UIPlatform
{
    /// <summary>
    /// 工具箱服务
    /// </summary>
    public class MyToolboxService : ToolboxService
    {
        public MyToolboxService()
        {
            ControlView = new TreeView();
            ControlView.Dock = DockStyle.Fill;
            ControlView.ImageList = images;
            ControlView.AllowDrop = true;
            ControlView.AfterSelect += new TreeViewEventHandler(ControlView_AfterSelect);
            images.Images.Clear();
            ToolItemList = new List<MyToolItem>();
        }

        void ControlView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ControlView.SelectedImageIndex = e.Node.ImageIndex;
            ControlView.ExpandAll();
        }

        public TreeView ControlView { get; private set; }
        ImageList images = new ImageList();
        List<MyToolItem> ToolItemList;

        public void Init(List<MyToolItem> toolItemList)
        {
            ToolItemList = toolItemList;
            foreach (var item in toolItemList.GroupBy(p => p.Category))
            {
                TreeNode node = new TreeNode();
                node.Text = item.Key;
                node.ImageIndex = 0;
                foreach (var control in item)
                {
                    images.Images.Add(control.Icon);
                    TreeNode subNode = new TreeNode();
                    subNode.ImageIndex = images.Images.Count - 1;
                    subNode.Name = control.Id;
                    subNode.Text = control.Name;
                    subNode.Tag = control;

                    control.UpdateNode(subNode);
                    node.Nodes.Add(subNode);
                }
                ControlView.Nodes.Add(node);
                ControlView.ExpandAll();
            }
        }

        

        /// <summary>
        /// 获取描述可用工具箱类别的字符串的集合
        /// </summary>
        protected override CategoryNameCollection CategoryNames
        {
            //get 
            //{
            //    return new CategoryNameCollection(getCategoryNames()); 
            //}

            get
            {
                return null;
            }
        }
        /// <summary>
        /// 获取所有种类
        /// </summary>
        /// <returns></returns>
        private string[] getCategoryNames()
        {
            List<string> result = new List<string>();
            var dd= ToolItemList.GroupBy(p => p.Category);
            foreach (var item in dd)
            {
                result.Add(item.Key);
            }
            return result.ToArray();
        }

        protected override IList GetItemContainers(string categoryName)
        {
            List<ToolboxItem> categoryItem = new List<ToolboxItem>();
            ToolItemList.Where(p => p.Category.Equals(categoryName)).ToList().ForEach(item => 
            {
                categoryItem.Add(item.BoxItem);
            });
            ToolboxItem[] result = new ToolboxItem[categoryItem.Count];
            categoryItem.CopyTo(result, 0);
            return result; 
        }

        protected override IList GetItemContainers()
        {
            var result = new MyToolItem[ToolItemList.Count];
            ToolItemList.CopyTo(result, 0);
            return result;
        }

        protected override void Refresh()
        {
            ControlView.Refresh();
        }

        protected override string SelectedCategory
        {
            //get
            //{
            //    return ((MoultonToolItem)ControlView.SelectedNode.Tag).Category;
            //}
            //set
            //{
            //    ControlView.SelectedNode = ToolItemList.FirstOrDefault(p => p.Category == value).ItemNode;
            //}
            get
            {
                return null;
            }
            set
            {

            }
        }

        protected override ToolboxItemContainer SelectedItemContainer
        {
            get
            {
                if (ControlView.SelectedNode != null)
                {
                    MyToolItem selectNode = ControlView.SelectedNode.Tag as MyToolItem;
                    if (selectNode != null)
                    {
                        return new ToolboxItemContainer(selectNode.BoxItem);
                    }
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    ControlView.SelectedNode = null;
                }
            }
        }
    }

    public class MyToolItem
    {
        public string Id { get; private set; }
        public string Dll { get; private set; }
        public string ClassName { get; private set; }
        public string Name { get; private set; }
        public string Category { get; private set; }
        public ToolboxItem BoxItem { get; private set; }
        public TreeNode ItemNode { get; private set; }
        public Image Icon { get; set; }

        public MyToolItem(string category, Type boxItemType,string dll,string className)
        {
            if (boxItemType == null)
                throw new NullReferenceException("存在没有识别的ToolItem类型。");
            Id = Guid.NewGuid().ToString();
            Name = boxItemType.Name;
            Dll = dll;
            ClassName = className;
            Category = category;
            Icon = getIcon(boxItemType);
            BoxItem = new ToolboxItem(boxItemType);
            
        }
        public MyToolItem(string category, ToolboxItem boxItem)
        {
            Id = Guid.NewGuid().ToString();
            Name = boxItem.ToString();
            Category = category;
            BoxItem = boxItem;
        }

        public MyToolItem(string name, string category, ToolboxItem boxItem)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Category = category;
            BoxItem = boxItem;
        }

        public void UpdateNode(TreeNode itemNode)
        {
            ItemNode = itemNode;
        }

        public Bitmap getIcon(Type type)
        {
            ToolboxBitmapAttribute tba = TypeDescriptor.GetAttributes(type)[typeof(ToolboxBitmapAttribute)] as ToolboxBitmapAttribute;
            if (tba != null)
            {
                return (Bitmap)tba.GetImage(type);
            }
            return null;
        }
    }
}
