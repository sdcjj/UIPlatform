using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;

namespace UIPlatform
{
    public class MyEventBindingService : EventBindingService
    {
        public MyEventBindingService(IServiceProvider myhost) : base(myhost)
        {
        }

        protected override string CreateUniqueMethodName(System.ComponentModel.IComponent component, System.ComponentModel.EventDescriptor e)
        {
            return component.Site.Name + "_" + e.EventType.Name.Replace("Handler", "");//给事件绑定方法时
        }

        protected override System.Collections.ICollection GetCompatibleMethods(System.ComponentModel.EventDescriptor e)
        {
            return new List<object>();
        }

        protected override bool ShowCode(System.ComponentModel.IComponent component, System.ComponentModel.EventDescriptor e, string methodName)
        {
            return false;
        }

        protected override bool ShowCode(int lineNumber)
        {
            return false;
        }

        protected override bool ShowCode()
        {
            return false;
        }
    }
}
