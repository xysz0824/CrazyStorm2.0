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
        const double TabDragPreviewOpacity = 0.65;

        #region Private Members
        Point lastMouseDown;
        DependencyObject lastSelectedItem;
        Point tabDragMouseDown;
        Point tabDragPreviewMouseOffset;
        TabItem draggingPropertyTab;
        bool tabDragStarted;
        Popup tabDragPreviewPopup;
        ImageSource tabDragPreviewImageSource;
        Window draggingPropertyWindow;
        DispatcherTimer propertyWindowDragTimer;
        DateTime propertyWindowLastMoveTime;
        Dictionary<Component, Window> propertyWindows = new Dictionary<Component, Window>();
        HashSet<Window> dockingPropertyWindows = new HashSet<Window>();
        #endregion

        #region Private Methods
        bool IsPropertyTab(TabItem item)
        {
            return item != null && item.DataContext is Component && item.Content is ScrollViewer;
        }
        bool IsFinderTab(TabItem item)
        {
            return item != null && item.Content is FinderPanel;
        }
        bool IsSortableClosableTab(TabItem item)
        {
            return IsPropertyTab(item) || IsFinderTab(item);
        }
        void AttachPropertyTabHandlers(TabItem item)
        {
            if (!IsSortableClosableTab(item)) return;

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
        void ResetTabDragState()
        {
            var item = draggingPropertyTab;
            draggingPropertyTab = null;
            tabDragStarted = false;
            tabDragMouseDown = new Point();
            tabDragPreviewMouseOffset = new Point();
            tabDragPreviewImageSource = null;
            CloseTabDragPreview();

            if (item != null && item.IsMouseCaptured) item.ReleaseMouseCapture();
        }
        ImageSource CreateTabDragPreviewImageSource(TabItem item)
        {
            if (item == null) return null;

            item.UpdateLayout();
            var width = item.ActualWidth;
            var height = item.ActualHeight;
            if (width <= 0 || height <= 0) return null;

            var dpi = VisualTreeHelper.GetDpi(item);
            var pixelWidth = Math.Max(1, (int)Math.Round(width * dpi.DpiScaleX));
            var pixelHeight = Math.Max(1, (int)Math.Round(height * dpi.DpiScaleY));
            var dpiX = dpi.PixelsPerInchX;
            var dpiY = dpi.PixelsPerInchY;

            var bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Pbgra32);
            var brush = new VisualBrush(item) { Stretch = Stretch.Fill };
            var drawing = new DrawingVisual();
            using (var dc = drawing.RenderOpen())
            {
                dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
            }
            bitmap.Render(drawing);
            bitmap.Freeze();
            return bitmap;
        }
        void EnsureTabDragPreviewPopup()
        {
            if (tabDragPreviewPopup != null) return;

            tabDragPreviewPopup = new Popup();
            tabDragPreviewPopup.AllowsTransparency = true;
            tabDragPreviewPopup.Placement = PlacementMode.AbsolutePoint;
            tabDragPreviewPopup.StaysOpen = true;
            tabDragPreviewPopup.IsHitTestVisible = false;
        }
        void ShowTabDragPreview(TabItem item, Point screenPoint)
        {
            if (item == null) return;

            if (tabDragPreviewImageSource == null) return;

            EnsureTabDragPreviewPopup();

            var border = new Border();
            border.Background = Brushes.Transparent;
            border.Opacity = TabDragPreviewOpacity;
            border.SnapsToDevicePixels = true;
            border.IsHitTestVisible = false;
            border.Child = new Image
            {
                Source = tabDragPreviewImageSource,
                Width = item.ActualWidth,
                Height = item.ActualHeight,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            };

            tabDragPreviewPopup.Child = border;
            UpdateTabDragPreviewPosition(screenPoint);
            tabDragPreviewPopup.IsOpen = true;
        }
        void UpdateTabDragPreviewPosition(Point screenPoint)
        {
            if (tabDragPreviewPopup == null || !tabDragPreviewPopup.IsOpen) return;

            var dipScreenPoint = ConvertScreenPointToWindowPosition(screenPoint);
            tabDragPreviewPopup.HorizontalOffset = dipScreenPoint.X - tabDragPreviewMouseOffset.X;
            tabDragPreviewPopup.VerticalOffset = dipScreenPoint.Y - tabDragPreviewMouseOffset.Y;
        }
        void CloseTabDragPreview()
        {
            if (tabDragPreviewPopup == null) return;

            tabDragPreviewPopup.IsOpen = false;
            tabDragPreviewPopup.Child = null;
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
        TabItem GetSwapTabTargetAtScreenPoint(TabItem draggingTab, Point screenPoint)
        {
            if (draggingTab == null || LeftTabControl == null) return null;

            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                var item = LeftTabControl.Items[i] as TabItem;
                if (item == null || item == draggingTab || !item.IsVisible) continue;
                if (item.ActualWidth <= 0 || item.ActualHeight <= 0) continue;

                var topLeft = item.PointToScreen(new Point(0, 0));
                var bottomRight = item.PointToScreen(new Point(item.ActualWidth, item.ActualHeight));
                if (new Rect(topLeft, bottomRight).Contains(screenPoint)) return item;
            }
            return null;
        }
        void SwapTabs(TabItem left, TabItem right)
        {
            if (left == null || right == null || left == right || LeftTabControl == null) return;

            var leftIndex = LeftTabControl.Items.IndexOf(left);
            var rightIndex = LeftTabControl.Items.IndexOf(right);
            if (leftIndex < 2 || rightIndex < 2 || leftIndex == rightIndex) return;

            if (leftIndex < rightIndex)
            {
                LeftTabControl.Items.RemoveAt(rightIndex);
                LeftTabControl.Items.RemoveAt(leftIndex);
                LeftTabControl.Items.Insert(leftIndex, right);
                LeftTabControl.Items.Insert(rightIndex, left);
            }
            else
            {
                LeftTabControl.Items.RemoveAt(leftIndex);
                LeftTabControl.Items.RemoveAt(rightIndex);
                LeftTabControl.Items.Insert(rightIndex, left);
                LeftTabControl.Items.Insert(leftIndex, right);
            }

            LeftTabControl.SelectedItem = left;
            left.Focus();
        }
        bool IsPropertyWindowInPropertyTabDropBounds(Window window)
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
        Window CreatePropertyWindow(Component component, ScrollViewer scroll, Point screenPoint)
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
            window.LocationChanged += PropertyWindow_LocationChanged;
            window.Closed += PropertyWindow_Closed;
            return window;
        }
        void EnsurePropertyWindowDragTimer()
        {
            if (propertyWindowDragTimer == null)
            {
                propertyWindowDragTimer = new DispatcherTimer();
                propertyWindowDragTimer.Interval = TimeSpan.FromMilliseconds(40);
                propertyWindowDragTimer.Tick += PropertyWindowDragTimer_Tick;
            }
            if (!propertyWindowDragTimer.IsEnabled) propertyWindowDragTimer.Start();
        }
        void UpdatePropertyWindowDragTimerState()
        {
            if (propertyWindowDragTimer == null) return;

            if (propertyWindows.Count == 0) propertyWindowDragTimer.Stop();
            else if (!propertyWindowDragTimer.IsEnabled) propertyWindowDragTimer.Start();
        }
        void FloatPropertyTab(TabItem item, Point screenPoint)
        {
            if (!IsPropertyTab(item)) return;
            if (!LeftTabControl.Items.Contains(item)) return;

            var component = item.DataContext as Component;
            if (component == null || propertyWindows.ContainsKey(component)) return;

            var scroll = item.Content as ScrollViewer;
            DetachPropertyTabHandlers(item);
            item.Content = null;
            LeftTabControl.Items.Remove(item);
            ResetLeftTab();

            var window = CreatePropertyWindow(component, scroll, screenPoint);
            propertyWindows[component] = window;
            EnsurePropertyWindowDragTimer();
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
        void CleanupPropertyWindow(Window window)
        {
            if (window == null) return;

            if (draggingPropertyWindow == window)
                draggingPropertyWindow = null;

            window.LocationChanged -= PropertyWindow_LocationChanged;
            window.Closed -= PropertyWindow_Closed;

            dockingPropertyWindows.Remove(window);

            Window mappedWindow;
            var component = window.DataContext as Component;
            if (component != null && propertyWindows.TryGetValue(component, out mappedWindow) && mappedWindow == window)
                propertyWindows.Remove(component);

            UpdatePropertyWindowDragTimerState();
        }
        void ClosePropertyWindow(Window window)
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
            Window propertyWindow;
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
            if (propertyWindows.TryGetValue(component, out propertyWindow))
            {
                if (propertyWindow.WindowState == WindowState.Minimized) propertyWindow.WindowState = WindowState.Normal;
                propertyWindow.Activate();
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
            foreach (var window in propertyWindows.Values)
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
            foreach (var pair in propertyWindows)
            {
                if (!set.Contains(pair.Key)) windowsToClose.Add(pair.Value);
            }
            foreach (var window in windowsToClose) ClosePropertyWindow(window);
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
            var textBlock = sender as TextBlock;
            if (textBlock == null) return;
            var component = textBlock.DataContext as Component;
            if (component == null) return;

            // TreeView is single-select; handle Ctrl multi-select here and stop TreeView from selecting again.
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                var set = new List<CrazyStorm.Core.Component>();
                set.Add(component);
                SelectComponents(set, true);
                e.Handled = true;
                return;
            }

            // Clicking the same selected item will not raise SelectedItemChanged, so keep selection sync here.
            if (ComponentTree.SelectedItem == component)
            {
                var set = new List<CrazyStorm.Core.Component>();
                set.Add(component);
                SelectComponents(set, true);
            }
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
            if (!IsSortableClosableTab(tabItem)) return;

            // Only allow tab dragging from the header area, not from PropertyPanel content.
            var point = e.GetPosition(tabItem);
            var headerBounds = new Rect(0, 0, tabItem.ActualWidth, tabItem.ActualHeight);
            if (!headerBounds.Contains(point)) return;

            draggingPropertyTab = tabItem;
            tabDragMouseDown = e.GetPosition(this);
            tabDragPreviewMouseOffset = e.GetPosition(tabItem);
            tabDragStarted = false;
        }
        private void PropertyTab_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var tabItem = sender as TabItem;
            if (tabItem == null || tabItem != draggingPropertyTab) return;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                ResetTabDragState();
                return;
            }

            var current = e.GetPosition(this);
            if (!tabDragStarted)
            {
                if (Math.Abs(current.X - tabDragMouseDown.X) < SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(current.Y - tabDragMouseDown.Y) < SystemParameters.MinimumVerticalDragDistance)
                {
                    return;
                }

                tabDragStarted = true;
                tabItem.CaptureMouse();
                tabDragPreviewImageSource = CreateTabDragPreviewImageSource(tabItem);
                ShowTabDragPreview(tabItem, PointToScreen(e.GetPosition(this)));
            }

            UpdateTabDragPreviewPosition(PointToScreen(e.GetPosition(this)));
        }
        private void PropertyTab_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var screenPoint = PointToScreen(e.GetPosition(this));
            var tabItem = sender as TabItem;
            var swapTarget = tabItem != null &&
                tabItem == draggingPropertyTab &&
                tabDragStarted &&
                IsPointInPropertyTabDropBounds(screenPoint)
                ? GetSwapTabTargetAtScreenPoint(tabItem, screenPoint)
                : null;
            var shouldFloat = tabItem != null &&
                tabItem == draggingPropertyTab &&
                tabDragStarted &&
                IsPropertyTab(tabItem) &&
                !IsPointInPropertyTabDropBounds(screenPoint);

            ResetTabDragState();

            if (shouldFloat) FloatPropertyTab(tabItem, screenPoint);
            else if (swapTarget != null) SwapTabs(tabItem, swapTarget);
        }
        private void PropertyTab_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (sender == draggingPropertyTab) ResetTabDragState();
        }
        private void PropertyWindow_LocationChanged(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window == null || dockingPropertyWindows.Contains(window)) return;

            draggingPropertyWindow = window;
            propertyWindowLastMoveTime = DateTime.Now;
            EnsurePropertyWindowDragTimer();
        }
        private void PropertyWindowDragTimer_Tick(object sender, EventArgs e)
        {
            if (propertyWindows.Count == 0)
            {
                UpdatePropertyWindowDragTimerState();
                draggingPropertyWindow = null;
                return;
            }

            var window = draggingPropertyWindow;
            if (window == null || dockingPropertyWindows.Contains(window)) return;

            if ((DateTime.Now - propertyWindowLastMoveTime).TotalMilliseconds < 120) return;

            draggingPropertyWindow = null;

            if (IsPropertyWindowInPropertyTabDropBounds(window)) ReDockPropertyWindow(window);
        }
        private void PropertyWindow_Closed(object sender, EventArgs e)
        {
            CleanupPropertyWindow(sender as Window);
        }
        private void TabClose_Click(object sender, RoutedEventArgs e)
        {
            var tabItem = VisualHelper.VisualUpwardSearch<TabItem>(sender as DependencyObject) as TabItem;
            if (tabItem == draggingPropertyTab) ResetTabDragState();
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
