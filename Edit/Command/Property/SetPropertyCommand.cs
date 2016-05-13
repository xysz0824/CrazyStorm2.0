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
using CrazyStorm.Expression;

namespace CrazyStorm
{
    class SetPropertyCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var environment = Parameter[0] as Expression.Environment;
            var container = Parameter[1] as PropertyContainer;
            var property = Parameter[2] as PropertyGridItem;
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
            var environment = Parameter[0] as Expression.Environment;
            var container = Parameter[1] as PropertyContainer;
            var property = Parameter[2] as PropertyGridItem;
            var cell = Parameter[3] as DataGridCell;
            var newValue = History[0] as string;
            var attribute = Parameter[5] as PropertyAttribute;
            var updateFunc = Parameter[6] as Action;
            SetProperty(environment, container, property.Info, cell, newValue, attribute, updateFunc);
            //If set invalid value, pop undo stack to prevent for redoing again.
            if (History[1] != null && !(bool)History[1])
                stack.UndoPop();
        }
        static bool SetProperty(Expression.Environment environment, PropertyContainer container, 
            PropertyInfo propertyInfo, DataGridCell cell, string newValue, PropertyAttribute attribute, Action updateFunc)
        {
            try
            {
                object value;
                if (attribute.IsLegal(newValue, out value))
                {
                    container.Properties[propertyInfo.Name].Expression = false;
                    propertyInfo.GetSetMethod().Invoke(container, new object[] { value });
                    cell.ToolTip = null;
                    cell.BorderThickness = new Thickness(0);
                    updateFunc();
                    return true;
                }
                if (attribute is StringPropertyAttribute)
                    throw new ExpressionException("Illegal string.");

                var lexer = new Lexer();
                lexer.Load(newValue);
                var syntaxTree = new Parser(lexer).Expression();
                if (syntaxTree is Number)
                    throw new ExpressionException("Illegal input.");

                var result = syntaxTree.Eval(environment);
                if (!PropertyTypeRule.IsMatchWith(propertyInfo.PropertyType, result.GetType()))
                    throw new ExpressionException("Type error.");

                container.Properties[propertyInfo.Name].Expression = true;
                container.Properties[propertyInfo.Name].Value = newValue;
                cell.ToolTip = null;
                cell.BorderThickness = new Thickness(0);
                updateFunc();
                return true;
            }
            catch (ExpressionException e)
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
    }
}
