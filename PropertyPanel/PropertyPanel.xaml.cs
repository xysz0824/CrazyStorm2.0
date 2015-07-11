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
           //Load component properties.
            var componentProperties = new ObservableCollection<PropertyPanelItem>();
            var componentList = component.InitializeProperties(typeof(Component));
            foreach (var item in componentList)
            {
                var property = new PropertyPanelItem() {
                    Info = item,
                    Name = item.Name,
                    Value = component.Properties[item].Value
                };
                componentProperties.Add(property);
            }
            ComponentGrid.DataContext = componentProperties;
            //Load specific properties.
            var specificProperties = new ObservableCollection<PropertyPanelItem>();
            IList<PropertyInfo> specificList;
            if (component is Emitter)
                specificList = component.InitializeProperties(typeof(Emitter));
            else
                specificList = component.InitializeProperties(component.GetType());

            foreach (var item in specificList)
            {
                var property = new PropertyPanelItem()
                {
                    Info = item,
                    Name = item.Name,
                    Value = component.Properties[item].Value
                };
                specificProperties.Add(property);
            }
            SpecificGrid.DataContext = specificProperties;
            //Load particle properties.
            if (component is MultiEmitter)
            {
                ParticleGroup.Visibility = Visibility.Visible;
                var particleProperties = new ObservableCollection<PropertyPanelItem>();
                var particleList = (component as MultiEmitter).Particle.InitializeProperties(typeof(Particle));
                foreach (var item in particleList)
                {
                    var property = new PropertyPanelItem()
                    {
                        Info = item,
                        Name = item.Name,
                        Value = (component as MultiEmitter).Particle.Properties[item].Value
                    };
                    particleProperties.Add(property);
                }
                ParticleGrid.DataContext = particleProperties;
            }
            else if (component is CurveEmitter)
            {
                ParticleGroup.Visibility = Visibility.Visible;
                var particleProperties = new ObservableCollection<PropertyPanelItem>();
                var particleList = (component as CurveEmitter).CurveParticle.InitializeProperties(typeof(CurveParticle));
                foreach (var item in particleList)
                {
                    var property = new PropertyPanelItem()
                    {
                        Info = item,
                        Name = item.Name,
                        Value = (component as CurveEmitter).CurveParticle.Properties[item].Value
                    };
                    particleProperties.Add(property);
                }
                ParticleGrid.DataContext = particleProperties;
            }
 
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
            //Update component property.
            var componentProperties = ComponentGrid.DataContext as ObservableCollection<PropertyPanelItem>;
            foreach (var item in componentProperties)
            {
                var result = item.Info.GetGetMethod().Invoke(component, null).ToString();
                component.Properties[item.Info].Value = result;
                item.Value = component.Properties[item.Info].Value;
            }
            //Update specific property.
            var specificProperties = SpecificGrid.DataContext as ObservableCollection<PropertyPanelItem>;
            foreach (var item in specificProperties)
            {
                var result = item.Info.GetGetMethod().Invoke(component, null).ToString();
                component.Properties[item.Info].Value = result;
                item.Value = component.Properties[item.Info].Value;
            }
            //Update particle property.
            var particleProperties = ParticleGrid.DataContext as ObservableCollection<PropertyPanelItem>;
            if (component is MultiEmitter)
            {
                var particle = (component as MultiEmitter).Particle;
                foreach (var item in particleProperties)
                {
                    var result = item.Info.GetGetMethod().Invoke(particle, null).ToString();
                    particle.Properties[item.Info].Value = result;
                    item.Value = particle.Properties[item.Info].Value;
                }
            }
            else if (component is CurveEmitter)
            {
                var curveParticle = (component as CurveEmitter).CurveParticle;
                foreach (var item in particleProperties)
                {
                    var result = item.Info.GetGetMethod().Invoke(curveParticle, null).ToString();
                    curveParticle.Properties[item.Info].Value = result;
                    item.Value = curveParticle.Properties[item.Info].Value;
                }
            }
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
            if (component is MultiEmitter)
                SetProperty((component as MultiEmitter).Particle, e);
            else if (component is CurveEmitter)
                SetProperty((component as CurveEmitter).CurveParticle, e);
        }
    }
}
