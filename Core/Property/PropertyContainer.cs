/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
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
    public abstract class PropertyContainer : ICloneable, ICopyable<PropertyContainer>
    {
        IDictionary<string, PropertyValue> properties;
        public IDictionary<string, PropertyValue> Properties { get { return properties; } }
        IDictionary<string, VMInstruction[]> propertyExpressions;
        public IDictionary<string, VMInstruction[]> PropertyExpressions { get { return propertyExpressions; } }
        public ParticleSystem System { get; set; }

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
                        var obj = property.GetGetMethod().Invoke(this, null);
                        var value = new PropertyValue { Value = obj == null ? "" : obj.ToString() };
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
        public virtual void CopyTo(PropertyContainer target)
        {
            target.properties = properties;
            target.propertyExpressions = propertyExpressions;
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
        public void GeneratePropertyExpressions(List<byte> data)
        {
            List<byte> newData = new List<byte>();
            foreach (var pair in properties)
            {
                if (pair.Value.Expression)
                {
                    List<byte> pairData = new List<byte>();
                    pairData.AddRange(PlayDataHelper.GetStringBytes(pair.Key));
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
        public int ExtractBlock(VMInstruction[] original, int end, out int start)
        {
            start = end;
            var needOperand = VM.GetOperandConsumption(original[start]);
            if (needOperand <= 0) return needOperand;
            start--;
            while (start >= 0)
            {
                var instruction = original[start];
                var consumption = VM.GetOperandConsumption(instruction);
                if (consumption > 0) needOperand--;
                needOperand += consumption;
                if (needOperand <= 0) break;
                start--;
            }
            return needOperand;
        }
        public bool ExecuteRandomExpression(string name, float frameScale)
        {
            if (!PropertyExpressions.ContainsKey(name)) return false;
            var expression = PropertyExpressions[name];
            if (expression[expression.Length - 1].code == VMCode.RAND)
            {
                //Entire expression is random
                VM.Execute(this, expression, frameScale);
                return true;
            }
            else
            {
                //Partial random expression
                for (int i = expression.Length - 1; i >= 0; --i)
                {
                    var instruction = expression[i];
                    if (instruction.code == VMCode.RAND)
                    {
                        var rand = 0f;
                        ExtractBlock(expression, i, out int startIndex);
                        VM.Execute(this, expression, startIndex, i, frameScale);
                        rand = VM.PopFloat();
                        for (int k = i + 1; k < expression.Length; ++k)
                        {
                            var needOperand = ExtractBlock(expression, k, out int sIndex);
                            if (needOperand != 0 || sIndex > startIndex) continue;
                            if (expression[k].code == VMCode.SUB && k == i + 1) rand = -rand;
                            break;
                        }
                        VM.PushFloat(rand);
                        return true;
                    }
                    else if (instruction.code == VMCode.VECTOR2)
                    {
                        //Entire expression is Vector2
                        ExtractBlock(expression, i - 1, out int rightStartIndex);
                        var randV = new Vector2();
                        //Right Part
                        for (int j = i - 1; j >= rightStartIndex; --j)
                        {
                            instruction = expression[j];
                            if (instruction.code != VMCode.RAND) continue;
                            ExtractBlock(expression, j, out int startIndex);
                            VM.Execute(this, expression, startIndex, j, frameScale);
                            randV.x = VM.PopFloat();
                            for (int k = j + 1; k < expression.Length; ++k)
                            {
                                var needOperand = ExtractBlock(expression, k, out int sIndex);
                                if (needOperand != 0 || sIndex > startIndex) continue;
                                if (expression[k].code == VMCode.SUB && k == j + 1) randV.x = -randV.x;
                                break;
                            }
                            break;
                        }
                        ExtractBlock(expression, rightStartIndex - 1, out int leftStartIndex);
                        //Left Part
                        for (int j = rightStartIndex - 1; j >= leftStartIndex; --j)
                        {
                            instruction = expression[j];
                            if (instruction.code != VMCode.RAND) continue;
                            ExtractBlock(expression, j, out int startIndex);
                            VM.Execute(this, expression, startIndex, j, frameScale);
                            randV.y = VM.PopFloat();
                            for (int k = j + 1; k < expression.Length; ++k)
                            {
                                var needOperand = ExtractBlock(expression, k, out int sIndex);
                                if (needOperand != 0 || sIndex > startIndex) continue;
                                if (expression[k].code == VMCode.SUB && k == j + 1) randV.y = -randV.y;
                                break;
                            }
                            break;
                        }
                        VM.PushVector2(randV);
                        return true;
                    }
                }
            }
            return false;
        }
        public void ExecuteDynamicExpression(string name, float frameScale)
        {
            if (!PropertyExpressions.ContainsKey(name)) return;
            foreach (var instruction in PropertyExpressions[name])
            {
                if (instruction.code == VMCode.NAME)
                {
                    VM.Execute(this, PropertyExpressions[name], frameScale);
                    SetProperty(name);
                    VM.Clear();
                    break;
                }
            }
        }
        public void ExecuteDynamicExpressions(float frameScale)
        {
            foreach (var expression in PropertyExpressions)
            {
                foreach (var instruction in expression.Value)
                {
                    if (instruction.code == VMCode.NAME)
                    {
                        VM.Execute(this, expression.Value, frameScale);
                        SetProperty(expression.Key);
                        VM.Clear();
                        break;
                    }
                }
            }
        }
        public void ExecuteExpressionsAndSet(float frameScale)
        {
            foreach (var expression in PropertyExpressions)
            {
                VM.Execute(this, expression.Value, frameScale);
                SetProperty(expression.Key);
                VM.Clear();
            }
        }
        public abstract bool PushProperty(string propertyName);
        public abstract bool SetProperty(string propertyName);
    }
}
