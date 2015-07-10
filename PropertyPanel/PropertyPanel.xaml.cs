/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Collections.ObjectModel;

namespace CrazyStorm
{
    /// <summary>
    /// PropertyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PropertyPanel : UserControl
    {
        CommandStack commandStack;
        Component component;
        Action updateFunc;
        public PropertyPanel(CommandStack commandStack, Component component, Action updateFunc)
        {
            this.commandStack = commandStack;
            this.component = component;
            this.updateFunc = updateFunc;
            InitializeComponent();
           
            var componentProperties = new ObservableCollection<PropertyPanelItem>();
            var componentList = component.InitializeProperties(typeof(Component));
            foreach (var item in componentList)
            {
                var property = new PropertyPanelItem() {
                    Info = item,
                    Name = item.Name,
                    Value = component.Properties[item]
                };
                componentProperties.Add(property);
            }
            ComponentGrid.DataContext = componentProperties;

            var specificProperties = new ObservableCollection<PropertyPanelItem>();
            var specificList = component.InitializeProperties(component.GetType());
            foreach (var item in specificList)
            {
                var property = new PropertyPanelItem()
                {
                    Info = item,
                    Name = item.Name,
                    Value = component.Properties[item]
                };
                specificProperties.Add(property);
            }
            SpecificGrid.DataContext = specificProperties;
            
            //var particleProperties = new ObservableCollection<Property>();
            //var particleList = multiEmitter.Particle.InitializeProperties(typeof(Particle));
            //foreach (var item in particleList)
            //{
            //    var property = new Property()
            //    {
            //        Info = item,
            //        Name = item.Name,
            //        Value = multiEmitter.Particle.Properties[item]
            //    };
            //    particleProperties.Add(property);
            //}
            //ParticleGrid.DataContext = particleProperties;
            //member3.Add(new Property() { Name = "生命", Value = "" });
            //member3.Add(new Property() { Name = "类型", Value = "" });
            //member3.Add(new Property() { Name = "宽度比", Value = "" });
            //member3.Add(new Property() { Name = "高度比", Value = "" });
            //member3.Add(new Property() { Name = "RGB", Value = "" });
            //member3.Add(new Property() { Name = "不透明度", Value = "" });
            //member3.Add(new Property() { Name = "旋转", Value = "" });
            //member3.Add(new Property() { Name = "旋转速度方向相关", Value = "" });
            //member3.Add(new Property() { Name = "旋转保持比例", Value = "" });
            //member3.Add(new Property() { Name = "速度", Value = "" });
            //member3.Add(new Property() { Name = "速度角", Value = "" });
            //member3.Add(new Property() { Name = "加速度", Value = "" });
            //member3.Add(new Property() { Name = "加速度角", Value = "" });
            //member3.Add(new Property() { Name = "雾化效果", Value = "" });
            //member3.Add(new Property() { Name = "消隐效果", Value = "" });
            //member3.Add(new Property() { Name = "拖影效果", Value = "" });
            //member3.Add(new Property() { Name = "混合模式", Value = "" });
            //member3.Add(new Property() { Name = "出屏即消", Value = "" });
            //member3.Add(new Property() { Name = "开启碰撞", Value = "" });
            //member3.Add(new Property() { Name = "不受遮罩影响", Value = "" });
            //member3.Add(new Property() { Name = "不受反弹板影响", Value = "" });
            //member3.Add(new Property() { Name = "不受力场影响", Value = "" });
        }

        void SetProperty(PropertyContainer container, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var property = e.Row.Item as PropertyPanelItem;
                var presenter = VisualHelper.GetVisualChild<DataGridCellsPresenter>(e.Row);
                var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(1);
                var newValue = (e.EditingElement as TextBox).Text;
                if (property.Value == newValue)
                    return;
                var attribute = property.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                new SetPropertyCommand().Do(commandStack, container, property, cell, newValue, attribute, updateFunc);
            }
        }

        public void UpdateProperty()
        {
            var componentProperties = ComponentGrid.DataContext as ObservableCollection<PropertyPanelItem>;
            foreach (var item in componentProperties)
            {
                var result = item.Info.GetGetMethod().Invoke(component, null).ToString();
                component.Properties[item.Info] = result;
                item.Value = component.Properties[item.Info];
            }
            var specificProperties = SpecificGrid.DataContext as ObservableCollection<PropertyPanelItem>;
            foreach (var item in specificProperties)
            {
                var result = item.Info.GetGetMethod().Invoke(component, null).ToString();
                component.Properties[item.Info] = result;
                item.Value = component.Properties[item.Info];
            }
            //
        }

        private void ComponentGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            SetProperty(component, e);
        }

        private void SpecificGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            SetProperty(component, e);
        }

        private void ParticleGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //PropertyPanelHelper.UpdateProperty(component.Particle, e);
        }
    }
}
