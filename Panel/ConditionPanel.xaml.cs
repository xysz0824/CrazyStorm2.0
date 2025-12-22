/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
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
using CrazyStorm.Core;
using CrazyStorm.Expression;
using Environment = CrazyStorm.Expression.Environment;

namespace CrazyStorm
{
    public partial class ConditionPanel : UserControl
    {
        #region Private Members
        Popup popup;
        #endregion
        #region Public Members
        public Environment Environment { get; set; }
        #endregion

        #region Constructor
        public ConditionPanel()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Methods
        public void Reset()
        {
            LeftConditionComboBox.SelectedIndex = -1;
            LeftValue.Text = string.Empty;
            And.IsChecked = true;
            Or.IsChecked = false;
            RightConditionComboBox.SelectedIndex = -1;
            RightValue.Text = string.Empty;
        }
        public void AddConditionVariable(VariableComboBoxItem item)
        {
            LeftConditionComboBox.Items.Add(item);
            RightConditionComboBox.Items.Add(item);
        }
        public bool SetConditionInfo(Environment env, EventInfo eventInfo)
        {
            //Check if there have errors
            if (LeftValue.ToolTip != null || RightValue.ToolTip != null) return false;
            if (LeftLessThan.IsChecked == true && LeftEqual.IsChecked == true) eventInfo.leftOperator = "<=";
            else if (LeftMoreThan.IsChecked == true && LeftEqual.IsChecked == true) eventInfo.leftOperator = ">=";
            else if (LeftLessThan.IsChecked == true && LeftMoreThan.IsChecked == true) eventInfo.leftOperator = "!=";
            else if (LeftLessThan.IsChecked == true) eventInfo.leftOperator = "<";
            else if (LeftEqual.IsChecked == true) eventInfo.leftOperator = "=";
            else if (LeftMoreThan.IsChecked == true) eventInfo.leftOperator = ">";
            if (LeftConditionComboBox.SelectedItem != null && !String.IsNullOrEmpty(LeftValue.Text) &&
                !String.IsNullOrEmpty(eventInfo.leftOperator))
            {
                var selectedItem = LeftConditionComboBox.SelectedItem as VariableComboBoxItem;
                eventInfo.leftProperty = selectedItem.Name;
                eventInfo.leftType = env.GetValueType(selectedItem.Name);
                eventInfo.leftValue = LeftValue.Text;
            }
            if (RightLessThan.IsChecked == true && RightEqual.IsChecked == true) eventInfo.rightOperator = "<=";
            else if (RightMoreThan.IsChecked == true && RightEqual.IsChecked == true) eventInfo.rightOperator = ">=";
            else if (RightLessThan.IsChecked == true && RightMoreThan.IsChecked == true) eventInfo.rightOperator = "!=";
            else if (RightLessThan.IsChecked == true) eventInfo.rightOperator = "<";
            else if (RightEqual.IsChecked == true) eventInfo.rightOperator = "=";
            else if (RightMoreThan.IsChecked == true) eventInfo.rightOperator = ">";
            if (RightConditionComboBox.SelectedItem != null && !String.IsNullOrEmpty(RightValue.Text) &&
                !String.IsNullOrEmpty(eventInfo.rightOperator))
            {
                var selectedItem = RightConditionComboBox.SelectedItem as VariableComboBoxItem;
                eventInfo.rightProperty = selectedItem.Name;
                eventInfo.rightType = env.GetValueType(selectedItem.Name);
                eventInfo.rightValue = RightValue.Text;
            }
            //Allow empty condition
            if (String.IsNullOrEmpty(eventInfo.leftProperty) && String.IsNullOrEmpty(eventInfo.rightProperty))
            {
                return true;
            }
            if (String.IsNullOrEmpty(eventInfo.leftProperty) && !String.IsNullOrEmpty(eventInfo.rightProperty))
            {
                eventInfo.leftProperty = eventInfo.rightProperty;
                eventInfo.rightProperty = null;
                eventInfo.leftOperator = eventInfo.rightOperator;
                eventInfo.rightOperator = null;
                eventInfo.leftType = eventInfo.rightType;
                eventInfo.rightType = PropertyType.IllegalType;
                eventInfo.leftValue = eventInfo.rightValue;
                eventInfo.rightValue = null;
            }
            if (!String.IsNullOrEmpty(eventInfo.leftProperty) && !String.IsNullOrEmpty(eventInfo.rightProperty))
            {
                if (And.IsChecked == true) eventInfo.midOperator = "&";
                else if (Or.IsChecked == true) eventInfo.midOperator = "|";
            }
            eventInfo.hasCondition = true;
            return true;
        }
        public void MapConditionInfo(EventInfo eventInfo)
        {
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
            if (eventInfo.hasCondition && eventInfo.rightProperty != null)
            {
                for (int i = 0; i < LeftConditionComboBox.Items.Count; ++i)
                {
                    var item = LeftConditionComboBox.Items[i] as VariableComboBoxItem;
                    if (item.Name == eventInfo.leftProperty)
                        LeftConditionComboBox.SelectedIndex = i;
                }
                foreach (var checkBox in checkBoxMap[eventInfo.leftOperator]) checkBox.IsChecked = true;
                LeftValue.Text = eventInfo.leftValue;
                foreach (var checkBox in checkBoxMap[eventInfo.midOperator]) checkBox.IsChecked = true;
                for (int i = 0; i < RightConditionComboBox.Items.Count; ++i)
                {
                    var item = RightConditionComboBox.Items[i] as VariableComboBoxItem;
                    if (item.Name == eventInfo.rightProperty)
                        RightConditionComboBox.SelectedIndex = i;
                }
                checkBoxMap[">"] = new[] { RightMoreThan };
                checkBoxMap["="] = new[] { RightEqual };
                checkBoxMap["<"] = new[] { RightLessThan };
                checkBoxMap[">="] = new[] { RightMoreThan, RightEqual };
                checkBoxMap["<="] = new[] { RightLessThan, RightEqual };
                checkBoxMap["!="] = new[] { RightLessThan, RightMoreThan };
                foreach (var checkBox in checkBoxMap[eventInfo.rightOperator]) checkBox.IsChecked = true;
                RightValue.Text = eventInfo.rightValue;
            }
            else
            {
                for (int i = 0; i < LeftConditionComboBox.Items.Count; ++i)
                {
                    var item = LeftConditionComboBox.Items[i] as VariableComboBoxItem;
                    if (item.Name == eventInfo.leftProperty)
                        LeftConditionComboBox.SelectedIndex = i;
                }
                foreach (var checkBox in checkBoxMap[eventInfo.leftOperator]) checkBox.IsChecked = true;
                LeftValue.Text = eventInfo.leftValue;
            }
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
            if (String.IsNullOrEmpty(input))
                return;

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
            if (String.IsNullOrEmpty(input))
                return;

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
        #endregion
    }
}
