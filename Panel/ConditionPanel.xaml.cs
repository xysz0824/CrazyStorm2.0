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
        public event EventHandler<ConditionChangedEventArgs> ConditionChanged;
        public event EventHandler ConditionConfirmed;
        #endregion

        #region Constructor
        public ConditionPanel()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Methods
        void ResetConditionMenu()
        {
            LeftLessThan.IsChecked = LeftEqual.IsChecked = LeftMoreThan.IsChecked = false;
            LeftConditionComboBox.SelectedIndex = -1;
            LeftValue.Text = string.Empty;
            And.IsChecked = false;
            Or.IsChecked = false;
            RightLessThan.IsChecked = RightEqual.IsChecked = RightMoreThan.IsChecked = false;
            RightConditionComboBox.SelectedIndex = -1;
            RightValue.Text = string.Empty;
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
            if (!IsInitialized) return null;
            UIHelper.SetErrorToolTip(ConditionFunctionContent, null);
            UIHelper.HideIntellisense(popup);
            UIHelper.SetErrorToolTip(LeftValue, null);
            UIHelper.SetErrorToolTip(RightValue, null);
            if (ConditionFunction.Visibility == Visibility.Visible)
            {
                var lexer = new Lexer();
                ConditionFunctionContent.Text = ConditionFunctionContent.Text.Trim();
                if (string.IsNullOrEmpty(ConditionFunctionContent.Text))
                {
                    MapEmptyConditionToMenu();
                    return string.Empty;
                }
                var input = ExpressionHelper.ReverseTranslate(ConditionFunctionContent.Text);
                lexer.Load(input);
                var expression = new Parser(lexer).Expression();
                if (expression is Number)
                {
                    UIHelper.SetErrorToolTip(ConditionFunctionContent, new ExpressionException("IllegalInput"));
                    return null;
                }
                try
                {
                    var v = expression.Eval(Environment);
                    if (!PropertyTypeRule.IsMatchWith(typeof(bool), v.GetType()))
                    {
                        UIHelper.SetErrorToolTip(ConditionFunctionContent, new ExpressionException("TypeError"));
                        return null;
                    }
                    MapConditionToMenu(input, true);
                    return input;
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
                    var item = LeftConditionComboBox.SelectedItem as VariableComboBoxItem;
                    var finalText = default(string);
                    LeftValue.Text = LeftValue.Text.Trim();
                    var input = ExpressionHelper.ReverseTranslate(LeftValue.Text);
                    try
                    {
                        if (!string.IsNullOrEmpty(input))
                        {
                            object value = Environment.GetProperty(item.Name);
                            if (value != null)
                            {
                                if (PropertyTypeRule.TryParse(value, input, out value))
                                {
                                    finalText = ExpressionHelper.Translate(value.ToString());
                                }
                                else
                                {
                                    var lexer = new Lexer();
                                    lexer.Load(input);
                                    var syntaxTree = new Parser(lexer).Expression();
                                    if (syntaxTree is Number)
                                    {
                                        UIHelper.SetErrorToolTip(LeftValue, new ExpressionException("TypeError"));
                                    }
                                    else
                                    {
                                        var eval = syntaxTree.Eval(Environment);
                                        if (!(PropertyTypeRule.IsMatchWith(value.GetType(), eval.GetType())))
                                        {
                                            UIHelper.SetErrorToolTip(LeftValue, new ExpressionException("TypeError"));
                                        }
                                        else
                                        {
                                            finalText = LeftValue.Text;
                                        }
                                    }
                                }
                            }
                            if (value == null) value = Environment.GetLocal(item.Name);
                            if (value == null) value = Environment.GetGlobal(item.Name);
                            if (value != null)
                            {
                                //Fields of support struct must be float type.
                                var lexer = new Lexer();
                                lexer.Load(input);
                                var syntaxTree = new Parser(lexer).Expression();
                                var eval = syntaxTree.Eval(Environment);
                                if (!(eval is float))
                                {
                                    UIHelper.SetErrorToolTip(LeftValue, new ExpressionException("TypeError"));
                                }
                                else finalText = LeftValue.Text;
                            }
                        }
                    }
                    catch (ExpressionException e)
                    {
                        UIHelper.SetErrorToolTip(LeftValue, e);
                    } 
                    if (!string.IsNullOrEmpty(finalText))
                    {
                        leftProperty = item.Name;
                        leftValue = LeftValue.Text;
                    }
                }
                else leftOperator = default;
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
                    var item = RightConditionComboBox.SelectedItem as VariableComboBoxItem;
                    var finalText = default(string);
                    RightValue.Text = RightValue.Text.Trim();
                    var input = ExpressionHelper.ReverseTranslate(RightValue.Text);
                    try
                    {
                        if (!string.IsNullOrEmpty(input))
                        {
                            object value = Environment.GetProperty(item.Name);
                            if (value != null)
                            {
                                if (PropertyTypeRule.TryParse(value, input, out value))
                                {
                                    finalText = ExpressionHelper.Translate(value.ToString());
                                }
                                else
                                {
                                    var lexer = new Lexer();
                                    lexer.Load(input);
                                    var syntaxTree = new Parser(lexer).Expression();
                                    if (syntaxTree is Number)
                                    {
                                        UIHelper.SetErrorToolTip(RightValue, new ExpressionException("TypeError"));
                                    }
                                    else
                                    {
                                        var eval = syntaxTree.Eval(Environment);
                                        if (!(PropertyTypeRule.IsMatchWith(value.GetType(), eval.GetType())))
                                        {
                                            UIHelper.SetErrorToolTip(RightValue, new ExpressionException("TypeError"));
                                        }
                                        else
                                        {
                                            finalText = RightValue.Text;
                                        }
                                    }
                                }
                            }
                            if (value == null) value = Environment.GetLocal(item.Name);
                            if (value == null) value = Environment.GetGlobal(item.Name);
                            if (value != null)
                            {
                                //Fields of support struct must be float type.
                                var lexer = new Lexer();
                                lexer.Load(input);
                                var syntaxTree = new Parser(lexer).Expression();
                                var eval = syntaxTree.Eval(Environment);
                                if (!(eval is float))
                                {
                                    UIHelper.SetErrorToolTip(RightValue, new ExpressionException("TypeError"));
                                }
                                else finalText = RightValue.Text;
                            }
                        }
                    }
                    catch (ExpressionException e)
                    {
                        UIHelper.SetErrorToolTip(RightValue, e);
                    }
                    if (!string.IsNullOrEmpty(finalText))
                    {
                        rightProperty = item.Name;
                        rightValue = RightValue.Text;
                    }
                }
                else rightOperator = default;
                //Allow empty condition
                if (String.IsNullOrEmpty(leftProperty) && String.IsNullOrEmpty(rightProperty)) return null;
                var midOperator = default(string);
                if (!String.IsNullOrEmpty(leftProperty) && !String.IsNullOrEmpty(rightProperty))
                {
                    if (And.IsChecked == true) midOperator = " & ";
                    else if (Or.IsChecked == true) midOperator = " | ";
                }
                var result = string.Format("{0}{1}{2}{3}{4}{5}{6}", leftProperty, leftOperator, leftValue, midOperator,
                    rightProperty, rightOperator, rightValue).Trim();
                ConditionFunctionContent.Text = ExpressionHelper.Translate(result);
                return result;
            }
            return null;
        }
        private void MapEmptyConditionToMenu()
        {
            internalSetting = true;
            ResetConditionMenu();
            internalSetting = false;    
        }
        private void MapConditionToMenu(string condition, bool internalMap)
        {
            internalSetting = true;
            ResetConditionMenu();
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
            if (expression.SimpleLeftExpression)
            {
                var leftProperty = expression.GetLeftChild() as Name;
                if (leftProperty == null) return;
                var leftPropertyName = leftProperty.Token.GetValue() as string;
                for (int i = 0; i < LeftConditionComboBox.Items.Count; ++i)
                {
                    var item = LeftConditionComboBox.Items[i] as VariableComboBoxItem;
                    if (item.Name == leftPropertyName)
                    {
                        LeftConditionComboBox.SelectedIndex = i;
                        break;
                    }
                }
                var op = (expression.Token as IdentifierToken).GetValue() as string;
                foreach (var checkBox in checkBoxMap[op]) checkBox.IsChecked = true;
                var leftValue = expression.GetRightChild();
                LeftValue.Text = leftValue.ToString();
                if (!internalMap) SwitchToConditionMenu_Click(null, null);
            }
            else
            {
                var left = expression.GetLeftChild() as BinaryExpression;
                var right = expression.GetRightChild() as BinaryExpression;
                if (left != null && left.SimpleLeftExpression && right != null && right.SimpleLeftExpression)
                {
                    var leftProperty = left.GetLeftChild() as Name;
                    if (leftProperty == null) return;
                    var leftPropertyName = leftProperty.Token.GetValue() as string;
                    for (int i = 0; i < LeftConditionComboBox.Items.Count; ++i)
                    {
                        var item = LeftConditionComboBox.Items[i] as VariableComboBoxItem;
                        if (item.Name == leftPropertyName)
                        {
                            LeftConditionComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                    var leftOp = (left.Token as IdentifierToken).GetValue() as string;
                    foreach (var checkBox in checkBoxMap[leftOp]) checkBox.IsChecked = true;
                    var leftValue = left.GetRightChild();
                    LeftValue.Text = leftValue.ToString();

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
                        {
                            RightConditionComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                    var rightOp = (right.Token as IdentifierToken).GetValue() as string;
                    foreach (var checkBox in checkBoxMap[rightOp]) checkBox.IsChecked = true;
                    var rightValue = right.GetRightChild();
                    RightValue.Text = rightValue.ToString();
                    if (!internalMap) SwitchToConditionMenu_Click(null, null);
                }
                else
                {
                    if (!internalMap) SwitchToFunction_Click(null, null);
                }
            }
            internalSetting = false;
        }
        public void MapCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition)) return;
            ConditionFunctionContent.Text = ExpressionHelper.Translate(condition);
            MapConditionToMenu(condition, false);
        }
        #endregion

        #region Window EventHandlers
        private void LeftConditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (internalSetting) return;
            LeftValue_PreviewLostKeyboardFocus(sender, null);
        }
        private void RightConditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (internalSetting) return;
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
            var condition = BuildCondition();
            if (condition != null)
            {
                ConditionChanged?.Invoke(this, new ConditionChangedEventArgs(condition));
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
            var condition = BuildCondition();
            if (condition != null)
            {
                ConditionChanged?.Invoke(this, new ConditionChangedEventArgs(condition));
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
            var condition = BuildCondition();
            if (condition != null)
            {
                ConditionChanged?.Invoke(this, new ConditionChangedEventArgs(condition));
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
            var condition = BuildCondition();
            if (condition != null)
            {
                ConditionChanged?.Invoke(this, new ConditionChangedEventArgs(condition));
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
            var condition = BuildCondition();
            if (condition != null)
            {
                ConditionChanged?.Invoke(this, new ConditionChangedEventArgs(condition));
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
            var condition = BuildCondition();
            if (condition != null)
            {
                ConditionChanged?.Invoke(this, new ConditionChangedEventArgs(condition));
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
            var condition = BuildCondition();
            if (condition != null)
            {
                ConditionChanged?.Invoke(this, new ConditionChangedEventArgs(condition));
            }
        }
        private void ConditionFunctionContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var condition = BuildCondition();
                if (condition != null)
                {
                    ConditionChanged?.Invoke(this, new ConditionChangedEventArgs(condition));
                    ConditionConfirmed?.Invoke(this, e);
                }
                ConditionFunctionContent.CaretIndex = ConditionFunctionContent.Text.Length;
            }
        }
        private void LeftValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var condition = BuildCondition();
                if (condition != null)
                {
                    ConditionChanged?.Invoke(this, new ConditionChangedEventArgs(condition));
                    ConditionConfirmed?.Invoke(this, e);
                }
                LeftValue.CaretIndex = LeftValue.Text.Length;
            }
        }
        private void RightValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var condition = BuildCondition();
                if (condition != null)
                {
                    ConditionChanged?.Invoke(this, new ConditionChangedEventArgs(condition));
                    ConditionConfirmed?.Invoke(this, e);
                }
                RightValue.CaretIndex = RightValue.Text.Length;
            }
        }
        #endregion
    }
}
