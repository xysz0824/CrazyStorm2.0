/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public class PropertyContainer : ICloneable
    {
        IDictionary<string, PropertyValue> properties;
        public IDictionary<string, PropertyValue> Properties { get { return properties; } }
        public PropertyContainer()
        {
            properties = new Dictionary<string, PropertyValue>();
        }
        public List<PropertyInfo> InitializeAndGetProperties(Type type)
        {
            var propertiesInfo = new List<PropertyInfo>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.DeclaringType.Name != type.Name)
                    continue;

                object[] attributes = property.GetCustomAttributes(false);
                if (attributes.Length > 0 && attributes[0] is PropertyAttribute)
                {
                    propertiesInfo.Add(property);
                    if (!properties.ContainsKey(property.Name))
                    {
                        var value = new PropertyValue { Value = property.GetGetMethod().Invoke(this, null).ToString() };
                        properties[property.Name] = value;
                    }
                }
            }
            return propertiesInfo;
        }
        public List<PropertyInfo> GetProperties()
        {
            var propertiesInfo = new List<PropertyInfo>();
            foreach (PropertyInfo property in GetType().GetProperties())
            {
                object[] attributes = property.GetCustomAttributes(false);
                if (attributes.Length > 0 && attributes[0] is PropertyAttribute)
                    propertiesInfo.Add(property);
            }
            return propertiesInfo;
        }
        public virtual object Clone()
        {
            var clone = MemberwiseClone() as PropertyContainer;
            clone.properties = new Dictionary<string, PropertyValue>();
            foreach (var pair in properties)
                clone.properties[pair.Key] = pair.Value.Clone() as PropertyValue;
            return clone;
        }
        public void BuildFromXmlElement(XmlElement node)
        {
            var propertiesNode = node.SelectSingleNode("Properties");
            if (propertiesNode == null)
                throw new System.IO.FileLoadException("FileDataError");

            foreach (XmlElement childNode in propertiesNode.ChildNodes)
            {
                if (!childNode.HasAttribute("Key"))
                    throw new System.IO.FileLoadException("FileDataError");

                string key = childNode.GetAttribute("Key");
                if (!childNode.HasAttribute("Value"))
                    throw new System.IO.FileLoadException("FileDataError");

                string expression = childNode.GetAttribute("Value");
                PropertyInfo property = GetType().GetProperty(key);
                if (property == null)
                    throw new System.IO.FileLoadException("FileDataError");

                var value = new PropertyValue { Expression = true, Value = expression };
                properties[property.Name] = value;
            }
        }
        public XmlElement GetXmlElement(XmlDocument doc)
        {
            var propertiesNode = doc.CreateElement("Properties");
            foreach (var pair in properties)
            {
                if (pair.Value.Expression)
                {
                    var pairNode = doc.CreateElement("Dictionary");
                    var keyAttribute = doc.CreateAttribute("Key");
                    keyAttribute.Value = pair.Key;
                    pairNode.Attributes.Append(keyAttribute);
                    var valueAttribute = doc.CreateAttribute("Value");
                    valueAttribute.Value = pair.Value.Value;
                    pairNode.Attributes.Append(valueAttribute);
                    propertiesNode.AppendChild(pairNode);
                }
            }
            return propertiesNode;
        }
        public void GeneratePlayData(List<byte> data)
        {
            List<byte> newData = new List<byte>();
            foreach (var pair in properties)
            {
                if (pair.Value.Expression)
                {
                    List<byte> pairData = new List<byte>();
                    pairData.AddRange(PlayDataHelper.GetBytes(pair.Key));
                    pairData.AddRange(pair.Value.CompiledExpression);
                    newData.AddRange(PlayDataHelper.CreateBlock(pairData));
                }
            }
            data.AddRange(PlayDataHelper.CreateBlock(newData));
        }
    }
}
