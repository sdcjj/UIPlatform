using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using System.Xml.Linq;
using System.Globalization;

namespace UIPlatform
{
    public class MyDesignSurface : DesignSurface
    {
        public MyDesignSurface()
        {
            InitServices();
        }

        private MyUndoEngine _undoEngine;//支持撤销重做
        ISelectionService _itemSelectionService;//选中服务
        MyNameCreationService _nameCreationService;//自动名称服务
        CodeDomComponentSerializationService _codeDomComponentSerializationService;//支持Undo/Redo
        public event GenericEventHandler<ComponentEventArgs> ItemComponentAdded;
        public event GenericEventHandler<ComponentChangedEventArgs> ItemComponentChanged;
        public event GenericEventHandler<ComponentEventArgs> ItemComponentRemoved;

        private void InitServices()
        {
            _nameCreationService = new MyNameCreationService();//自动命名
            RegisterService(typeof(INameCreationService), _nameCreationService);

            RegisterService(typeof(IEventBindingService), new MyEventBindingService(this.ServiceContainer));

            _codeDomComponentSerializationService = new CodeDomComponentSerializationService(this.ServiceContainer);
            RegisterService(typeof(ComponentSerializationService), _codeDomComponentSerializationService);

            RegisterService(typeof(IMenuCommandService), new MenuCommandService(this));//添加菜单命令服务

            RegisterService(typeof(IDesignerSerializationService), new MyDesignerSerializationService(this.ServiceContainer));//支持复制粘贴

            _undoEngine = new MyUndoEngine(this.ServiceContainer);//撤销和重做
            _undoEngine.Enabled = true;
            RegisterService(typeof(UndoEngine), _undoEngine);

            #region 监听服务
            IComponentChangeService changeService = this.GetService(typeof(IComponentChangeService)) as IComponentChangeService;//初始化组件改变服务
            if (changeService != null)
            {
                changeService.ComponentAdded += new ComponentEventHandler(changeService_ComponentAdded);
                changeService.ComponentChanged += new ComponentChangedEventHandler(changeService_ComponentChanged);
                changeService.ComponentRemoved += new ComponentEventHandler(changeService_ComponentRemoved);
                changeService.ComponentRename += new ComponentRenameEventHandler(changeService_ComponentRename);
            }

            _itemSelectionService = this.GetService(typeof(ISelectionService)) as ISelectionService;//初始化服务容器
            if (_itemSelectionService != null)
            {
                _itemSelectionService.SelectionChanged += new EventHandler(ItemSelectionService_SelectionChanged);
            }

            #endregion

            UseGrid(new Size(10, 10));//绘制网格
        }

        #region 属性自动选择
        public void SelectItem(object ob)
        {
            MyPropertyGrid grid = (MyPropertyGrid)this.ServiceContainer.GetService(typeof(MyPropertyGrid));
            
            if (grid == null) return;
            IDesignerHost designerHost = (IDesignerHost)this.GetService(typeof(IDesignerHost));

            grid.SelectedObject = ob;

            if (designerHost != null)
            {
                grid.Site = (new IDEContainer(designerHost)).CreateSite(grid);
                grid.PropertyTabs.AddTabType(typeof(System.Windows.Forms.Design.EventsTab), PropertyTabScope.Document);
                grid.ShowEvents(true);
            }
            else
            {
                grid.Site = null;
            }
        }

        public void SelectItems(object[] objects)
        {
            //MyPropertyGrid grid = (MyPropertyGrid)this.ServiceContainer.GetService(typeof(MyPropertyGrid));
            //if (grid == null) return;
            //grid.SelectedObjects = objects;

            IDesignerHost designerHost = (IDesignerHost)this.GetService(typeof(IDesignerHost));
            MyPropertyGrid grid = (MyPropertyGrid)this.ServiceContainer.GetService(typeof(MyPropertyGrid));
            if (grid == null) return;
            if (objects == null)
                grid.SelectedObject = null;
            else
                grid.SelectedObjects = objects;

            if (designerHost != null)
            {
                grid.Site = (new IDEContainer(designerHost)).CreateSite(grid);
                grid.PropertyTabs.AddTabType(typeof(System.Windows.Forms.Design.EventsTab), PropertyTabScope.Document);
                grid.ShowEvents(true);
            }
            else
            {
                grid.Site = null;
            }
        }
        #endregion

        /// <summary>
        /// 重新注册服务
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="serviceInstance"></param>
        public void RegisterService(Type serviceType, object serviceInstance)
        {
            if (serviceInstance != null)
            {
                this.ServiceContainer.RemoveService(serviceType, false);
                this.ServiceContainer.AddService(serviceType, serviceInstance);
            }

        }

