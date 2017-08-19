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
using System.IO;

namespace CrazyStorm.Core
{
    public abstract class PropertyContainer : ICloneable
    {
        IDictionary<string, PropertyValue> properties;
        public IDictionary<string, PropertyValue> Properties { get { return properties; } }
        IDictionary<string, VMInstruction[]> propertyExpressions;
        public IDictionary<string, VMInstruction[]> PropertyExpressions { get { return propertyExpressions; } }
        public PropertyContainer()
        {
            properties = new Dictionary<string, PropertyValue>();
            propertyExpressions = new Dictionary<string, VMInstruction[]>();
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
        public void LoadPropertyExpressions(BinaryReader reader)
        {
            using (BinaryReader listReader = PlayDataHelper.GetBlockReader(reader))
            {
                while (!PlayDataHelper.EndOfReader(listReader))
                {
                    using (BinaryReader expressionReader = PlayDataHelper.GetBlockReader(listReader))
                    {
                        string propertyName = PlayDataHelper.ReadString(expressionReader);
                        int bytesLength = (int)expressionReader.BaseStream.Length - propertyName.Length - 1;
                        propertyExpressions[propertyName] = VM.Decode(expressionReader.ReadBytes(bytesLength));
                    }
                }
            }
        }
        public void ExecuteExpressions()
        {
            foreach (var expression in PropertyExpressions)
            {
                VM.Execute(this, expression.Value);
                SetProperty(expression.Key);
            }
        }
        public void ExecuteExpression(string propertyName)
        {
            if (!PropertyExpressions.ContainsKey(propertyName))
                return;

            VM.Execute(this, PropertyExpressions[propertyName]);
            SetProperty(propertyName);
        }
        public abstract bool PushProperty(string propertyName);
        public abstract bool SetProperty(string propertyName);
    }
}
