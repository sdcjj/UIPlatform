using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Forms;
using System.IO;

namespace UIPlatform
{
    /// <summary>
    /// 封装组件
    /// </summary>
    public interface ISolutionItem
    {
        bool Save(string basePath);
        XElement ConvertToXElement();
        TreeNode ConvertToTreeNode();
        XElement GetConfig();
     }

    public abstract class SolutionItem : ISolutionItem
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Extension { get; protected set; }
        public int ImageIndex { get; set; }
        /// <summary>
        /// 是否有子节点，用于判断此节点的右键菜单里是否有添加选项
        /// </summary>
        public bool HasSubItem { get; protected set; }
        public XElement ConfigDoc { get; protected set; }

        public List<SolutionItem> Items { get; set; }

        public SolutionItem Parent { get; set; }

        public virtual void Add(SolutionItem item)
        {
            if (HasSubItem)
            {
                ConfigDoc.Add(item.ConvertToXElement());
                Items.Add(item);
            }
        }

        public virtual void AddRange(List<SolutionItem> items)
        {
            if (items != null)
            {
                items.ForEach(p =>
                {
                    ConfigDoc.Add(p.ConvertToXElement());
                });
                Items.AddRange(items);
            }
        }
        
        public virtual string GetFullPath()
        {
            return Path + "\\" + Name + Extension;
        }

        public virtual string GetRelativeDirectory()
        {
            string result = Name;
            SolutionItem cc = this;
            while (cc.Parent != null)
            {
                result = cc.Parent.Name + "\\" + result;
                cc = cc.Parent;
            }
            return result;
        }

        public virtual string GetRelativePath()
        {
            return Path + "\\" + Name + Extension;
        }

        public virtual bool Save(string basePath)
        {
            try
            {
                if (ConfigDoc != null)
                    ConfigDoc.Save(basePath + "\\" + GetRelativePath());
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public virtual XElement ConvertToXElement() 
        {
            return new XElement(
                this.GetType().FullName,
                new XAttribute("Id", Id),
                new XAttribute("Name", Name),
                new XAttribute("Icon", ImageIndex),
                new XAttribute("Path", Path));
        }

        public virtual XElement GetConfig() 
        {
            return ConvertToXElement();
        }

        public virtual TreeNode ConvertToTreeNode()
        {
            return new TreeNode(Name + Extension, ImageIndex, ImageIndex) { Tag = this };
        }

        public bool SaveConfig(string basePath)
        {
            XElement result = GetConfig();
            string dir = basePath + Path;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            result.Save(basePath + "\\" + GetRelativePath());
            return true;
        }
    }

    public class RootSolutionItem : SolutionItem
    {
        public RootSolutionItem(string id, string path, string name, int imageIndex = 3)
        {
            Id = id;
            Extension = ".hcsln";
            HasSubItem = true;
            Path = "";
            Name = name;
            ImageIndex = imageIndex;
            Items = new List<SolutionItem>();
            ConfigDoc = ConvertToXElement();
        }

        public override TreeNode ConvertToTreeNode()
        {
            return new TreeNode(Name, ImageIndex, ImageIndex) { Tag = this };
        }
    }

    public class ProjectSolutionItem : SolutionItem
    {
        public ProjectSolutionItem(string id, string path, string name, int imageIndex = 4)
        {
            Id = id;
            Path = path;
            ImageIndex = imageIndex;
            Extension = ".hcproj";
            HasSubItem = true;
            if (name.EndsWith(".hcproj"))
            {
                Name = name.Substring(0, name.Length - ".hcproj".Length);
            }
            else
            {
                Name = name;
            }
            Items = new List<SolutionItem>();
            ConfigDoc = ConvertToXElement();
        }

        public override string GetRelativePath()
        {
            return Path + "\\" +Name+  Extension;
        }

        public override XElement GetConfig()
        {
            XElement result = ConvertToXElement();
            if (Items != null)
            {
                Items.ForEach(p => 
                {
                    result.Add(p.ConvertToXElement());
                });
            }
            return result;
        }
    }

    public class FormSolutionItem : SolutionItem
    {
        public FormSolutionItem(string id, string path, string name, int imageIndex = 11)
        {
            Id = id;
            Extension = ".hcfm";
            Path = path;
            Name = name;
            ImageIndex = imageIndex;
            HasSubItem = false;
            ConfigDoc = ConvertToXElement();
            FormCodeItem = new FormCodeSolutionItem(Guid.NewGuid().ToString(), path, name);
            DesignCodeItem = new DesignCodeSolutionItem(Guid.NewGuid().ToString(), path, name);
        }

        public FormCodeSolutionItem FormCodeItem { get; set; }
        public DesignCodeSolutionItem DesignCodeItem { get; set; }


        public override XElement ConvertToXElement()
        {
                return new XElement(
                                   this.GetType().FullName,
                                   new XAttribute("Id", Guid.NewGuid().ToString()),
                                   new XAttribute("Name", Name),
                                   new XAttribute("Icon", ImageIndex),
                                   new XAttribute("Path", Path));
        }

        public override XElement GetConfig()
        {
            XElement dd= FormCodeItem.GetConfig();
            XElement dd2= DesignCodeItem.GetConfig();
            return new XElement(
                      this.GetType().FullName,
                      new XAttribute("Id", Guid.NewGuid().ToString()),
                      new XAttribute("Name", Name),
                      new XAttribute("Icon", ImageIndex),
                      new XAttribute("Path", Path),
                     FormCodeItem.GetConfig(),
                      DesignCodeItem.GetConfig());
        }

        public override TreeNode ConvertToTreeNode()
        {
            TreeNode result = base.ConvertToTreeNode();
            result.Nodes.Add(FormCodeItem.ConvertToTreeNode());
            result.Nodes.Add(DesignCodeItem.ConvertToTreeNode());
            return result;
        }
    }

    public class FormCodeSolutionItem : SolutionItem
    {
        public FormCodeSolutionItem(string id, string path, string name, int imageIndex = 13)
        {
            Id = id;
            Extension = ".hcxml";
            ImageIndex = imageIndex;
            HasSubItem = false;
            Path = path;
            if (name.EndsWith(".hcxml"))
            {
                Name = name.Substring(0, name.Length - ".hcxml".Length);
            }
            else
            {
                Name = name;
            }
            ConfigDoc = ConvertToXElement();
        }
    }

    public class DesignCodeSolutionItem : SolutionItem
    {
        public DesignCodeSolutionItem(string id, string path, string name, int imageIndex = 13)
        {
            Id = id;
            Extension = ".desxml";
            ImageIndex = imageIndex;
            HasSubItem = false;
            Path = path;
            if (name.EndsWith(".desxml"))
            {
                Name = name.Substring(0, name.Length - ".desxml".Length);
            }
            else
            {
                Name = name;
            }
            ConfigDoc =ConvertToXElement();
        }
    }

    public class SolutionItemFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="args">path/name/imageindex</param>
        /// <returns></returns>
        public static ISolutionItem CreateSolutionItem(string typeName,object[] args)
        {
            object ob = SystemHelper.CreateInstance(typeName, args);
            return ob as ISolutionItem;
        }
    }
}
