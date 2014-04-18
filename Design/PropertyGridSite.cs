using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace UIPlatform
{
    public class PropertyGridSite : ISite
    {
        private IServiceProvider sp;
        private IComponent component;

        public PropertyGridSite(IServiceProvider sp, IComponent component)
        {
            this.sp = sp;
            this.component = component;
        }
        #region Implementation of ISite

        public System.ComponentModel.IComponent Component
        {
            get
            {
                return component;
            }
        }

        public System.ComponentModel.IContainer Container
        {
            get
            {
                return null;
            }
        }

        public bool DesignMode
        {
            get
            {
                return false;
            }
        }

        public string Name
        {
            get
            {
                return null;
            }
            set { }
        }

        #endregion

        #region Implementation of IServiceProvider

        public object GetService(Type serviceType)
        {
            if (sp != null)
            {
                return sp.GetService(serviceType);
            }
            return null;
        }

        #endregion
    }
}
