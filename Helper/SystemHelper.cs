using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DevExpress.XtraBars;
using System.Drawing;
using System.Reflection;
using DevExpress.XtraBars.Docking;

namespace UIPlatform
{
    /// <summary>
    /// 泛型委托
    /// </summary>
    /// <typeparam name="Sender">事件触发体类</typeparam>
    /// <typeparam name="Event">事件内容类</typeparam>
    /// <param name="sender">触发体</param>
    /// <param name="e">内容</param>
    public delegate void GenericEventHandler<Sender, Event>(Sender sender, Event e);

    /// <summary>
    /// 泛型委托
    /// </summary>
    /// <typeparam name="Sender">事件触发体类</typeparam>
    /// <typeparam name="Event">事件内容类</typeparam>
    /// <param name="e">内容</param>
    public delegate void GenericEventHandler<Event>(Event e);


    class SystemHelper
    {
        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        public static event EventHandler<ItemClickEventArgs> BarItemClick;
        public static event GenericEventHandler<ISolutionItem> LoadDesignForm;
        public static event GenericEventHandler<object> CloseDesign;

        public static object lockobject = new object();

        /// <summary>
        /// 添加了文件或文件夹需要在解决方案里实时更新
        /// SolutionItem父节点
        /// SolutionItem添加的子项
        /// </summary>
        public static event GenericEventHandler<SolutionItem, SolutionItem> SolutionItemAdded;
        public static event GenericEventHandler<string> LoadSolution;
        #region 功能方法
        /// <summary>
        /// 根据文件路径加载xml
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static XDocument LoadXml(string path)
        {
            XDocument doc = null;
            try
            {
                doc = XDocument.Load(path);
            }
            catch (Exception ex)
            {

            }
            return doc;
        }


        /// <summary>
        /// 根据图片名称获取图片
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Bitmap getImageByFileName(string name)
        {
            Bitmap result = null;
            try
            {
                result = new Bitmap(BasePath + "Images\\" + name);
            }
            catch { }
            return result;
        }

        public static DockPanel AddFloatDockPanel(DockPanel parentPanel, string title, string iconPath, bool isActive)
        {
            DockPanel result = new DockPanel();
            result.Dock = DockingStyle.Fill;
            result.TabText = result.Text = title;
            result.Image = getImageByFileName(iconPath + ".png");
            if (isActive)
                parentPanel.ActiveChild = result;
            parentPanel.Controls.Add(result);
            return result;
        }
        #endregion

        #region 加载菜单
        /// <summary>
        /// 初始化菜单树
        /// </summary>
        /// <param name="barMenu"></param>
        /// <param name="MenuDoc"></param>
        public static void InitMenuInfo(BarManager barManager, Bar barMenu, XDocument MenuDoc)
        {
            if (MenuDoc == null) return;
            if (MenuDoc.Root.Name == "Menu")
            {
                barMenu.LinksPersistInfo.AddRange(GetMenuInfo(barManager, barMenu, MenuDoc.Root));
            }
        }
        /// <summary>
        /// 将xml转换成菜单树
        /// </summary>
        /// <param name="barMenu"></param>
        /// <param name="MenuElement"></param>
        /// <returns></returns>
        public static LinkPersistInfo[] GetMenuInfo(BarManager barManager, Bar barMenu, XElement MenuElement)
        {
            List<LinkPersistInfo> result = new List<LinkPersistInfo>();
            if (MenuElement.Name == "Menu" && MenuElement.HasElements)
            {
                MenuElement.Elements("MenuItem").ToList().ForEach(p =>
                {
                    result.AddRange(GetMenuInfo(barManager, barMenu, p));
                });
            }
            else if (MenuElement.Name == "MenuItem")
            {
                BarItem barItem;
                if (MenuElement.HasElements)
                {
                    barItem = new BarSubItem() { Caption = MenuElement.Attribute("Name").Value, Name = MenuElement.Attribute("Id").Value };
                }
                else
                {
                    barItem = new BarButtonItem() { Caption = MenuElement.Attribute("Name").Value, Name = MenuElement.Attribute("Id").Value };
                    barItem.Glyph = getImageByFileName(MenuElement.Attribute("Icon").Value + ".png");
                }
                bool isBeginGroup = MenuElement.Attribute("BeginGroup").Value == "0" ? false : true;
                barItem.Tag = MenuElement;
                LinkPersistInfo linkBar = new LinkPersistInfo(barItem, isBeginGroup);
                barManager.Items.Add(barItem);
                barItem.ItemClick += new ItemClickEventHandler(barItem_ItemClick);
                result.Add(linkBar);

                MenuElement.Elements("MenuItem").ToList().ForEach(p =>
                {
                    ((BarSubItem)barItem).LinksPersistInfo.AddRange(GetMenuInfo(barManager, barMenu, p));
                });
            }
            return result.ToArray();
        }

        #endregion

        #region 事件方法
        //菜单项点击统一管理
        public static void barItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (BarItemClick != null)
            {
                BarItemClick(sender, e);
            }
        }

        public static void UpdateSolutionExplorer(SolutionItem parent, SolutionItem e)
        {
            if (SolutionItemAdded != null && e != null)
            {
                SolutionItemAdded(parent, e);
            }
        }

        public static void OpenDesignView(ISolutionItem e)
        {
            if (LoadDesignForm != null&&e!=null)
            {
                LoadDesignForm(e);
            }
        }

        public static void OpenSolution(string path)
        {
            if (LoadSolution != null)
            {
                LoadSolution(path);
            }
        }

        public static void CloseDesignFun(string path)
        {
            if (CloseDesign != null)
            {
                CloseDesign(path);
            }
        }
        #endregion

        public static object CreateInstance(string className,object[] args=null)
        {
            if (!string.IsNullOrWhiteSpace(className))
            {
                Assembly[] assembly = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var item in assembly)
                {
                    if (item != null)
                    {
                        //object obj = item.CreateInstance(className);
                        object obj = item.CreateInstance(className, true, BindingFlags.CreateInstance, null, args, null, null);
                        if (obj != null)
                            return obj;
                    }
                }
            }
            return null;
        }

        public static Type getAssemblyType(string dllName, string className)
        {
            Type result = null;
            if (!string.IsNullOrWhiteSpace(className))
            {
                List<Assembly> assemblys = AppDomain.CurrentDomain.GetAssemblies().ToList();
                Assembly assembly = assemblys.FirstOrDefault(p => p.FullName.ToUpper().Equals((dllName + ".dll").ToUpper()));
                if (assembly != null)
                {
                    result= assembly.GetType(dllName + "." + className);
                }
                if(result==null)
                {
                    try
                    {
                        result= Assembly.LoadFrom(BasePath + dllName + ".dll").GetType(dllName + "." + className);
                    }
                    catch { }
                }
                if (result == null)
                {
                    try
                    {
                        result = Assembly.LoadFrom(BasePath + "Dlls//"+dllName + ".dll").GetType(dllName + "." + className);
                    }
                    catch { }
                }
            }
            return result;
        }
    }
}
