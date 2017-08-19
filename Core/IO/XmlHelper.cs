/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

namespace CrazyStorm.Core
{
    class XmlHelper
    {
        public static void BuildFromFields(Type type, object source, XmlElement node)
        {
            if (node == null)
                return;

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            FieldInfo[] fieldInfos = type.GetFields(flags);
            foreach (var info in fieldInfos)
            {
                object[] attributes = info.GetCustomAttributes(false);
                for (int i = 0; i < attributes.Length; ++i)
                {
                    if (attributes[i] is XmlAttributeAttribute)
                    {
                        if (!node.HasAttribute(info.Name))
                            continue;

                        var text = node.GetAttribute(info.Name);
                        object value;
                        if (PropertyTypeRule.TryParse(info.GetValue(source), text, out value))
                            info.SetValue(source, value);
                        else
                            throw new System.IO.FileLoadException("FileDataError");

                        break;
                    }
                }
            }
        }
        public static void BuildFromFields(object source, XmlElement node)
        {
            BuildFromFields(source.GetType(), source, node);
        }
        public static void StoreFields(Type type, object source, XmlDocument doc, XmlElement node)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            FieldInfo[] fieldInfos = type.GetFields(flags);
            foreach (var info in fieldInfos)
            {
                object[] attributes = info.GetCustomAttributes(false);
                for (int i = 0; i < attributes.Length; ++i)
                {
                    if (attributes[i] is XmlAttributeAttribute)
                    {
                        var xmlAttribute = doc.CreateAttribute(info.Name);
                        xmlAttribute.Value = info.GetValue(source).ToString();
                        node.Attributes.Append(xmlAttribute);
                        break;
                    }
                }
            }
        }
        public static void StoreFields(object source, XmlDocument doc, XmlElement node)
        {
            StoreFields(source.GetType(), source, doc, node);
        }
        public static void BuildFromStruct<T>(ref T source, XmlElement node, string name)
        {
            XmlElement structNode = (XmlElement)node.SelectSingleNode(name);
            if (structNode == null)
                return;

            FieldInfo[] fieldInfos = source.GetType().GetFields();
            foreach (var info in fieldInfos)
            {
                if (!structNode.HasAttribute(info.Name))
                    continue;

                var text = structNode.GetAttribute(info.Name);
                object value;
                if (PropertyTypeRule.TryParse(info.GetValue(source), text, out value))
                    info.SetValueDirect(__makeref(source), value);
                else
                    throw new System.IO.FileLoadException("FileDataError");
            }
        }
        public static void StoreStruct<T>(T source, XmlDocument doc, XmlElement node, string name)
        {
            var structNode = doc.CreateElement(name);
            FieldInfo[] fieldInfos = source.GetType().GetFields();
            foreach (var info in fieldInfos)
            {
                var xmlAttribute = doc.CreateAttribute(info.Name);
                xmlAttribute.Value = info.GetValue(source).ToString();
                structNode.Attributes.Append(xmlAttribute);
            }
            node.AppendChild(structNode);
        }
        public static void BuildFromList(IList<string> source, XmlElement node, string name)
        {
            var listNode = node.SelectSingleNode(name);
            if (listNode == null)
                return;

            foreach (XmlElement childNode in listNode.ChildNodes)
            {
                if (!childNode.HasAttribute("Value"))
                    throw new System.IO.FileLoadException("FileDataError");

                var value = childNode.GetAttribute("Value");
                source.Add(value);
            }
        }
        public static void StoreList(IList<string> source, XmlDocument doc, XmlElement node, string name)
        {
            var listNode = doc.CreateElement(name);
            foreach (var item in source)
            {
                var itemNode = doc.CreateElement("Item");
                var valueAttribute = doc.CreateAttribute("Value");
                valueAttribute.Value = item;
                itemNode.Attributes.Append(valueAttribute);
                listNode.AppendChild(itemNode);
            }
            node.AppendChild(listNode);
        }
        public static void BuildFromObjectList<T>(IList<T> source, object prototype, XmlElement node, string name)
            where T : IXmlData
        {
            source.Clear();
            var objectListNode = (XmlElement)node.SelectSingleNode(name);
            if (objectListNode == null)
                return;

            foreach (XmlElement childNode in objectListNode.ChildNodes)
            {
                T obj = (T)((prototype as ICloneable).Clone());
                obj.BuildFromXml(childNode);
                source.Add(obj);
            }
        }
        public static void StoreObjectList<T>(IList<T> source, XmlDocument doc, XmlElement node, string name)
            where T : IXmlData
        {
            var objectListNode = doc.CreateElement(name);
            foreach (var obj in source)
                (obj as IXmlData).StoreAsXml(doc, objectListNode);

            node.AppendChild(objectListNode);
        }
        public static void BuildFromDictionary<K>(IDictionary<string, K> source, XmlElement node, string name)
            where K : new()
        {
            source.Clear();
            var dictionaryNode = (XmlElement)node.SelectSingleNode(name);
            if (dictionaryNode == null)
                return;

            foreach (XmlElement childNode in dictionaryNode.ChildNodes)
            {
                if (!childNode.HasAttribute("Key"))
                    continue;

                string key = childNode.GetAttribute("Key");
                if (!childNode.HasAttribute("Value"))
                    throw new System.IO.FileLoadException("FileDataError");

                string valueStr = childNode.GetAttribute("Value");
                K temp = new K();
                object value;
                if (PropertyTypeRule.TryParse(temp, valueStr, out value))
                    source[key] = (K)value;
                else
                    throw new System.IO.FileLoadException("FileDataError");
            }
        }
        public static void StoreDictionary<K>(IDictionary<string,K> source, XmlDocument doc, XmlElement node, string name)
        {
            var dictionaryNode = doc.CreateElement(name);
            foreach (var pair in source)
            {
                var pairNode = doc.CreateElement("Dictionary");
                var keyAttribute = doc.CreateAttribute("Key");
                keyAttribute.Value = pair.Key;
                pairNode.Attributes.Append(keyAttribute);
                var valueAttribute = doc.CreateAttribute("Value");
                valueAttribute.Value = pair.Value.ToString();
                pairNode.Attributes.Append(valueAttribute);
                dictionaryNode.AppendChild(pairNode);
            }
            node.AppendChild(dictionaryNode);
        }
        public static XmlElement FindNode(XmlElement node, string name)
        {
            var selectNode = node.SelectSingleNode(name);
            if (selectNode != null)
                return (XmlElement)selectNode;
            else
            {
                foreach (var childNode in node.ChildNodes)
                {
                    selectNode = FindNode((XmlElement)childNode, name);
                    if (selectNode != null)
                        return (XmlElement)selectNode;
                }
                return null;
            }
        }
    }
}
