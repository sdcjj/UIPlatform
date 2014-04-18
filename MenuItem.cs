using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIPlatform
{
    public class MenuItem
    {
        public string Caption { get; set; }
        public string CommondName { get; set; }
    }

    public abstract class SystemCommond
    {
        public string Id { get; set; }
        
    }
}
