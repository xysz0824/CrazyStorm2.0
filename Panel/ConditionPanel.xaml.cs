/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using CrazyStorm.Expression;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Environment = CrazyStorm.Expression.Environment;

namespace CrazyStorm
{
    public sealed class ConditionChangedEventArgs : EventArgs
    {
        public string ModifiedCondition;
        public ConditionChangedEventArgs(string modified)
        {
            ModifiedCondition = modified;
        }
    }
    public partial class ConditionPanel : UserControl
    {
        #region Private Members
        Popup popup;
        bool internalSetting;
        #endregion

        #region Public Members
        public Environment Environment { get; set; }
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header),
                typeof(string), typeof(ConditionPanel), new PropertyMetadata(string.Empty));
        #endregion

        #region Constructor
        public ConditionPanel()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Methods
        void Reset()
        {
            SwitchToConditionMenu_Click(null, null);
            LeftLessThan.IsChecked = LeftEqual.IsChecked = LeftMoreThan.IsChecked = false;
            LeftConditionComboBox.SelectedIndex = -1;
            LeftValue.Text = string.Empty;
            And.IsChecked = false;
            Or.IsChecked = false;
            RightLessThan.IsChecked = RightEqual.IsChecked = RightMoreThan.IsChecked = false;
            RightConditionComboBox.SelectedIndex = -1;
            RightValue.Text = string.Empty;
            ConditionFunctionContent.Text = string.Empty;
        }
        public void AddConditionVariable(VariableComboBoxItem item)
        {
            LeftConditionComboBox.Items.Add(item);
            RightConditionComboBox.Items.Add(item);
        }
        public bool HasError()
        {
            return UIHelper.HasError(LeftValue) || UIHelper.HasError(RightValue) ||
                UIHelper.HasError(ConditionFunctionContent);
        }
        public string BuildCondition()
        {
            UIHelper.SetErrorToolTip(ConditionFunctionContent, null);
            if (ConditionFunction.Visibility == Visibility.Visible)
            {
                var lexer = new Lexer();
                lexer.Load(ExpressionHelper.ReverseTranslate(ConditionFunctionContent.Text));
                var expression = new Parser(lexer).Expression();
                if (expression is Number)
                {
                    UIHelper.SetErrorToolTip(ConditionFunctionContent, new ExpressionException("IllegalInput"));
                    return null;
                }
                try
                {
                    var result = expression.Eval(Environment);
                    if (!PropertyTypeRule.IsMatchWith(typeof(bool), result.GetType()))
                    {
                        UIHelper.SetErrorToolTip(ConditionFunctionContent, new ExpressionException("TypeError"));
                        return null;
                    }
                    return ExpressionHelper.ReverseTranslate(ConditionFunctionContent.Text);
                }
                catch (ExpressionException e)
                {
                    UIHelper.SetErrorToolTip(ConditionFunctionContent, e);
                    return null;
                }
            }
            else if (ConditionMenu.Visibility == Visibility.Visible)
            {
                var leftOperator = default(string);
                var leftProperty = default(string);
                var leftValue = default(string);
                if (LeftLessThan.IsChecked == true && LeftEqual.IsChecked == true) leftOperator = "<=";
                else if (LeftMoreThan.IsChecked == true && LeftEqual.IsChecked == true) leftOperator = ">=";
                else if (LeftLessThan.IsChecked == true && LeftMoreThan.IsChecked == true) leftOperator = "!=";
                else if (LeftLessThan.IsChecked == true) leftOperator = "<";
                else if (LeftEqual.IsChecked == true) leftOperator = "=";
                else if (LeftMoreThan.IsChecked == true) leftOperator = ">";
                if (LeftConditionComboBox.SelectedItem != null && !String.IsNullOrEmpty(LeftValue.Text) &&
                    !String.IsNullOrEmpty(leftOperator))
                {
                    var selectedItem = LeftConditionComboBox.SelectedItem as VariableComboBoxItem;
                    leftProperty = selectedItem.Name;
                    leftValue = LeftValue.Text;
                }
                else leftOperator = default(string);
                var rightOperator = default(string);
                var rightProperty = default(string);
                var rightValue = default(string);
                if (RightLessThan.IsChecked == true && RightEqual.IsChecked == true) rightOperator = "<=";
                else if (RightMoreThan.IsChecked == true && RightEqual.IsChecked == true) rightOperator = ">=";
                else if (RightLessThan.IsChecked == true && RightMoreThan.IsChecked == true) rightOperator = "!=";
                else if (RightLessThan.IsChecked == true) rightOperator = "<";
                else if (RightEqual.IsChecked == true) rightOperator = "=";
                else if (RightMoreThan.IsChecked == true) rightOperator = ">";
                if (RightConditionComboBox.SelectedItem != null && !String.IsNullOrEmpty(RightValue.Text) &&
                    !String.IsNullOrEmpty(rightOperator))
                {
                    var selectedItem = RightConditionComboBox.SelectedItem as VariableComboBoxItem;
                    rightProperty = selectedItem.Name;
                    rightValue = RightValue.Text;
                }
                else rightOperator = default(string);
                //Allow empty condition
                if (String.IsNullOrEmpty(leftProperty) && String.IsNullOrEmpty(rightProperty)) return null;
                var midOperator = default(string);
                if (!String.IsNullOrEmpty(leftProperty) && !String.IsNullOrEmpty(rightProperty))
                {
                    if (And.IsChecked == true) midOperator = "&";
                    else if (Or.IsChecked == true) midOperator = "|";
                }
                return string.Format("{0}{1}{2} {3} {4}{5}{6}", leftProperty, leftOperator, leftValue, midOperator,
                    rightProperty, rightOperator, rightValue).Trim();
            }
            return null;
        }
        public void MapCondition(string condition)
        {
            if (condition == null) return;
            internalSetting = true;
            Reset();
            ConditionFunctionContent.Text = ExpressionHelper.Translate(condition);
            var checkBoxMap = new Dictionary<string, CheckBox[]>();
            checkBoxMap[">"] = new[] { LeftMoreThan };
            checkBoxMap["="] = new[] { LeftEqual };
            checkBoxMap["<"] = new[] { LeftLessThan };
            checkBoxMap[">="] = new[] { LeftMoreThan, LeftEqual };
            checkBoxMap["<="] = new[] { LeftLessThan, LeftEqual };
            checkBoxMap["!="] = new[] { LeftLessThan, LeftMoreThan };
            var buttonMap = new Dictionary<string, RadioButton[]>();
            buttonMap["&"] = new[] { And };
            buttonMap["|"] = new[] { Or };
            var lexer = new Lexer();
            lexer.Load(condition);
            var expression = new Parser(lexer).Expression() as BinaryExpression;
            if (expression.LeafExpression)
            {
                var leftProperty = expression.GetLeftChild() as Name;
                if (leftProperty == null) return;
                var leftPropertyName = leftProperty.Token.GetValue() as string;
                for (int i = 0; i < LeftConditionComboBox.Items.Count; ++i)
                {
                    var item = LeftConditionComboBox.Items[i] as VariableComboBoxItem;
                    if (item.Name == leftPropertyName)
                        LeftConditionComboBox.SelectedIndex = i;
                }
                var op = (expression.Token as IdentifierToken).GetValue() as string;
                foreach (var checkBox in checkBoxMap[op]) checkBox.IsChecked = true;
                var leftValue = expression.GetRightChild();
                LeftValue.Text = leftValue.Token.GetValue().ToString();
                SwitchToConditionMenu_Click(null, null);
            }
            else
            {
                var left = expression.GetLeftChild() as BinaryExpression;
                var right = expression.GetRightChild() as BinaryExpression;
                if (left != null && left.LeafExpression && right != null && right.LeafExpression)
                {
                    var leftProperty = left.GetLeftChild() as Name;
                    if (leftProperty == null) return;
                    var leftPropertyName = leftProperty.Token.GetValue() as string;
                    for (int i = 0; i < LeftConditionComboBox.Items.Count; ++i)
                    {
                        var item = LeftConditionComboBox.Items[i] as VariableComboBoxItem;
                        if (item.Name == leftPropertyName)
                            LeftConditionComboBox.SelectedIndex = i;
                    }
                    var leftOp = (left.Token as IdentifierToken).GetValue() as string;
                    foreach (var checkBox in checkBoxMap[leftOp]) checkBox.IsChecked = true;
                    var leftValue = left.GetRightChild();
                    LeftValue.Text = leftValue.Token.GetValue().ToString();

                    var midOp = (expression.Token as IdentifierToken).GetValue() as string;
                    foreach (var button in buttonMap[midOp]) button.IsChecked = true;

                    checkBoxMap[">"] = new[] { RightMoreThan };
                    checkBoxMap["="] = new[] { RightEqual };
                    checkBoxMap["<"] = new[] { RightLessThan };
                    checkBoxMap[">="] = new[] { RightMoreThan, RightEqual };
                    checkBoxMap["<="] = new[] { RightLessThan, RightEqual };
                    checkBoxMap["!="] = new[] { RightLessThan, RightMoreThan };
                    var rightProperty = right.GetLeftChild() as Name;
                    if (rightProperty == null) return;
                    var rightPropertyName = rightProperty.Token.GetValue() as string;
                    for (int i = 0; i < RightConditionComboBox.Items.Count; ++i)
                    {
                        var item = RightConditionComboBox.Items[i] as VariableComboBoxItem;
                        if (item.Name == rightPropertyName)
                            RightConditionComboBox.SelectedIndex = i;
                    }
                    var rightOp = (right.Token as IdentifierToken).GetValue() as string;
                    foreach (var checkBox in checkBoxMap[rightOp]) checkBox.IsChecked = true;
                    var rightValue = right.GetRightChild();
                    RightValue.Text = rightValue.Token.GetValue().ToString();
                    SwitchToConditionMenu_Click(null, null);
                }
                else
                {
                    SwitchToFunction_Click(null, null);
                }
            }
            internalSetting = false;
        }
        #endregion

        #region Window EventHandlers
        private void LeftConditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LeftValue_PreviewLostKeyboardFocus(sender, null);
        }
        private void RightConditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RightValue_PreviewLostKeyboardFocus(sender, null);
        }
        private void LeftValue_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (LeftConditionComboBox.SelectedItem != null)
            {
                var item = LeftConditionComboBox.SelectedItem as VariableComboBoxItem;
                object value = Environment.GetProperty(item.Name);
                popup = new Popup();
                UIHelper.ShowIntellisense(popup, value.GetType(), LeftValue);
            }
        }
        private void LeftValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UIHelper.HideIntellisense(popup);
            UIHelper.SetErrorToolTip(LeftValue, null);
            LeftValue.Text = LeftValue.Text.Trim();
            string input = LeftValue.Text;
            if (String.IsNullOrEmpty(input)) return;
            if (LeftConditionComboBox.SelectedItem != null)
            {
                var item = LeftConditionComboBox.SelectedItem as VariableComboBoxItem;
                object value = Environment.GetProperty(item.Name);
                if (value != null)
                {
                    if (!PropertyTypeRule.TryParse(value, input, out value))
                    {
                        UIHelper.SetErrorToolTip(LeftValue, new ExpressionException("TypeError"));
                        return;
                    }
                    LeftValue.Text = value.ToString();
                    return;
                }
                if (value == null) value = Environment.GetLocal(item.Name);
                if (value == null) value = Environment.GetGlobal(item.Name);
                if (value != null)
                {
                    float testValue;
                    if (!float.TryParse(input, out testValue))
                    {
                        UIHelper.SetErrorToolTip(LeftValue, new ExpressionException("TypeError"));
                        return;
                    }
                }
            }
        }
        private void RightValue_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (RightConditionComboBox.SelectedItem != null)
            {
                var item = RightConditionComboBox.SelectedItem as VariableComboBoxItem;
                object value = Environment.GetProperty(item.Name);
                popup = new Popup();
                UIHelper.ShowIntellisense(popup, value.GetType(), RightValue);
            }
        }
        private void RightValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UIHelper.HideIntellisense(popup);
            UIHelper.SetErrorToolTip(RightValue, null);
            RightValue.Text = RightValue.Text.Trim();
            string input = RightValue.Text;
            if (String.IsNullOrEmpty(input)) return;
            if (RightConditionComboBox.SelectedItem != null)
            {
                var item = RightConditionComboBox.SelectedItem as VariableComboBoxItem;
                object value = Environment.GetProperty(item.Name);
                if (value != null)
                {
                    if (!PropertyTypeRule.TryParse(value, input, out value))
                    {
                        UIHelper.SetErrorToolTip(RightValue, new ExpressionException("TypeError"));
                        return;
                    }
                    RightValue.Text = value.ToString();
                    return;
                }
                if (value == null) value = Environment.GetLocal(item.Name);
                if (value == null) value = Environment.GetGlobal(item.Name);
                if (value != null)
                {
                    float testValue;
                    if (!float.TryParse(input, out testValue))
                    {
                        UIHelper.SetErrorToolTip(RightValue, new ExpressionException("TypeError"));
                        return;
                    }
                }
            }
        }
        private void LeftOperator_Checked(object sender, RoutedEventArgs e)
        {
            if (internalSetting) return;
            if (LeftMoreThan.IsChecked == true && LeftLessThan.IsChecked == true && LeftEqual.IsChecked == true)
            {
                if (sender == LeftMoreThan)
                {
                    LeftLessThan.IsChecked = false;
                }
                else if (sender == LeftLessThan)
                {
                    LeftMoreThan.IsChecked = false;
                }
                else if (sender == LeftEqual)
                {
                    LeftMoreThan.IsChecked = false;
                }
            }
        }
        private void LeftOperator_Unchecked(object sender, RoutedEventArgs e)
        {
            if (internalSetting) return;
            if (LeftMoreThan.IsChecked == false && LeftLessThan.IsChecked == false && LeftEqual.IsChecked == false)
            {
                if (sender == LeftMoreThan || sender == LeftLessThan)
                {
                    LeftEqual.IsChecked = true;
                }
                else if (sender == LeftEqual)
                {
                    LeftMoreThan.IsChecked = true;
                }
            }
        }
        private void RightOperator_Checked(object sender, RoutedEventArgs e)
        {
            if (internalSetting) return;
            if (RightMoreThan.IsChecked == true && RightLessThan.IsChecked == true && RightEqual.IsChecked == true)
            {
                if (sender == RightMoreThan)
                {
                    RightLessThan.IsChecked = false;
                }
                else if (sender == RightLessThan)
                {
                    RightMoreThan.IsChecked = false;
                }
                else if (sender == RightEqual)
                {
                    RightMoreThan.IsChecked = false;
                }
            }
        }
        private void RightOperator_Unchecked(object sender, RoutedEventArgs e)
        {
            if (internalSetting) return;
            if (RightMoreThan.IsChecked == false && RightLessThan.IsChecked == false && RightEqual.IsChecked == false)
            {
                if (sender == RightMoreThan || sender == RightLessThan)
                {
                    RightEqual.IsChecked = true;
                }
                else if (sender == RightEqual)
                {
                    RightMoreThan.IsChecked = true;
                }
            }
        }
        private void SwitchToFunction_Click(object sender, RoutedEventArgs e)
        {
            ConditionMenu.Visibility = Visibility.Collapsed;
            ConditionFunction.Visibility = Visibility.Visible;
        }
        private void SwitchToConditionMenu_Click(object sender, RoutedEventArgs e)
        {
            ConditionFunction.Visibility = Visibility.Collapsed;
            ConditionMenu.Visibility = Visibility.Visible;
        }
        private void ConditionFunctionContent_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UIHelper.SetErrorToolTip(ConditionFunctionContent, null);
            ConditionFunctionContent.Text = ConditionFunctionContent.Text.Trim();
            string input = ExpressionHelper.ReverseTranslate(ConditionFunctionContent.Text);
            if (String.IsNullOrEmpty(input)) return;
            try
            {
                var lexer = new Lexer();
                lexer.Load(input);
                var syntaxTree = new Parser(lexer).Expression();
                var result = syntaxTree.Eval(Environment);
                if (!(result is bool)) throw new ExpressionException("TypeError");
            }
            catch (ExpressionException error)
            {
                UIHelper.SetErrorToolTip(ConditionFunctionContent, error);
            }
        }
        #endregion
    }
}
