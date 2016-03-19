/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CrazyStorm.Core;
using CrazyStorm.Script;

namespace CrazyStorm
{
    class SetPropertyCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var environment = Parameter[0] as Script.Environment;
            var container = Parameter[1] as PropertyContainer;
            var property = Parameter[2] as PropertyPanelItem;
            var cell = Parameter[3] as DataGridCell;
            var newValue = Parameter[4] as string;
            var attribute = Parameter[5] as PropertyAttribute;
            var updateFunc = Parameter[6] as Action;
            if (History[0] == null)
                History[0] = property.Value;
            History[1] = SetProperty(environment, container, property.Info, cell, newValue, attribute, updateFunc);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var environment = Parameter[0] as Script.Environment;
            var container = Parameter[1] as PropertyContainer;
            var property = Parameter[2] as PropertyPanelItem;
            var cell = Parameter[3] as DataGridCell;
            var newValue = History[0] as string;
            var attribute = Parameter[5] as PropertyAttribute;
            var updateFunc = Parameter[6] as Action;
            SetProperty(environment, container, property.Info, cell, newValue, attribute, updateFunc);
            //If set invalid value, pop undo stack to prevent for redoing again.
            if (History[1] != null && !(bool)History[1])
                stack.UndoPop();
        }
        static bool SetProperty(Script.Environment environment, PropertyContainer container, 
            PropertyInfo propertyInfo, DataGridCell cell, string newValue, PropertyAttribute attribute, Action updateFunc)
        {
            object value;
            if (attribute.IsLegal(newValue, out value))
            {
                container.Properties[propertyInfo].Expression = false;
                propertyInfo.GetSetMethod().Invoke(container, new object[] { value });
                cell.BorderThickness = new Thickness(0);
                updateFunc();
                return true;
            }
            try
            {
                var lexer = new Lexer();
                lexer.Load(newValue);
                var syntaxTree = new Parser(lexer).Expression();
                if (syntaxTree is Number)
                    throw new ScriptException("Illegal input.");
                var result = syntaxTree.Test(environment);
                if (!IsImplicitFrom(propertyInfo.PropertyType, result.GetType()))
                    throw new ScriptException("Type error.");

                container.Properties[propertyInfo].Expression = true;
                container.Properties[propertyInfo].Value = newValue;
                cell.ToolTip = null;
                cell.BorderThickness = new Thickness(0);
                updateFunc();
                return true;
            }
            catch (ScriptException e)
            {
                var tip = new ToolTip();
                var tipText = new TextBlock();
                tipText.Text = e.Message;
                tip.Content = tipText;
                cell.ToolTip = tip;
                cell.BorderBrush = new SolidColorBrush(Colors.Red);
                cell.BorderThickness = new Thickness(2);
                return false;
            }
        }
        static bool IsImplicitFrom(System.Type typeA, System.Type typeB)
        {
            if (typeA.Equals(typeB))
                return true;

            System.Type intType = typeof(int);
            System.Type floatType = typeof(float);
            if ((typeA.Equals(intType) && typeB.Equals(floatType)) ||
                (typeA.Equals(floatType) && typeB.Equals(intType)))
                return true;

            return false;
        }
    }
}
