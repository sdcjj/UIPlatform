using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.IO;

namespace UIPlatform
{
    public partial class UCControlItem : UserControl
    {
        public UCControlItem(SolutionItem item)
        {
            InitializeComponent();
            SystemHelper.SolutionItemAdded += new GenericEventHandler<SolutionItem, SolutionItem>(SystemHelper_SolutionItemAdded);
            SolutionControl = item;
            saved = false;
        }
        void SystemHelper_SolutionItemAdded(SolutionItem parentItem, SolutionItem e)
        {
            
        }

        public event GenericEventHandler<UCControlItem, bool> SavedChange;
        XDocument FormXMLDoc;
        XDocument DesignXMLDoc;

        public SolutionItem SolutionControl { get; set; }
        private bool saved;
        public bool IsSaved
        {
            get
            {
                return saved;
            }
            set
            {
                if (SavedChange != null)
                    SavedChange(this,value);
                saved = value;
            }
        }

        
        public MyDesignSurface Surface { get; private set; }
        
        public void Init(bool isNew,bool isSave)
        {
            //if (SolutionControl.ConfigDoc == null)
            if (!isNew)
            {
                FormSolutionItem formSolution = SolutionControl as FormSolutionItem;
                //SolutionControl.SetConfigDoc(XDocument.Load(SolutionControl.Path + "\\" + SolutionControl.Name), false);
                FormXMLDoc = XDocument.Load(UCSolutionExplorer.BasePath+ formSolution.FormCodeItem.GetRelativePath());
                DesignXMLDoc = XDocument.Load(UCSolutionExplorer.BasePath + formSolution.DesignCodeItem.GetRelativePath());
                Surface = new MyDesignSurface();
                Surface.ItemComponentChanged += new GenericEventHandler<ComponentChangedEventArgs>(Surface_ItemComponentChanged);
                Surface.ItemComponentAdded += new GenericEventHandler<ComponentEventArgs>(Surface_ItemComponentAdded);
                Surface.ItemComponentRemoved += new GenericEventHandler<ComponentEventArgs>(Surface_ItemComponentRemoved);
                this.Name = SolutionControl.Name;
                tpDesign.Text = SolutionControl.Name + "[视图设计]";
                tpXML.Text = SolutionControl.Name + "[表单代码]";
                tpDesignCode.Text = SolutionControl.Name + "[设计器代码]";

                Surface.CreateControl(DesignXMLDoc);

                Control view = (Control)Surface.View;
                view.Dock = DockStyle.Fill;
                tpDesign.Controls.Clear();
                tpDesign.Controls.Add(view);
                BindCode();
            }
            else
            {
                FormXMLDoc = new XDocument(new XElement("Object"));
                DesignXMLDoc = new XDocument(new XElement("Object"));
                Surface = new MyDesignSurface();
                Surface.ItemComponentChanged += new GenericEventHandler<ComponentChangedEventArgs>(Surface_ItemComponentChanged);
                Surface.ItemComponentAdded += new GenericEventHandler<ComponentEventArgs>(Surface_ItemComponentAdded);
                Surface.ItemComponentRemoved += new GenericEventHandler<ComponentEventArgs>(Surface_ItemComponentRemoved);
                this.Name = SolutionControl.Name;
                tpDesign.Text = SolutionControl.Name + "[视图设计]";
                tpXML.Text = SolutionControl.Name + "[表单代码]";
                tpDesignCode.Text = SolutionControl.Name + "[设计器代码]";

                Surface.CreateRootComponent(typeof(Form));
                Control view = (Control)Surface.View;
                view.Dock = DockStyle.Fill;
                tpDesign.Controls.Clear();
                tpDesign.Controls.Add(view);
                BindCode();
            }
            if (isSave)
            {
                Save();
            }
        }

        /// <summary>
        /// 添加设计视图和配置视图
        /// </summary>
        /// <param name="s"></param>
        public static UCControlItem InitBySolutionItem(SolutionItem item, bool isNew, bool save)
        {
            UCControlItem controlItem = new UCControlItem(item);
            try
            {
                controlItem.Init(isNew, save);
            }
            catch (Exception ex)
            {

            }
            return controlItem;
        }

        void Surface_ItemComponentRemoved(ComponentEventArgs e)
        {
            IsSaved = false;
        }

        void Surface_ItemComponentAdded(ComponentEventArgs e)
        {
            IsSaved = false;
        }

        void Surface_ItemComponentChanged(ComponentChangedEventArgs e)
        {
            IsSaved = false;
            //XElement node = new XElement("Control",
            //    new XAttribute("Name", sender.Name),
            //    new XAttribute("Class", sender.GetType().FullName),
            //    new XAttribute("Width", sender.Width),
            //    new XAttribute("Height", sender.Height),
            //    new XAttribute("X", location.X),
            //    new XAttribute("Y", location.Y),
            //    new XAttribute("Text", sender.Text)
            //    );
            //doc.Root.Add(node);
            //control1.Text = doc.Document.ToString();
        }

        public void GlobalInvoke(CommandID command)
        {
            Surface.GlobalInvoke(command);
        }

        public void GlobalInvoke(string command)
        {
            if (command == "Save")
            {
                Save();
            }
        }

        public void BindCode()
        {
            txtDesignCode.Text = DesignXMLDoc.ToString();
            txXML.Text = FormXMLDoc.ToString();
        }

        private void tbMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tbMain.SelectedIndex == 0)
                {
 
                }
                else if (tbMain.SelectedIndex == 1)
                {
 
                }
                else if (tbMain.SelectedIndex == 2)
                {
                    txtDesignCode.Text = (DesignXMLDoc = Surface.GetDesignDoc()).ToString();
                }

            }
            catch (Exception ex)
            {

            }
        }

        internal void Save()
        {
            lock (SystemHelper.lockobject)
            {
                DesignXMLDoc = Surface.GetDesignDoc();
                FormXMLDoc.Save(UCSolutionExplorer.BasePath + (SolutionControl as FormSolutionItem).FormCodeItem.GetFullPath());
                DesignXMLDoc.Save(UCSolutionExplorer.BasePath + (SolutionControl as FormSolutionItem).DesignCodeItem.GetFullPath());
                IsSaved = true;
            }
        }
    }
}
