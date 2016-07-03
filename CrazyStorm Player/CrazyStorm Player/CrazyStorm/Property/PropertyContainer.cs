/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    public abstract class PropertyContainer
    {
        IDictionary<string, VMInstruction[]> propertyExpressions;
        public IDictionary<string, VMInstruction[]> PropertyExpressions { get { return propertyExpressions; } }
        public PropertyContainer()
        {
            propertyExpressions = new Dictionary<string, VMInstruction[]>();
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
