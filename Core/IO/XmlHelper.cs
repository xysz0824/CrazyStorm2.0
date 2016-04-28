/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

namespace CrazyStorm.Core
{
    class XmlHelper
    {
        public static void StoreFields(Type type, object source, XmlDocument doc, XmlElement node)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            FieldInfo[] fieldInfos = type.GetFields(flags);
            foreach (var info in fieldInfos)
            {
                object[] attributes = info.GetCustomAttributes(false);
                if (attributes.Length > 0 && attributes[0] is XmlAttributeAttribute)
                {
                    var xmlAttribute = doc.CreateAttribute(info.Name);
                    xmlAttribute.Value = info.GetValue(source).ToString();
                    node.Attributes.Append(xmlAttribute);
                }
            }
        }
        public static void StoreFields(object source, XmlDocument doc, XmlElement node)
        {
            StoreFields(source.GetType(), source, doc, node);
        }
        public static void StoreStruct(ValueType source, XmlDocument doc, XmlElement node, string name)
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
        public static void StoreList<T>(IList<T> source, XmlDocument doc, XmlElement node, string name)
        {
            var listNode = doc.CreateElement(name);
            foreach (var item in source)
            {
                var itemNode = doc.CreateElement("Item");
                var valueAttribute = doc.CreateAttribute("Value");
                valueAttribute.Value = item.ToString();
                itemNode.Attributes.Append(valueAttribute);
                listNode.AppendChild(itemNode);
            }
            node.AppendChild(listNode);
        }
        public static void StoreObjectList<T>(IList<T> source, XmlDocument doc, XmlElement node, string name)
            where T : IXmlData
        {
            var objectListNode = doc.CreateElement(name);
            foreach (var obj in source)
                (obj as IXmlData).StoreAsXml(doc, objectListNode);

            node.AppendChild(objectListNode);
        }
        public static void StoreDictionary<T,K>(IDictionary<T,K> source, XmlDocument doc, XmlElement node, string name)
        {
            var dictionaryNode = doc.CreateElement(name);
            foreach (var pair in source)
            {
                var pairNode = doc.CreateElement("Dictionary");
                var keyAttribute = doc.CreateAttribute("Key");
                keyAttribute.Value = pair.Key.ToString();
                pairNode.Attributes.Append(keyAttribute);
                var valueAttribute = doc.CreateAttribute("Value");
                valueAttribute.Value = pair.Value.ToString();
                pairNode.Attributes.Append(valueAttribute);
                dictionaryNode.AppendChild(pairNode);
            }
            node.AppendChild(dictionaryNode);
        }
    }
}
