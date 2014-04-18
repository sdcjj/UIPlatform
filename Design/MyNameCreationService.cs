using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design.Serialization;

namespace UIPlatform
{
    /// <summary>
    /// 控件命名服务
    /// </summary>
    public class MyNameCreationService : INameCreationService
    {
        /// <summary>
        /// 自动生成默认名称
        /// </summary>
        /// <param name="container"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public string CreateName(System.ComponentModel.IContainer container, Type dataType)
        {
            IList<string> list = new List<string>();
            for (int i = 0; i < container.Components.Count; i++)
            {
                list.Add(container.Components[i].Site.Name);
            }
            return CreateNameByList(list, dataType.Name);
        }

        public bool IsValidName(string name)
        {
            return true;
        }

        public void ValidateName(string name)
        {

        }

        /// <summary>
        /// 创建一个基于baseName并且在array中不存在的名称
        /// </summary>
        public static string CreateNameByList(IList<string> list, string baseName)
        {
            int uniqueID = 1;
            bool unique = false;
            while (!unique)
            {
                unique = true;
                foreach (string s in list)
                {
                    if (s.StartsWith(baseName + uniqueID.ToString()))
                    {
                        unique = false;
                        uniqueID++;
                        break;
                    }
                }
            }
            return baseName + uniqueID.ToString();
        }
    }
}