        public IServiceProvider getServiceProvider()
        {
            return this.ServiceContainer;
        }

        #region 监听改变事件

        void changeService_ComponentRename(object sender, ComponentRenameEventArgs e)
        {
            //if (ItemComponentChanged != null)
            //{
            //    ItemComponentChanged(e.Component);
            //}
        }

        void changeService_ComponentRemoved(object sender, ComponentEventArgs e)
        {
            if (ItemComponentRemoved != null)
            {
                ItemComponentRemoved(e);
            }
        }

        void changeService_ComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (ItemComponentChanged != null)
            {
                ItemComponentChanged(e);
            }
        }

        void changeService_ComponentAdded(object sender, ComponentEventArgs e)
        {
            object[] selection = { e.Component };
            SelectItems(selection);

            if (ItemComponentAdded != null)
            {
                ItemComponentAdded(e);
            }
        }

        #endregion

        #region 选择的控件发生改变时
        void ItemSelectionService_SelectionChanged(object sender, EventArgs e)
        {
            object[] selection;
            if (_itemSelectionService.SelectionCount == 0)
            {
                SelectItem(null);
            }
            else
            {
                selection = new object[_itemSelectionService.SelectionCount];
                _itemSelectionService.GetSelectedComponents().CopyTo(selection, 0);
                SelectItems(selection);
            }
        }

        #endregion

        //执行命令
        internal void GlobalInvoke(CommandID commandID)
        {
            if (string.IsNullOrEmpty(commandID.ToString())) return;

            IMenuCommandService ims = this.GetService(typeof(IMenuCommandService)) as IMenuCommandService;

            ims.GlobalInvoke(commandID);

            if (commandID.Equals(StandardCommands.Undo))
            {
                _undoEngine.Undo();
            }
            else if (commandID.Equals(StandardCommands.Redo))
            {
                _undoEngine.Redo();
            }
        }

        #region 设计界面网格风格

        public void UseSnapLines()
        {
            SetDesignerOptionStyle(new MyDesignerOptionService4SnapLines());
        }
        /// <summary>
        /// 自动对齐网格
        /// </summary>
        /// <param name="gridSize"></param>
        public void UseGrid(Size gridSize)
        {
            SetDesignerOptionStyle(new MyDesignerOptionService4Grid(gridSize));
        }
        /// <summary>
        /// 只绘制网格
        /// </summary>
        /// <param name="gridSize"></param>
        public void UseGridWithoutSnapping(Size gridSize)
        {
            SetDesignerOptionStyle(new MyDesignerOptionService4GridWithoutSnapping(gridSize));
        }
        /// <summary>
        /// 
        /// </summary>
        public void UseNoGuides()
        {
            SetDesignerOptionStyle(new MyDesignerOptionService4NoGuides());

        }

        private void SetDesignerOptionStyle(DesignerOptionService opsService2)
        {
            IServiceContainer serviceProvider = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
            DesignerOptionService opsService = serviceProvider.GetService(typeof(DesignerOptionService)) as DesignerOptionService;
            if (null != opsService)
            {
                serviceProvider.RemoveService(typeof(DesignerOptionService));
            }
            serviceProvider.AddService(typeof(DesignerOptionService), opsService2);
        }
        #endregion


        public Control CreateControl(XElement controlElenent, object parent)
        {
            IDesignerHost host = GetIDesignerHost();
            if (null == host) return null;
            if (null == parent) return null;
            IComponent newComp = host.CreateComponent(Type.GetType(controlElenent.Attribute("Type").Value));
            if (null == newComp) return null;
            IDesigner designer = host.GetDesigner(newComp);
            if (null == designer) return null;
            if (designer is IComponentInitializer)
                ((IComponentInitializer)designer).InitializeNewComponent(null);
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(newComp);

            if (controlElenent != null)
            {
                controlElenent.Elements("Property").ToList().ForEach(p =>
                {
                    PropertyDescriptor pdS = pdc.Find(p.Attribute("Name").Value, false);
                    object value;
                    value = TypeDescriptor.GetConverter(pdS.PropertyType).ConvertFromInvariantString(p.Value);
                    if (null != pdS)
                        pdS.SetValue(newComp, value);
                });

                controlElenent.Elements("Object").ToList().ForEach(p =>
                {
                    (newComp as Control).Controls.Add(CreateControl(p, newComp));
                });
            }
            ((Control)newComp).Parent = parent as Control;
            return newComp as Control;
        }


