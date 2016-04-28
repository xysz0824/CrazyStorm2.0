/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public class PropertyContainer : ICloneable
    {
        IDictionary<PropertyInfo, PropertyValue> properties;
        public IDictionary<PropertyInfo, PropertyValue> Properties { get { return properties; } }
        public PropertyContainer()
        {
            properties = new Dictionary<PropertyInfo, PropertyValue>();
        }
        public IList<PropertyInfo> InitializeProperties(Type type)
        {
            var propertiesInfo = new List<PropertyInfo>();
            foreach (var property in type.GetProperties())
            {
                if (property.DeclaringType.Name != type.Name)
                    continue;

                object[] attributes = property.GetCustomAttributes(false);
                if (attributes.Length > 0 && attributes[0] is PropertyAttribute)
                {
                    propertiesInfo.Add(property);
                    if (!properties.ContainsKey(property))
                    {
                        var value = new PropertyValue { Value = property.GetGetMethod().Invoke(this, null).ToString() };
                        properties[property] = value;
                    }
                }
            }
            return propertiesInfo;
        }
        public virtual object Clone()
        {
            var clone = MemberwiseClone() as PropertyContainer;
            clone.properties = new Dictionary<PropertyInfo, PropertyValue>();
            foreach (var pair in properties)
                clone.properties[pair.Key] = pair.Value.Clone() as PropertyValue;
            return clone;
        }
        public XmlElement GetXmlElement(XmlDocument doc)
        {
            var propertiesNode = doc.CreateElement("Properties");
            foreach (var pair in Properties)
            {
                if (pair.Value.Expression)
                {
                    var pairNode = doc.CreateElement("Dictionary");
                    var keyAttribute = doc.CreateAttribute("Key");
                    keyAttribute.Value = pair.Key.Name;
                    pairNode.Attributes.Append(keyAttribute);
                    var valueAttribute = doc.CreateAttribute("Value");
                    valueAttribute.Value = pair.Value.Value;
                    pairNode.Attributes.Append(valueAttribute);
                    propertiesNode.AppendChild(pairNode);
                }
            }
            return propertiesNode;
        }
    }
}
