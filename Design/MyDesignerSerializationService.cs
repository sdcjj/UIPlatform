using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design.Serialization;

namespace UIPlatform
{
    /// <summary>
    /// 支持复制粘贴
    /// </summary>
    public class MyDesignerSerializationService:IDesignerSerializationService
    {
        private IServiceProvider serviceProvider;
        private ComponentSerializationService serializer;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sp"></param>
        public MyDesignerSerializationService(IServiceProvider sp)
        {
            serviceProvider = sp;
            serializer = serviceProvider.GetService(typeof(ComponentSerializationService)) as ComponentSerializationService;
        }

        public System.Collections.ICollection Deserialize(object serializationData)
        {
            if (serializer != null && serializationData is SerializationStore)
            {
                return serializer.Deserialize((SerializationStore)serializationData);
            }
            return null;
        }

        public object Serialize(System.Collections.ICollection objects)
        {
            if (objects == null)
            {
                objects = new object[0];
            }
            if (serializer != null)
            {
                SerializationStore store = serializer.CreateStore();
                using (store)
                {
                    foreach (object obj in objects)
                    {
                        serializer.Serialize(store, obj);
                    }
                }
                return store;
            }
            return null;
        }
    }
}