        public void CreateControl(XDocument controlDoc)
        {
            if (controlDoc == null) return ;
            IComponent root= CreateRootComponent(controlDoc.Root);
            controlDoc.Root.Elements("Object").ToList().ForEach(p => 
            {
                CreateControl(p, root);
            });
        }

        /// <summary>
        /// 创建根元素
        /// </summary>
        /// <param name="controlType"></param>
        /// <param name="controlSize"></param>
        /// <returns></returns>
        public IComponent CreateRootComponent(Type controlType)
        {
            IDesignerHost host = GetIDesignerHost();
            if (null == host) return null;
            if (null != host.RootComponent) return null;

            this.BeginLoad(controlType);
            if (this.LoadErrors.Count > 0)
                throw new Exception("加载 " + controlType.ToString() + " 失败");
            
            Control ctrl = null;
            Type hostType = host.RootComponent.GetType();
            if (hostType == typeof(Form))
            {
                ctrl = this.View as Control;
                //ctrl.BackColor = Color.LightGray;
                //PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(ctrl);
                //PropertyDescriptor pdS = pdc.Find("Size", false);
                //if (null != pdS)
                //    pdS.SetValue(host.RootComponent, controlSize);
            }
            else if (hostType == typeof(UserControl))
            {
                ctrl = this.View as Control;
                ctrl.BackColor = Color.DarkGray;
                //PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(ctrl);
                //PropertyDescriptor pdS = pdc.Find("Size", false);
                //if (null != pdS)
                //    pdS.SetValue(host.RootComponent, controlSize);
            }
            else if (hostType == typeof(Component))
            {
                ctrl = this.View as Control;
                ctrl.BackColor = Color.White;
            }
            else
            {
                throw new Exception("加载" + hostType.ToString() + "失败");
            }

            return host.RootComponent;
        }

        /// <summary>
        /// 创建根元素
        /// </summary>
        /// <param name="controlType"></param>
        /// <param name="controlSize"></param>
        /// <returns></returns>
        public IComponent CreateRootComponent(XElement rootElement)
        {
            IDesignerHost host = GetIDesignerHost();
            if (null == host) return null;
            if (null != host.RootComponent) return null;
            Type controlType =Type.GetType(rootElement.Attribute("Type").Value);
            this.BeginLoad(controlType);
            if (this.LoadErrors.Count > 0)
                throw new Exception("加载 " + controlType.ToString() + " 失败");

            Type hostType = host.RootComponent.GetType();
            Control ctrl = this.View as Control; 
            rootElement.Elements("Property").ToList().ForEach(p => 
            {
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(ctrl);
                PropertyDescriptor pdS = pdc.Find(p.Attribute("Name").Value, false);
                object value;
                value = TypeDescriptor.GetConverter(pdS.PropertyType).ConvertFromInvariantString(p.Value);
                if (null != pdS)
                    pdS.SetValue(host.RootComponent, value);
            });

            return host.RootComponent;
        }

        /// <summary>
        /// 获取设计器
        /// </summary>
        /// <returns></returns>
        public IDesignerHost GetIDesignerHost()
        {
            return (IDesignerHost)(this.GetService(typeof(IDesignerHost)));
        }


        internal void SetObjectToPropertyGrid(object[] c)
        {
            
        }

        /// <summary>
        /// 获取设计文档XML
        /// </summary>
        /// <returns></returns>
        public XDocument GetDesignDoc()
        {
            IDesignerHost host = GetIDesignerHost();

            IComponent root = host.RootComponent;
            string val="";
            if (root is Form)
            {
                val = (root as Form).ClientSize.Width + "," + (root as Form).ClientSize.Height;
            }
            else if(root is Control)
            {
                val = (root as Control).ClientSize.Width + "," + (root as Control).ClientSize.Height;
            }
            XElement rootEl = GetElementByComponent(root, "Object");
            rootEl.Add(new XElement("Property", new XAttribute("Name", "ClientSize")) { Value = val });
            XDocument result = new XDocument(rootEl);

            //XElement children = new XElement("Controls");
            //foreach (IComponent comp in host.Container.Components)
            //{

            //    if (comp != root && !((comp as Control).Parent is Form))
            //    {
            //        children.Add(GetElementByComponent(comp, "Object"));
            //    }
            //}
            //result.Root.Add(children);

            return result;
        }

        private static readonly Attribute[] propertyAttributes = new Attribute[] { DesignOnlyAttribute.No };

