using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars.Docking2010.Views;
using DevExpress.XtraBars;
using System.ComponentModel.Design;
using System.Reflection;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using DevExpress.XtraBars.Docking2010.Views.Tabbed;
using DevExpress.XtraEditors;

namespace UIPlatform
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            SystemHelper.BarItemClick += new EventHandler<ItemClickEventArgs>(Menu_BarItemClick);
            SystemHelper.LoadDesignForm += new GenericEventHandler<ISolutionItem>(SystemHelper_LoadDesignForm);
            tabbedView1.DocumentClosing += new DocumentCancelEventHandler(tabbedView1_DocumentClosing);
            InitMenu();
            InitDockLayout();
        }

        #region 公共变量

        UCSolutionExplorer SolutionExplorer;
        DockPanel RightContainer;
        DockPanel BottomContainer;
        DockPanel LeftContainer;
        MyPropertyGrid PropertyItem;
        MyToolboxService ToolboxService;

        #endregion

        //加载菜单
        void InitMenu()
        {
            string MenuFilePath = SystemHelper.BasePath + "Configs//MenuConfig.xml";
            SystemHelper.InitMenuInfo(barManager1, barTopMenu, SystemHelper.LoadXml(MenuFilePath));
            
        }

        //加载功能界面
        void InitDockLayout()
        {
            #region 初始化容器
            RightContainer = new DockPanel();
            BottomContainer = new DockPanel();
            LeftContainer = new DockPanel();

            LeftContainer.Dock = DockingStyle.Left;
            LeftContainer.Tabbed = true;
            RightContainer.Dock = DockingStyle.Right;
            RightContainer.Tabbed = true;
            BottomContainer.Dock = DockingStyle.Bottom;
            BottomContainer.Tabbed = true;

            string LayoutFilePath = SystemHelper.BasePath + "Configs//LayoutConfig.xml";
            XDocument xDocument = SystemHelper.LoadXml(LayoutFilePath);
            #endregion

            #region 工具箱管理器


            ToolboxService = new MyToolboxService();
            List<MyToolItem> items = new List<MyToolItem>();
            xDocument.Root.Element("LeftPanel").Element("ToolPanel").Elements("Tools").ToList().ForEach(p =>
            {
                p.Elements("Tool").ToList().ForEach(item =>
                {
                    Type type = SystemHelper.getAssemblyType(item.Attribute("Dll").Value, item.Attribute("ClassName").Value);
                    if (type != null)
                    {
                        MyToolItem toolItem = new MyToolItem(p.Attribute("Name").Value, type, item.Attribute("Dll").Value, item.Attribute("ClassName").Value);
                        items.Add(toolItem);
                    }
                });
            });
            ToolboxService.Init(items);

            SystemHelper.AddFloatDockPanel(LeftContainer, "工具箱", "工具", true).Controls.Add(ToolboxService.ControlView);

            #endregion

            #region 数据源管理器
            SystemHelper.AddFloatDockPanel(LeftContainer, "数据源", "数据源", false);
            #endregion

            #region 解决方案管理器
            DockPanel solutionPanel = SystemHelper.AddFloatDockPanel(RightContainer, "解决方案管理器", "解决方案", true);
            SolutionExplorer = new UCSolutionExplorer();
            SolutionExplorer.Dock = DockStyle.Fill;
            solutionPanel.Resize += (sender, e) =>
            {
                SolutionExplorer.Width = solutionPanel.Width;//确保子控件的宽度和Panel一致
                SolutionExplorer.Height = solutionPanel.Height;
            };
            solutionPanel.Controls.Add(SolutionExplorer);
            #endregion

            #region 属性管理器
            DockPanel propertyPanel = SystemHelper.AddFloatDockPanel(RightContainer, "属性管理器", "属性",false );
            PropertyItem = new MyPropertyGrid();
            PropertyItem.Dock = DockStyle.Fill;
            propertyPanel.Resize += (sender, e) =>
            {
                PropertyItem.Width = propertyPanel.Width;//确保子控件的宽度和Panel一致
                PropertyItem.Height = propertyPanel.Height;
            };

            propertyPanel.Controls.Add(PropertyItem);
            #endregion

            SystemHelper.AddFloatDockPanel(BottomContainer, "错误列表", "错误列表", true).Visibility = DockVisibility.AutoHide;
            SystemHelper.AddFloatDockPanel(BottomContainer, "查找结果", "查找", false).Visibility = DockVisibility.AutoHide;
            SystemHelper.AddFloatDockPanel(BottomContainer, "实时调试", "调试", false).Visibility = DockVisibility.AutoHide;
            //BottomContainer.Tabbed = false;

            //将容器添加到主窗体中
            dockManager1.RootPanels.AddRange(new DockPanel[] { LeftContainer, RightContainer, BottomContainer});
        }
        #region 菜单命令解析
        void Menu_BarItemClick(object sender, ItemClickEventArgs e)
        {
            CommandID commandId = null;

            if (e.Item.Name == "001.004")
            {
                SaveFileDialog op = new SaveFileDialog() { Filter = "xml文件|*.xml", Title = "保存配置" };
                if (op.ShowDialog() == DialogResult.OK) { }
            }
            else if (e.Item.Name == "001.001.002")
            {
                FormNewItem form = new FormNewItem(SolutionExplorer.SelectItem);

                if (form.ShowDialog() == DialogResult.OK )
                {
                    if (!string.IsNullOrEmpty(form.BasePath))
                    {
                        //更新项目根目录
                        UCSolutionExplorer.BasePath = form.BasePath;
                    }
                    SystemHelper.UpdateSolutionExplorer(SolutionExplorer.SelectItem, form.SubSolutionItem);
                    if(form.ControlItem != null)
                        AddTabDocument(form.ControlItem, form.ItemName);
                }
            }
            else if (e.Item.Name == "001.001.003")
            {
                FormNewItem form = new FormNewItem(null);
                if (form.ShowDialog() == DialogResult.OK)
                { 
                    //更新项目根目录
                    UCSolutionExplorer.BasePath = form.BasePath;
                    SystemHelper.UpdateSolutionExplorer(SolutionExplorer.SelectItem, form.SubSolutionItem);
                }
            }
            else if (e.Item.Name == "001.002")
            {
                OpenFileDialog dialog = new OpenFileDialog() { Filter = "hcsln文件|*.hcsln", Title = "打开项目" };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SystemHelper.OpenSolution(dialog.FileName);
                }
            }
            else if (e.Item.Name == "001.003")
            {

            }
            #region 编辑菜单功能
            else if (e.Item.Name == "003.001")
                commandId = StandardCommands.Undo;
            else if (e.Item.Name == "003.002")
                commandId = StandardCommands.Redo;
            else if (e.Item.Name == "003.003")
                commandId = StandardCommands.Copy;
            else if (e.Item.Name == "003.004")
                commandId = StandardCommands.Paste;
            else if (e.Item.Name == "003.005")
                commandId = StandardCommands.Cut;
            else if (e.Item.Name == "003.006")
                commandId = StandardCommands.Delete;
            else if (e.Item.Name == "003.007")
                commandId = StandardCommands.SelectAll;
            #endregion
            #region 布局菜单功能
            else if (e.Item.Name == "004.001")
                ExecCommand("Save");
            else if (e.Item.Name == "004.002") ;
            else if (e.Item.Name == "004.003")
                commandId = StandardCommands.AlignLeft;
            else if (e.Item.Name == "004.004")
                commandId = StandardCommands.AlignRight;
            else if (e.Item.Name == "004.005")
                commandId = StandardCommands.AlignVerticalCenters;
            else if (e.Item.Name == "004.006")
                commandId = StandardCommands.AlignTop;
            else if (e.Item.Name == "004.007")
                commandId = StandardCommands.AlignBottom;
            else if (e.Item.Name == "004.008")
                commandId = StandardCommands.AlignHorizontalCenters;
            else if (e.Item.Name == "004.009")
                commandId = StandardCommands.SizeToControlWidth;
            else if (e.Item.Name == "004.010")
                commandId = StandardCommands.SizeToControlHeight;
            else if (e.Item.Name == "004.011")
                commandId = StandardCommands.SizeToControl;
            #endregion
            ExecCommand(commandId);
        }



        private void ExecCommand(CommandID commandId)
        {
            BaseDocument baseDocument = tabbedView1.Controller.View.ActiveDocument;
            if (commandId != null && baseDocument != null)
                ((UCControlItem)baseDocument.Control).GlobalInvoke(commandId);
        }

        private void ExecCommand(string commandStr)
        {
            BaseDocument baseDocument = tabbedView1.Controller.View.ActiveDocument;
            if (commandStr != null && baseDocument != null)
                ((UCControlItem)baseDocument.Control).GlobalInvoke(commandStr);
        }
        #endregion

        #region 配置文件解析
        Form LoadTestForm(XDocument doc)
        {
            Form result = new Form();
            result.WindowState = FormWindowState.Maximized;
            if (doc != null)
            {
                doc.Root.Elements("Control").ToList().ForEach(p =>
                {
                    Control item = (Control)SystemHelper.CreateInstance(p.Attribute("Class").Value);
                    item.Name = p.Attribute("Name").Value;
                    item.Width = int.Parse(p.Attribute("Width").Value);
                    item.Height = int.Parse(p.Attribute("Height").Value);
                    item.Text = p.Attribute("Text").Value;
                    item.Location = new Point(int.Parse(p.Attribute("X").Value), int.Parse(p.Attribute("Y").Value));
                    result.Controls.Add(item);
                });
            }
            return result;
        }

        private void AddTabDocument(UCControlItem controlItem, string name)
        {
            controlItem.SavedChange += new GenericEventHandler<UCControlItem, bool>(controlItem_SavedChange);
            controlItem.Surface.RegisterService(typeof(IToolboxService), ToolboxService);//注册工具箱服务

            controlItem.Surface.RegisterService(typeof(MyPropertyGrid), PropertyItem);//注册属性服务
            PropertyItem.Site = new PropertyGridSite(controlItem.Surface.getServiceProvider(), this);
            BaseDocument doc= tabbedView1.Controller.AddDocument(controlItem);
            doc.Caption=name;
        }

        void controlItem_SavedChange(UCControlItem sender, bool e)
        {
            //改变标题状态（加*和不加*）
            tabbedView1.DocumentGroups.ToList<DocumentGroup>().ForEach(p =>
            {
                foreach (var item in p.Items)
                {
                    if ((item.Control as UCControlItem).SolutionControl.Id == sender.SolutionControl.Id)
                    {
                        item.Caption = sender.SolutionControl.Name + (e ? "" : " *");
                    }
                }
            });
        }

        void tabbedView1_DocumentClosing(object sender, DocumentCancelEventArgs e)
        {
            UCControlItem control = (UCControlItem)e.Document.Control;
            if (!control.IsSaved)
            {
                if (MessageBox.Show("是否保存更改？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    control.Save();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
        
        void SystemHelper_LoadDesignForm(ISolutionItem e)
        {
            if (e is SolutionItem)
            {
                UCControlItem item = UCControlItem.InitBySolutionItem(e as SolutionItem,false,false);
                AddTabDocument(item, item.Name);
            }
        }
        #endregion

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Shift && !e.Alt && !e.Control)
            {
                if (e.KeyCode == Keys.Delete)
                    ExecCommand(StandardCommands.Delete);
                if (e.KeyCode == Keys.Up)
                    ExecCommand(MenuCommands.KeyMoveUp);
                if (e.KeyCode == Keys.Down)
                    ExecCommand(MenuCommands.KeyMoveDown);
                if (e.KeyCode == Keys.Right)
                    ExecCommand(MenuCommands.KeyMoveRight);
                if (e.KeyCode == Keys.Left)
                    ExecCommand(MenuCommands.KeyMoveLeft);
            }
            else if (e.Control)
            {
                if (e.KeyCode == Keys.C)
                    ExecCommand(StandardCommands.Copy);
                else if (e.KeyCode == Keys.X)
                    ExecCommand(StandardCommands.Cut);
                else if (e.KeyCode == Keys.V)
                    ExecCommand(StandardCommands.Paste);
                else if (e.KeyCode == Keys.Z)
                    ExecCommand(StandardCommands.Undo);
                else if (e.KeyCode == Keys.Y)
                    ExecCommand(StandardCommands.Redo);

            }
            else if (e.Shift)
            {

            }
            else if (e.Alt)
            {

            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            BottomContainer.Visibility = DockVisibility.AutoHide;
        }
    }
}
