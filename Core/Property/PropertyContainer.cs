/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CrazyStorm.Core
{
    public abstract class PropertyContainer
    {
        Dictionary<PropertyInfo, PropertyValue> properties = new Dictionary<PropertyInfo, PropertyValue>();
        public Dictionary<PropertyInfo, PropertyValue> Properties { get { return properties; } }
        public IList<PropertyInfo> InitializeProperties(Type type)
        {
            var propertiesInfo = new List<PropertyInfo>();
            foreach (var property in type.GetProperties())
            {
                if (property.DeclaringType.Name != type.Name)
                    continue;
                object[] attributes = property.GetCustomAttributes(false);
                if (attributes.Length == 1 && attributes[0] is PropertyAttribute)
                {
                    propertiesInfo.Add(property);
                    if (!Properties.ContainsKey(property))
                    {
                        var value = new PropertyValue { Value = property.GetGetMethod().Invoke(this, null).ToString() };
                        Properties[property] = value;
                    }
                }
            }
            return propertiesInfo;
        }
    }
}
