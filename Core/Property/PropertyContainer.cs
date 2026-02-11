/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public abstract class PropertyContainer : ICloneable
    {
        Dictionary<string, PropertyValue> properties;
        public Dictionary<string, PropertyValue> Properties { get { return properties; } }
        Dictionary<int, VMInstruction[]> propertyExpressions;
        public Dictionary<int, VMInstruction[]> PropertyExpressions { get { return propertyExpressions; } }
        public long ID { get; set; }
        public ParticleSystem System { get; set; }

        public PropertyContainer()
        {
            properties = new Dictionary<string, PropertyValue>();
            propertyExpressions = new Dictionary<int, VMInstruction[]>();
        }
        public List<PropertyInfo> InitializeAndGetProperties(Type type)
        {
            var propertiesInfo = new List<PropertyInfo>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.DeclaringType.Name != type.Name) continue;
                object[] attributes = property.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    if (attribute is PropertyAttribute)
                    {
                        propertiesInfo.Add(property);
                        if (!properties.ContainsKey(property.Name))
                        {
                            var obj = property.GetGetMethod().Invoke(this, null);
                            var value = new PropertyValue { Value = obj == null ? "" : obj.ToString() };
                            properties[property.Name] = value;
                        }
                        break;
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
                foreach (var attribute in attributes)
                {
                    if (attribute is PropertyAttribute)
                    {
                        propertiesInfo.Add(property);
                        break;
                    }
                }
            }
            return propertiesInfo;
        }
        public virtual object Clone()
        {
            var clone = MemberwiseClone() as PropertyContainer;
            clone.properties = new Dictionary<string, PropertyValue>();
            foreach (var pair in properties) clone.properties[pair.Key] = pair.Value.Clone() as PropertyValue;
            return clone;
        }
        public virtual void CopyTo(PropertyContainer target)
        {
            target.propertyExpressions = propertyExpressions;
        }
        public void BuildFromXmlElement(XmlElement node)
        {
            var propertiesNode = node.SelectSingleNode("Properties");
            if (propertiesNode == null) throw new System.IO.FileLoadException("FileDataError");
            foreach (XmlElement childNode in propertiesNode.ChildNodes)
            {
                if (!childNode.HasAttribute("Key")) throw new System.IO.FileLoadException("FileDataError");
                string key = childNode.GetAttribute("Key");
                if (!childNode.HasAttribute("Value")) throw new System.IO.FileLoadException("FileDataError");
                string expression = childNode.GetAttribute("Value");
                PropertyInfo property = GetType().GetProperty(key);
                if (property == null) throw new System.IO.FileLoadException("FileDataError");
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
        public void GeneratePropertyExpressions(List<VariableResource> variables, List<byte> data)
        {
            List<byte> newData = new List<byte>();
            foreach (var pair in properties)
            {
                if (pair.Value.Expression)
                {
                    List<byte> pairData = new List<byte>();
                    int propertyID = Expression.Environment.GetPropertyID(pair.Key, GetType(), null, variables);
                    if (propertyID == int.MinValue) throw new Exception($"Can't find {pair.Key}");
                    pairData.AddRange(BitConverter.GetBytes(propertyID));
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
                        int propertyID = expressionReader.ReadInt32();
                        int bytesLength = (int)expressionReader.BaseStream.Length - sizeof(int);
                        propertyExpressions[propertyID] = VM.Decode(expressionReader.ReadBytes(bytesLength));
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
        public bool ExecuteRandomExpression(int id, float frameScale)
        {
            if (!propertyExpressions.ContainsKey(id)) return false;
            var expression = propertyExpressions[id];
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
        public void ExecuteDynamicExpression(int id, float frameScale)
        {
            if (!propertyExpressions.ContainsKey(id)) return;
            for (int i = 0;i < propertyExpressions[id].Length; ++i)
            {
                var instruction = propertyExpressions[id][i];
                if (instruction.code == VMCode.NAME)
                {
                    VM.Execute(this, propertyExpressions[id], frameScale);
                    SetProperty(id);
                    VM.Clear();
                    break;
                }
            }
        }
        public void ExecuteDynamicExpressions(float frameScale)
        {
            foreach (var expression in propertyExpressions)
            {
                for (int i = 0;i < expression.Value.Length; ++i)
                {
                    var instruction = expression.Value[i];
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
            foreach (var expression in propertyExpressions)
            {
                VM.Execute(this, expression.Value, frameScale);
                SetProperty(expression.Key);
                VM.Clear();
            }
        }
        public abstract bool PushProperty(int propertyID);
        public abstract bool SetProperty(int propertyID);
    }
}