        /// <summary>
        /// 序列化组件
        /// </summary>
        /// <param name="component"></param>
        /// <param name="nodeName">节点名称</param>
        /// <returns></returns>
        protected XElement GetElementByComponent(IComponent component,string nodeName)
        {
            if (component == null) return null;
            XElement result = new XElement(nodeName);
            result.Add(new XAttribute("Type", component.GetType().AssemblyQualifiedName));

            if (component != null && component.Site != null && component.Site.Name != null)
            {
                result.Add(new XAttribute("Name", component.Site.Name));
            }

            foreach (Control child in ((Control)component).Controls)
            {
                if (child.Site != null)
                {
                    result.Add(GetElementByComponent(child, "Object"));
                }
            }

            PropertyDescriptorCollection properties = ClonePropertys(TypeDescriptor.GetProperties(component, propertyAttributes));
            foreach (PropertyDescriptor item in properties)
            {
                if (item.ShouldSerializeValue(component))
                {
                    DesignerSerializationVisibilityAttribute visibility = (DesignerSerializationVisibilityAttribute)item.Attributes[typeof(DesignerSerializationVisibilityAttribute)];
                    if (visibility == DesignerSerializationVisibilityAttribute.Visible && !item.IsReadOnly)//符合此条件才序列化
                    {
                        result.Add(new XElement("Property", new XAttribute("Name", item.Name)) { Value = GetPropertyValue(item, component) });
                    }
                    else if (visibility == DesignerSerializationVisibilityAttribute.Content && !item.IsReadOnly)//如果是内容
                    {
                        object propValue = item.GetValue(component);
                        if (typeof(IList).IsAssignableFrom(item.PropertyType))
                        {
                            foreach (object obj in (propValue as IList))
                            {
                                //XmlNode node = document.CreateElement("Item");
                                //XmlAttribute typeAttr = document.CreateAttribute("type");
                                //typeAttr.Value = obj.GetType().AssemblyQualifiedName;
                                //node.Attributes.Append(typeAttr);
                                //WriteValue(document, obj, node);
                                //parent.AppendChild(node);
                            }
                            //WriteCollection(document, (IList)propValue, node);
                        }
                        else
                        {
                            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(propValue, propertyAttributes);
                            //WriteProperties(document, props, propValue, node, elementName);
                        }
                        //if (node.ChildNodes.Count > 0)
                        //{
                        //    parent.AppendChild(node);
                        //}
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 克隆属性集合
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected PropertyDescriptorCollection ClonePropertys(PropertyDescriptorCollection properties)
        {
            PropertyDescriptor controlProp = properties["Controls"];
            PropertyDescriptor[] propArray = new PropertyDescriptor[properties.Count - 1];
            int idx = 0;
            foreach (PropertyDescriptor p in properties)
            {
                if (p != controlProp)
                {
                    propArray[idx++] = p;
                }
            }
            PropertyDescriptorCollection result = new PropertyDescriptorCollection(propArray);
            return result;
        }
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="property"></param>
        /// <param name="ob"></param>
        /// <returns></returns>
        protected string GetPropertyValue(PropertyDescriptor property,object ob)
        {
            string result = string.Empty;
            object propertyValue = property.GetValue(ob);
            if (propertyValue == null)
            {
                return result;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(propertyValue);

            if (converter.CanConvertFrom(typeof(string)) && converter.CanConvertTo(typeof(string)))
            {
                result = converter.ConvertTo(null, CultureInfo.InvariantCulture, propertyValue, typeof(string)) as string;
            }
            return result;
        }


        #region 不用

        /// <summary>
        /// 添加一个控件
        /// </summary>
        /// <param name="controlType"></param>
        /// <param name="controlSize"></param>
        /// <param name="controlLocation"></param>
        /// <returns></returns>
        public Control CreateControl(Type controlType, Size controlSize, Point controlLocation)
        {
            IDesignerHost host = GetIDesignerHost();
            if (null == host) return null;
            if (null == host.RootComponent) return null;
            IComponent newComp = host.CreateComponent(controlType);
            if (null == newComp) return null;
            IDesigner designer = host.GetDesigner(newComp);
            if (null == designer) return null;
            if (designer is IComponentInitializer)
                ((IComponentInitializer)designer).InitializeNewComponent(null);
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(newComp);

            PropertyDescriptor pdS = pdc.Find("Size", false);
            if (null != pdS)
                pdS.SetValue(newComp, controlSize);
            PropertyDescriptor pdL = pdc.Find("Location", false);
            if (null != pdL)
                pdL.SetValue(newComp, controlLocation);
            ((Control)newComp).Parent = host.RootComponent as Control;
            return newComp as Control;
        } 
        #endregion
    }
}
