/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        Point lastMouseDown;
        DependencyObject lastSelectedItem;
        Point propertyTabDragMouseDown;
        TabItem draggingPropertyTab;
        bool propertyTabDragStarted;
        Window draggingPropertyWindow;
        DispatcherTimer floatingPropertyWindowDragTimer;
        DateTime floatingPropertyWindowLastMoveTime;
        Dictionary<Component, Window> floatingPropertyWindows = new Dictionary<Component, Window>();
        HashSet<Window> dockingPropertyWindows = new HashSet<Window>();
        #endregion

        #region Private Methods
        bool IsPropertyTab(TabItem item)
        {
            return item != null && item.DataContext is Component && item.Content is ScrollViewer;
        }
        void AttachPropertyTabHandlers(TabItem item)
        {
            if (!IsPropertyTab(item)) return;

            item.PreviewMouseLeftButtonDown += PropertyTab_PreviewMouseLeftButtonDown;
            item.PreviewMouseMove += PropertyTab_PreviewMouseMove;
            item.PreviewMouseLeftButtonUp += PropertyTab_PreviewMouseLeftButtonUp;
            item.LostMouseCapture += PropertyTab_LostMouseCapture;
        }
        void DetachPropertyTabHandlers(TabItem item)
        {
            if (item == null) return;

            item.PreviewMouseLeftButtonDown -= PropertyTab_PreviewMouseLeftButtonDown;
            item.PreviewMouseMove -= PropertyTab_PreviewMouseMove;
            item.PreviewMouseLeftButtonUp -= PropertyTab_PreviewMouseLeftButtonUp;
            item.LostMouseCapture -= PropertyTab_LostMouseCapture;
        }
        void ResetPropertyTabDragState()
        {
            var item = draggingPropertyTab;
            draggingPropertyTab = null;
            propertyTabDragStarted = false;
            propertyTabDragMouseDown = new Point();

            if (item != null && item.IsMouseCaptured) item.ReleaseMouseCapture();
        }
        Point ConvertScreenPointToWindowPosition(Point screenPoint)
        {
            var source = PresentationSource.FromVisual(this);
            if (source == null || source.CompositionTarget == null) return screenPoint;

            return source.CompositionTarget.TransformFromDevice.Transform(screenPoint);
        }
        Rect GetWorkAreaBounds(Point screenPoint)
        {
            var screen = System.Windows.Forms.Screen.FromPoint(
                new System.Drawing.Point((int)Math.Round(screenPoint.X), (int)Math.Round(screenPoint.Y)));
            var area = screen.WorkingArea;
            var topLeft = ConvertScreenPointToWindowPosition(new Point(area.Left, area.Top));
            var bottomRight = ConvertScreenPointToWindowPosition(new Point(area.Right, area.Bottom));
            return new Rect(topLeft, bottomRight);
        }
        bool TryGetPropertyTabDropBounds(out Rect bounds)
        {
            bounds = Rect.Empty;
            if (LeftTabControl == null || !LeftTabControl.IsVisible) return false;

            LeftTabControl.UpdateLayout();
            var tabPanel = VisualHelper.VisualDownwardSearch<TabPanel>(LeftTabControl) as FrameworkElement;
            var dropTarget = tabPanel ?? (FrameworkElement)LeftTabControl;
            if (dropTarget == null || !dropTarget.IsVisible || dropTarget.ActualWidth <= 0 || dropTarget.ActualHeight <= 0)
            {
                return false;
            }

            var topLeft = dropTarget.PointToScreen(new Point(0, 0));
            var bottomRight = dropTarget.PointToScreen(new Point(dropTarget.ActualWidth, dropTarget.ActualHeight));
            bounds = new Rect(topLeft, bottomRight);
            bounds.Inflate(10, 10);
            return true;
        }
        bool IsPointInPropertyTabDropBounds(Point screenPoint)
        {
            Rect bounds;
            if (!TryGetPropertyTabDropBounds(out bounds)) return false;

            return bounds.Contains(screenPoint);
        }
        bool IsFloatingPropertyWindowInPropertyTabDropBounds(Window window)
        {
            if (window == null || !window.IsVisible) return false;

            if (LeftTabControl == null || !LeftTabControl.IsVisible) return false;
            LeftTabControl.UpdateLayout();

            var dropTarget = LeftTabControl as FrameworkElement;
            if (dropTarget == null || !dropTarget.IsVisible || dropTarget.ActualWidth <= 0 || dropTarget.ActualHeight <= 0)
            {
                return false;
            }

            var topLeft = dropTarget.PointToScreen(new Point(0, 0));
            var bottomRight = dropTarget.PointToScreen(new Point(dropTarget.ActualWidth, dropTarget.ActualHeight));
            var bounds = new Rect(topLeft, bottomRight);
            bounds.Inflate(10, 10);

            var width = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
            var height = window.ActualHeight > 0 ? window.ActualHeight : window.Height;
            if (double.IsNaN(width) || double.IsNaN(height) || width <= 0 || height <= 0)
            {
                return false;
            }

            var titleHeight = Math.Min(height, Math.Max(28, SystemParameters.WindowCaptionHeight + 12));
            var anchorY = Math.Min(height - 1, Math.Max(0, titleHeight * 0.5));
            var anchorXs = new[]
            {
                Math.Min(width - 1, Math.Max(0, 24)),
                Math.Min(width - 1, Math.Max(0, Math.Min(width * 0.35, 100))),
                Math.Min(width - 1, Math.Max(0, Math.Min(width * 0.5, 140))),
            };
            foreach (var anchorX in anchorXs)
            {
                var anchorPoint = window.PointToScreen(new Point(anchorX, anchorY));
                if (bounds.Contains(anchorPoint)) return true;
            }
            return false;
        }
        ScrollViewer CreatePropertyPanelScroll(Component component)
        {
            var scroll = new ScrollViewer();
            var baseScrollBarStyle = (Style)Application.Current.FindResource("FlatScrollBarStyle");
            var implicitScrollBarStyle = new Style(typeof(ScrollBar), baseScrollBarStyle);
            scroll.Resources.Add(typeof(ScrollBar), implicitScrollBarStyle);
            var trackBrush = (Brush)Application.Current.FindResource("ScrollBarTrackBrush");
            scroll.Resources.Add(SystemColors.ControlBrushKey, trackBrush);
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            var particleTypes = new List<ParticleType>();
            particleTypes.AddRange(ParticleType.DefaultTypes);
            particleTypes.AddRange(selectedSystem.CustomTypes);
            var panel = new PropertyPanel(commandStacks[selectedSystem], config, file,
                particleTypes, component, UpdateProperty);
            scroll.Content = panel;
            panel.OnBeginEditing += () => editingProperties = true;
            panel.OnEndEditing += () => editingProperties = false;
            return scroll;
        }
        TabItem CreatePropertyTabItem(Component component, ScrollViewer scroll)
        {
            var item = new TabItem();
            item.DataContext = component;
            item.Style = (Style)FindResource("CanCloseStyle");
            item.Content = scroll;
            AttachPropertyTabHandlers(item);
            return item;
        }
        Window CreateFloatingPropertyWindow(Component component, ScrollViewer scroll, Point screenPoint)
        {
            var window = new Window();
            window.DataContext = component;
            window.Owner = this;
            window.ShowInTaskbar = false;
            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.ResizeMode = ResizeMode.CanResize;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.MinWidth = 320;
            window.MinHeight = 300;
            window.Width = Math.Max(360, LeftTabControl.ActualWidth);
            var workArea = GetWorkAreaBounds(screenPoint);
            window.Height = Math.Max(window.MinHeight, workArea.Height);
            var windowPosition = ConvertScreenPointToWindowPosition(screenPoint);
            window.Left = Math.Max(workArea.Left, Math.Min(windowPosition.X, Math.Max(workArea.Left, workArea.Right - window.Width)));
            window.Top = workArea.Top;
            window.Content = scroll;
            window.SetBinding(Window.TitleProperty, new Binding("Name"));
            window.LocationChanged += FloatingPropertyWindow_LocationChanged;
            window.Closed += FloatingPropertyWindow_Closed;
            return window;
        }
        void EnsureFloatingPropertyWindowDragTimer()
        {
            if (floatingPropertyWindowDragTimer == null)
            {
                floatingPropertyWindowDragTimer = new DispatcherTimer();
                floatingPropertyWindowDragTimer.Interval = TimeSpan.FromMilliseconds(40);
                floatingPropertyWindowDragTimer.Tick += FloatingPropertyWindowDragTimer_Tick;
            }
            if (!floatingPropertyWindowDragTimer.IsEnabled) floatingPropertyWindowDragTimer.Start();
        }
        void UpdateFloatingPropertyWindowDragTimerState()
        {
            if (floatingPropertyWindowDragTimer == null) return;

            if (floatingPropertyWindows.Count == 0) floatingPropertyWindowDragTimer.Stop();
            else if (!floatingPropertyWindowDragTimer.IsEnabled) floatingPropertyWindowDragTimer.Start();
        }
        void FloatPropertyTab(TabItem item, Point screenPoint)
        {
            if (!IsPropertyTab(item)) return;
            if (!LeftTabControl.Items.Contains(item)) return;

            var component = item.DataContext as Component;
            if (component == null || floatingPropertyWindows.ContainsKey(component)) return;

            var scroll = item.Content as ScrollViewer;
            DetachPropertyTabHandlers(item);
            item.Content = null;
            LeftTabControl.Items.Remove(item);
            ResetLeftTab();

            var window = CreateFloatingPropertyWindow(component, scroll, screenPoint);
            floatingPropertyWindows[component] = window;
            EnsureFloatingPropertyWindowDragTimer();
            window.Show();
            window.Activate();
        }
        void ReDockPropertyWindow(Window window)
        {
            if (window == null || dockingPropertyWindows.Contains(window)) return;

            var component = window.DataContext as Component;
            var scroll = window.Content as ScrollViewer;
            if (component == null || scroll == null) return;

            dockingPropertyWindows.Add(window);
            window.Content = null;

            var item = CreatePropertyTabItem(component, scroll);
            LeftTabControl.Items.Add(item);
            LeftTabControl.SelectedItem = item;
            item.Focus();

            window.Close();
        }
        void CleanupFloatingPropertyWindow(Window window)
        {
            if (window == null) return;

            if (draggingPropertyWindow == window)
                draggingPropertyWindow = null;

            window.LocationChanged -= FloatingPropertyWindow_LocationChanged;
            window.Closed -= FloatingPropertyWindow_Closed;

            dockingPropertyWindows.Remove(window);

            Window mappedWindow;
            var component = window.DataContext as Component;
            if (component != null && floatingPropertyWindows.TryGetValue(component, out mappedWindow) && mappedWindow == window)
                floatingPropertyWindows.Remove(component);

            UpdateFloatingPropertyWindowDragTimerState();
        }
        void CloseFloatingPropertyWindow(Window window)
        {
            if (window == null) return;
            window.Close();
        }
        void UpdateSelectedGroup()
        {
            //Get all visible components in this particle system.
            var set = new List<Component>();
            foreach (var layer in selectedSystem.Layers)
                if (layer.Visible)
                    set.AddRange(layer.Components);
            //Clean removed component.
            for (int i = 0; i < selectedComponents.Count; ++i)
                if (!set.Contains(selectedComponents[i]))
                {
                    selectedComponents.RemoveAt(i);
                    i--;
                }
            //Update selected group.
            StatusText.Content = "";
            if (selectedComponents.Count > 0)
            {
                SelectedGroup.Opacity = 1;
                if (selectedComponents.Count == 1)
                {
                    var component = selectedComponents.First();
                    SelectedGroupType.Text = ExpressionHelper.FindTranslation(component.GetType().Name);
                    SelectedGroupName.DataContext = component;
                    SelectedGroupName.SetBinding(TextBlock.TextProperty, "Name");
                    SelectedGroupTip.Text = (string)FindResource("DoubleClickTipStr");
                    SelectedGroupIconPath.Data = (Geometry)FindResource($"{component.GetType().Name}_Icon");
                    var scale = (double)FindResource($"{component.GetType().Name}_Scale");
                    var transform = new ScaleTransform(scale, scale);
                    SelectedGroupIconPath.RenderTransform = transform;
                    if (selectedComponents[0].BindingTarget == null)
                    {
                        var layerBeginFrame = selectedSystem.Layers.First((l) => l.Components.Contains(selectedComponents[0])).BeginFrame;
                        StatusText.Content = string.Format((string)FindResource("ComponentLifeTimeStr"),
                        selectedComponents[0].BeginFrame + layerBeginFrame - 1,
                        selectedComponents[0].BeginFrame + layerBeginFrame - 1 + selectedComponents[0].TotalFrame - 1);
                    }
                    else
                    {
                        StatusText.Content = string.Format((string)FindResource("BindingTargetTipStr"),
                            selectedComponents[0].BindingTarget.Name);
                    }
                }
                else
                {
                    SelectedGroupIconPath.Data = (Geometry)FindResource($"Group_Icon");
                    var scale = (double)FindResource("Group_Scale");
                    var transform = new ScaleTransform(scale, scale);
                    SelectedGroupIconPath.RenderTransform = transform;
                    SelectedGroupType.Text = "Group";
                    SelectedGroupName.Text = selectedComponents.Count + (string)FindResource("ComponentUnitStr");
                    SelectedGroupTip.Text = string.Empty;
                }
            }
            else
                SelectedGroup.Opacity = 0;
        }
        void CreatePropertyPanel(Component component)
        {
            TabItem item;
            Window floatingWindow;
            //Prevent from repeating tab of components.  
            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                item = LeftTabControl.Items[i] as TabItem;
                if (item.DataContext == component)
                {
                    LeftTabControl.SelectedItem = item;
                    item.Focus();
                    return;
                }
            }
            if (floatingPropertyWindows.TryGetValue(component, out floatingWindow))
            {
                if (floatingWindow.WindowState == WindowState.Minimized) floatingWindow.WindowState = WindowState.Normal;
                floatingWindow.Activate();
                return;
            }

            item = CreatePropertyTabItem(component, CreatePropertyPanelScroll(component));
            LeftTabControl.Items.Add(item);
            LeftTabControl.SelectedItem = item;
            item.Focus();
            saved = false;
        }
        void UpdateProperty()
        {
            foreach (TabItem item in LeftTabControl.Items)
            {
                var scroll = item.Content as ScrollViewer;
                if (scroll != null)
                {
                    var content = scroll.Content as PropertyPanel;
                    if (content != null) content.UpdateProperty();
                }
            }
            foreach (var window in floatingPropertyWindows.Values)
            {
                var scroll = window.Content as ScrollViewer;
                if (scroll != null)
                {
                    var content = scroll.Content as PropertyPanel;
                    if (content != null) content.UpdateProperty();
                }
            }
            UpdateScreen();
        }
        void UpdateComponentMenu()
        {
            //Enable binding if selected one or more.
            BindComponentItem.IsEnabled = selectedComponents != null && selectedComponents.Count > 0;
            UnbindComponentItem.IsEnabled = BindComponentItem.IsEnabled;
            BindButton.IsEnabled = BindComponentItem.IsEnabled;
            UnbindButton.IsEnabled = UnbindComponentItem.IsEnabled;
        }
        void UpdateComponentPanels()
        {
            //Get all visible components in this particle system.
            var set = new List<Component>();
            foreach (var layer in selectedSystem.Layers)
            {
                if (layer.Visible) set.AddRange(layer.Components);
            }
            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                TabItem item = LeftTabControl.Items[i] as TabItem;
                //Remove the property panel which is not belonging to any component.
                if (item.DataContext is Component && !set.Contains(item.DataContext))
                {
                    DetachPropertyTabHandlers(item);
                    LeftTabControl.Items.RemoveAt(i);
                    i--;
                }
                //Update finder panel
                if (item.Content is FinderPanel)
                    (item.Content as FinderPanel).Update(selectedSystem);
            }

            var windowsToClose = new List<Window>();
            foreach (var pair in floatingPropertyWindows)
            {
                if (!set.Contains(pair.Key)) windowsToClose.Add(pair.Value);
            }
            foreach (var window in windowsToClose) CloseFloatingPropertyWindow(window);
        }
        void ResetLeftTab()
        {
            var hasComponentPanel = false;
            foreach (var item in LeftTabControl.Items)
            {
                var tabItem = item as TabItem;
                if (tabItem != null && tabItem.DataContext != null && tabItem.DataContext is Component)
                {
                    hasComponentPanel = true;
                    LeftTabControl.SelectedItem = tabItem;
                }
            }
            if (!hasComponentPanel) LeftTabControl.SelectedIndex = 0;
        }
        void BindComponent()
        {
            var center = new Point(config.ScreenWidthOver2, config.ScreenHeightOver2);
            bindingLines = new List<Line>();
            foreach (var component in selectedComponents)
            {
                //If component has parent, caculate the absolute position.
                float x = component.X;
                float y = component.Y;
                if (component.Parent != null)
                {
                    Vector2 parent = component.Parent.GetAbsolutePosition();
                    x += parent.x;
                    y += parent.y;
                }
                var line = DrawHelper.GetLine((int)(x + center.X), (int)(y + center.Y),
                    (int)screenMousePos.X, (int)screenMousePos.Y, 2, true, Colors.White, 0.5f);
                line.DataContext = component;
                bindingLines.Add(line);
            }
            UpdateScreen();
        }
        void UnbindComponent()
        {
            if (selectedComponents.Count > 0)
            {
                foreach (var component in selectedComponents)
                {
                    if (component.BindingTarget != null)
                    {
                        new UnbindComponentCommand().Do(commandStacks[selectedSystem], selectedComponents);
                        UpdateScreen();
                        UpdateSelectedGroup();
                        break;
                    }
                }
            }
        }
        #endregion

        #region Window EventHandlers
        private void ComponentButton_Click(object sender, RoutedEventArgs e)
        {
            //Create corresponding component according to different button.
            var button = sender as Button;
            aimRect = VisualHelper.VisualDownwardSearch((DependencyObject)ParticleTabControl.SelectedContent, "AimBox");
            aimRect.SetValue(OpacityProperty, 1.0d);
            aimComponent = ComponentFactory.Create(button.Name);
            aimComponent.Globals = file.Globals;
            var emitter = aimComponent as Emitter;
            if (emitter != null)
            {
                emitter.InitialTemplate.Type = ParticleType.DefaultTypes.First();
            }
        }
        private void ComponentTree_GotFocus(object sender, RoutedEventArgs e)
        {
            ComponentTree_SelectedItemChanged(null, null);
        }
        private void ComponentTree_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            lastMouseDown = e.GetPosition(ComponentTree);
            lastSelectedItem = sender as DependencyObject;
            //Select pointed component when mouse left-button down. 
            var textBlock = sender as TextBlock;
            var set = new List<CrazyStorm.Core.Component>();
            set.Add((Component)textBlock.DataContext);
            SelectComponents(set, true);
        }
        private void ComponentTree_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(ComponentTree);
                if ((Math.Abs(currentPosition.X - lastMouseDown.X) > 2.0) || 
                    (Math.Abs(currentPosition.Y - lastMouseDown.Y) > 2.0))
                {
                    if (lastSelectedItem != null)
                        DragDrop.DoDragDrop(lastSelectedItem, sender, DragDropEffects.Move);
                }
            }
            else
            {
                lastSelectedItem = null;
            }
        }
        private void ComponentTree_CheckDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
        private void ComponentTree_Drop(object sender, DragEventArgs e)
        {
            var sourceComponent = ((TextBlock)lastSelectedItem).DataContext as Component;
            if (!(e.OriginalSource is TextBlock)) return;

            var targetComponent = ((TextBlock)e.OriginalSource).DataContext as Component;
            if (sourceComponent == targetComponent) return;

            if (sourceComponent.Children.Contains(targetComponent)) return;

            new ComponentTreeCommand().Do(commandStacks[selectedSystem],
                selectedSystem, sourceComponent, targetComponent, new Action(UpdateProperty));
        }
        private void ComponentTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ComponentTree.SelectedItem == null) return;
            var set = new List<CrazyStorm.Core.Component>();
            set.Add(ComponentTree.SelectedItem as Component);
            SelectComponents(set, true);
        }
        private void ComponentTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock && ComponentTree.SelectedItem != null)
            {
                CreatePropertyPanel(ComponentTree.SelectedItem as Component);
            }
        }
        private void PropertyTab_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (VisualHelper.VisualUpwardSearch<Button>(e.OriginalSource as DependencyObject) != null) return;

            var tabItem = sender as TabItem;
            if (!IsPropertyTab(tabItem)) return;

            // Only allow tab dragging from the header area, not from PropertyPanel content.
            var point = e.GetPosition(tabItem);
            var headerBounds = new Rect(0, 0, tabItem.ActualWidth, tabItem.ActualHeight);
            if (!headerBounds.Contains(point)) return;

            draggingPropertyTab = tabItem;
            propertyTabDragMouseDown = e.GetPosition(this);
            propertyTabDragStarted = false;
        }
        private void PropertyTab_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var tabItem = sender as TabItem;
            if (tabItem == null || tabItem != draggingPropertyTab) return;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                ResetPropertyTabDragState();
                return;
            }

            var current = e.GetPosition(this);
            if (!propertyTabDragStarted)
            {
                if (Math.Abs(current.X - propertyTabDragMouseDown.X) < SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(current.Y - propertyTabDragMouseDown.Y) < SystemParameters.MinimumVerticalDragDistance)
                {
                    return;
                }

                propertyTabDragStarted = true;
                tabItem.CaptureMouse();
            }
        }
        private void PropertyTab_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var screenPoint = PointToScreen(e.GetPosition(this));
            var tabItem = sender as TabItem;
            var shouldFloat = tabItem != null &&
                tabItem == draggingPropertyTab &&
                propertyTabDragStarted &&
                !IsPointInPropertyTabDropBounds(screenPoint);

            ResetPropertyTabDragState();

            if (shouldFloat) FloatPropertyTab(tabItem, screenPoint);
        }
        private void PropertyTab_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (sender == draggingPropertyTab) ResetPropertyTabDragState();
        }
        private void FloatingPropertyWindow_LocationChanged(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window == null || dockingPropertyWindows.Contains(window)) return;

            draggingPropertyWindow = window;
            floatingPropertyWindowLastMoveTime = DateTime.Now;
            EnsureFloatingPropertyWindowDragTimer();
        }
        private void FloatingPropertyWindowDragTimer_Tick(object sender, EventArgs e)
        {
            if (floatingPropertyWindows.Count == 0)
            {
                UpdateFloatingPropertyWindowDragTimerState();
                draggingPropertyWindow = null;
                return;
            }

            var window = draggingPropertyWindow;
            if (window == null || dockingPropertyWindows.Contains(window)) return;

            if ((DateTime.Now - floatingPropertyWindowLastMoveTime).TotalMilliseconds < 120) return;

            draggingPropertyWindow = null;

            if (IsFloatingPropertyWindowInPropertyTabDropBounds(window)) ReDockPropertyWindow(window);
        }
        private void FloatingPropertyWindow_Closed(object sender, EventArgs e)
        {
            CleanupFloatingPropertyWindow(sender as Window);
        }
        private void TabClose_Click(object sender, RoutedEventArgs e)
        {
            var tabItem = VisualHelper.VisualUpwardSearch<TabItem>(sender as DependencyObject) as TabItem;
            if (tabItem == draggingPropertyTab) ResetPropertyTabDragState();
            DetachPropertyTabHandlers(tabItem);
            LeftTabControl.Items.Remove(tabItem);
            ResetLeftTab();
        }
        private void BindComponentItem_Click(object sender, RoutedEventArgs e)
        {
            BindComponent();
        }
        private void UnbindComponentItem_Click(object sender, RoutedEventArgs e)
        {
            UnbindComponent();
        }
        #endregion
    }
}
