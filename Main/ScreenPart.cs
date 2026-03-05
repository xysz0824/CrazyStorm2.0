/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using System;
using System.Collections;
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

namespace CrazyStorm
{
    public partial class Main
    {
        static readonly Point[] NoteTextOutlineOffsets =
        {
            new Point(-1, -1),
            new Point(0, -1),
            new Point(1, -1),
            new Point(-1, 0),
            new Point(1, 0),
            new Point(-1, 1),
            new Point(0, 1),
            new Point(1, 1)
        };
        static readonly Point[] NoteHandleRatios =
        {
            new Point(0, 0),
            new Point(0.5, 0),
            new Point(1, 0),
            new Point(0, 0.5),
            new Point(1, 0.5),
            new Point(0, 1),
            new Point(0.5, 1),
            new Point(1, 1)
        };

        #region Private Members
        Point screenMousePos;
        ParticleSystem selectedSystem;
        DependencyObject aimRect;
        Component aimComponent;
        DependencyObject selectionRect;
        double selectionRectX, selectionRectY;
        bool selectingComponent;
        List<Line> bindingLines;
        bool binded;
        List<Component> selectedComponents;
        #endregion

        #region Private Methods
        Color GetNoteColor(LayerColor color)
        {
            var resource = TryFindResource($"NoteColor_{color}");
            if (resource is Color mappedColor) return mappedColor;
            if (resource is SolidColorBrush mappedBrush) return mappedBrush.Color;

            var defaultResource = TryFindResource("NoteColor_Default");
            if (defaultResource is Color defaultColor) return defaultColor;
            if (defaultResource is SolidColorBrush defaultBrush) return defaultBrush.Color;

            return Colors.DodgerBlue;
        }
        TextBlock CreateNoteTextBlock(string text, Brush foreground, double width, double maxHeight)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = foreground,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Width = width,
                MaxHeight = maxHeight,
                TextTrimming = TextTrimming.CharacterEllipsis,
                IsHitTestVisible = false
            };
        }
        void AddOutlinedNoteText(Canvas item, string text, double width, double height)
        {
            var textWidth = Math.Max(0, width - 14);
            var textHeight = Math.Max(0, height - 10);
            if (textWidth <= 0 || textHeight <= 0) return;

            foreach (var offset in NoteTextOutlineOffsets)
            {
                var outlineText = CreateNoteTextBlock(text, Brushes.Black, textWidth, textHeight);
                outlineText.SetValue(Canvas.LeftProperty, 7d + offset.X);
                outlineText.SetValue(Canvas.TopProperty, 5d + offset.Y);
                item.Children.Add(outlineText);
            }

            var mainText = CreateNoteTextBlock(text, Brushes.White, textWidth, textHeight);
            mainText.SetValue(Canvas.LeftProperty, 7d);
            mainText.SetValue(Canvas.TopProperty, 5d);
            item.Children.Add(mainText);
        }
        List<Note> GetOrderedNotesForRender()
        {
            var ordered = new List<Note>();
            if (selectedSystem == null || selectedSystem.Notes == null) return ordered;

            ordered.Capacity = selectedSystem.Notes.Count;

            foreach (var note in selectedSystem.Notes)
            {
                if (!note.Selected) ordered.Add(note);
            }
            foreach (var note in selectedSystem.Notes)
            {
                if (note.Selected) ordered.Add(note);
            }
            return ordered;
        }
        void DrawNoteHandles(Canvas noteCanvas, Brush borderBrush)
        {
            const double handleSize = 8;
            var width = noteCanvas.Width;
            var height = noteCanvas.Height;
            foreach (var ratio in NoteHandleRatios)
            {
                var point = new Point(width * ratio.X, height * ratio.Y);
                var handle = new Border
                {
                    Width = handleSize,
                    Height = handleSize,
                    Background = Brushes.White,
                    BorderBrush = borderBrush,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(1),
                    SnapsToDevicePixels = true
                };
                handle.SetValue(Canvas.LeftProperty, point.X - handleSize / 2);
                handle.SetValue(Canvas.TopProperty, point.Y - handleSize / 2);
                noteCanvas.Children.Add(handle);
            }
        }
        void RenderNoteLayer(Canvas canvas)
        {
            if (canvas == null || selectedSystem == null) return;

            canvas.Children.Clear();
            if (selectedSystem.Notes != null && selectedSystem.Notes.Count > 0)
            {
                int selectedCount = 0;
                foreach (var note in selectedSystem.Notes)
                {
                    if (note.Selected) selectedCount++;
                }
                var orderedNotes = GetOrderedNotesForRender();
                for (int i = 0; i < orderedNotes.Count; ++i)
                {
                    var note = orderedNotes[i];
                    var width = Math.Max(Note.DefaultWidth, note.Width);
                    var height = Math.Max(Note.DefaultHeight, note.Height);
                    var color = GetNoteColor(note.Color);
                    var borderBrush = new SolidColorBrush(color);
                    var fillBrush = new SolidColorBrush(Color.FromArgb(52, color.R, color.G, color.B));

                    var item = new Canvas
                    {
                        Width = width,
                        Height = height,
                        Tag = note
                    };
                    item.SetValue(Canvas.LeftProperty, (double)note.X);
                    item.SetValue(Canvas.TopProperty, (double)note.Y);
                    Panel.SetZIndex(item, i + 1);

                    var body = new Border
                    {
                        Width = width,
                        Height = height,
                        CornerRadius = new CornerRadius(6),
                        Background = fillBrush,
                        BorderBrush = borderBrush,
                        BorderThickness = note.Selected ? new Thickness(2.5) : new Thickness(2),
                        SnapsToDevicePixels = true
                    };
                    item.Children.Add(body);

                    if (note.Selected)
                    {
                        var selectedBorder = new Border
                        {
                            Width = width + 4,
                            Height = height + 4,
                            CornerRadius = new CornerRadius(7),
                            BorderBrush = Brushes.White,
                            BorderThickness = new Thickness(1),
                            Opacity = 0.85,
                            IsHitTestVisible = false
                        };
                        selectedBorder.SetValue(Canvas.LeftProperty, -2d);
                        selectedBorder.SetValue(Canvas.TopProperty, -2d);
                        item.Children.Add(selectedBorder);
                    }

                    var editingThisNote = noteEditState == NoteEditState.EditNoteText
                        && noteEditor != null
                        && noteEditingTarget == note;
                    if (!editingThisNote)
                    {
                        var noteText = (note.Comment ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
                        AddOutlinedNoteText(item, noteText, width, height);
                    }

                    if (selectedCount == 1 && note.Selected) DrawNoteHandles(item, borderBrush);

                    canvas.Children.Add(item);
                }
            }
            RenderNoteInteractionOverlay(canvas);
        }
        bool TryGetCurrentScreenLayers(out Canvas noteLayer, out Canvas componentLayer)
        {
            noteLayer = null;
            componentLayer = null;
            foreach (TabItem item in ParticleTabControl.Items)
            {
                var content = item.Content as Canvas;
                if (item.Tag != selectedSystem) continue;
                noteLayer = VisualHelper.VisualDownwardSearch(content, "NoteLayer") as Canvas;
                componentLayer = VisualHelper.VisualDownwardSearch(content, "ComponentLayer") as Canvas;
                return true;
            }
            return false;
        }
        void UpdateNoteLayer()
        {
            Canvas noteLayer;
            Canvas componentLayer;
            if (!TryGetCurrentScreenLayers(out noteLayer, out componentLayer)) return;
            RenderNoteLayer(noteLayer);
        }
        void UpdateScreen()
        {
            Canvas noteLayer;
            Canvas componentLayer;
            if (!TryGetCurrentScreenLayers(out noteLayer, out componentLayer)) return;
            if (noteEditState == NoteEditState.DragNote
                || noteEditState == NoteEditState.ResizeNote
                || (noteEditState == NoteEditState.CreateNote && noteCreatePressed))
            {
                RenderNoteLayer(noteLayer);
                return;
            }
            if (componentLayer != null)
            {
                RenderNoteLayer(noteLayer);
                var center = new Point(config.ScreenWidthOver2, config.ScreenHeightOver2);
                if (selectedComponents == null) selectedComponents = new List<Component>();
                else selectedComponents.Clear();
                componentLayer.Children.Clear();
                //Update binding lines
                if (bindingLines != null && !binded) foreach (var line in bindingLines) componentLayer.Children.Add(line);
                else if (bindingLines != null && binded) foreach (var line in bindingLines) componentLayer.Children.Remove(line);
                //Update components on current screen.
                var assembly = Assembly.GetExecutingAssembly();
                var itemTemplate = FindResource("ComponentItem") as DataTemplate;
                foreach (var layer in selectedSystem.Layers)
                {
                    if (layer.Visible)
                    {
                        foreach (var component in layer.Components)
                        {
                            var item = itemTemplate.LoadContent() as Canvas;
                            var frame = VisualHelper.VisualDownwardSearch(item, "Frame") as Label;
                            var icon = VisualHelper.VisualDownwardSearch(item, "Icon") as Path;
                            var box = VisualHelper.VisualDownwardSearch(item, "Box") as Border;
                            var id = VisualHelper.VisualDownwardSearch(item, "ID") as Label;
                            id.Content = component.Name;
                            frame.DataContext = layer;
                            box.Opacity = component.Selected ? 1 : 0;
                            //If component has a parent, caculate the absolute position.
                            float x = component.X;
                            float y = component.Y;
                            if (component.Parent != null)
                            {
                                Vector2 parent = component.Parent.GetAbsolutePosition();
                                x += parent.x;
                                y += parent.y;
                            }
                            //Draw binding line.
                            if (component.BindingTarget != null)
                            {
                                float tx = component.BindingTarget.X;
                                float ty = component.BindingTarget.Y;
                                if (component.BindingTarget.Parent != null)
                                {
                                    Vector2 parent = component.BindingTarget.Parent.GetAbsolutePosition();
                                    tx += parent.x;
                                    ty += parent.y;
                                }
                                if (component.Selected)
                                {
                                    var v = new Vector2(tx - x, ty - y);
                                    DrawHelper.DrawArrow(componentLayer, (int)(x + center.X), (int)(y + center.Y), 
                                        Math.Max(1, (int)v.Length() - 16), 3, MathHelper.GetDegree(v), Colors.White, 0.5f);
                                }
                            }
                            if (component.Selected)
                            {
                                selectedComponents.Add(component);
                                //Draw component mark.
                                var marker = assembly.CreateInstance("CrazyStorm.ComponentMarker") as IComponentMark;
                                marker.Draw(componentLayer, component, (int)(x + center.X), (int)(y + center.Y));
                                //Draw specific mark.
                                if (component is Emitter) marker = assembly.CreateInstance("CrazyStorm.EmitterMarker") as IComponentMark;
                                else marker = assembly.CreateInstance("CrazyStorm." + component.GetType().Name + "Marker") as IComponentMark;
                                marker?.Draw(componentLayer, component, (int)(x + center.X), (int)(y + center.Y));
                            }
                            icon.Data = (Geometry)FindResource($"{component.GetType().Name}_Icon");
                            var scale = (double)FindResource($"{component.GetType().Name}_Scale");
                            var transform = new ScaleTransform(scale, scale, icon.ActualWidth / 2, icon.ActualHeight / 2);
                            icon.RenderTransform = transform;
                            item.SetValue(Canvas.LeftProperty, (double)x - box.Width / 2 + center.X);
                            item.SetValue(Canvas.TopProperty, (double)y - box.Height / 2 + center.Y);
                            componentLayer.Children.Add(item);
                        }
                    }
                }
            }
        }
        void SelectComponents(int x, int y, int width, int height, int clickCount)
        {
            var center = new Point(config.ScreenWidthOver2, config.ScreenHeightOver2);
            var set = new List<Component>();
            //Select those involved in selection rect. 
            int index = 0;
            foreach (var layer in selectedSystem.Layers)
            {
                if (layer.Visible)
                {
                    foreach (var component in layer.Components)
                    {
                        //If component has parent, caculate the absolute position.
                        float absoluteX = component.X;
                        float absoluteY = component.Y;
                        if (component.Parent != null)
                        {
                            Vector2 parent = component.Parent.GetAbsolutePosition();
                            absoluteX += parent.x;
                            absoluteY += parent.y;
                        }
                        var selectRect = new Rect(x, y, width, height);
                        var componentRect = new Rect(absoluteX - config.GridWidth / 2 + center.X,
                            absoluteY - config.GridHeight / 2 + center.Y, config.GridWidth, config.GridHeight);
                        if (selectRect.IntersectsWith(componentRect))
                        {
                            //Prevent overlay shade from preceding components.
                            if (width == 0 && height == 0 && set.Count > 0) set[set.Count - 1] = component;
                            else
                                set.Add(component);
                        }
                        index++;
                    }
                }
            }
            SelectComponents(set, width == 0 && height == 0, clickCount);
        }
        void SelectComponents(List<Component> set, bool canDoubleClick)
        {
            SelectComponents(set, canDoubleClick, 0);
        }
        void SelectComponents(List<Component> set, bool canDoubleClick, int clickCount)
        {
            ClearSelectedNotes();
            foreach (var layer in selectedSystem.Layers)
            {
                if (!layer.Visible) continue;
                foreach (var component in layer.Components)
                {
                    if (Keyboard.Modifiers != ModifierKeys.Control) component.Selected = false;
                    if (set != null)
                    {
                        foreach (var target in set)
                        {
                            if (component == target)
                            {
                                component.Selected = Keyboard.Modifiers != ModifierKeys.Control ? true : !component.Selected;
                                break;
                            }
                        }
                    }
                }
            }
            UpdateSelectedStatus();
            //If mouse double click
            if (!(canDoubleClick && set != null && set.Count > 0 && Keyboard.Modifiers != ModifierKeys.Control)) return;

            if (set.Count > 0 && clickCount == 2) CreatePropertyPanel(set.First());
        }
        void CancelAllSelection()
        {
            foreach (var layer in selectedSystem.Layers)
                foreach (var component in layer.Components)
                    component.Selected = false;
            ClearSelectedNotes();

            UpdateSelectedStatus();
        }
        #endregion

        #region Public Methods
        public void ChangeTheme(string name)
        {
            var dictionaries = App.Current.Resources.MergedDictionaries;
            var original = dictionaries.Where(d => d.Source != null && d.Source.OriginalString.StartsWith("Style\\")).ToList();
            original.ForEach(d => dictionaries.Remove(d));
            switch (name)
            {
                case "StyleDefault":
                    dictionaries.Add(new ResourceDictionary() { Source = new Uri("Style\\Default.xaml", UriKind.Relative) });
                    break;
                case "StyleBlack":
                    dictionaries.Add(new ResourceDictionary() { Source = new Uri("Style\\Black.xaml", UriKind.Relative) });
                    break;
            }
            config.Theme = name;
            config.Save();
        }
        #endregion

        #region Window EventHandlers
        private void ScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var moveStatusMap = new Dictionary<Key, MoveStatus> { 
                { Key.Up, MoveStatus.Up }, 
                { Key.Down, MoveStatus.Down }, 
                { Key.Left, MoveStatus.Left }, 
                { Key.Right, MoveStatus.Right }
            };
            switch (e.Key)
            {
                case Key.Escape:
                    if (HandleNoteEscapeKey()) e.Handled = true;
                    break;
                case Key.Delete:
                    if (!IsEditingNoteText() && GetSelectedNoteCount() > 0)
                    {
                        DeleteSelectedNotes();
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                    if (IsEditingNoteText()) return;
                    if (selectedComponents.Count > 0)
                    {
                        var gridSize = config.GridSize;
                        var stack = commandStacks[selectedSystem];
                        new MoveComponentCommand(moveStatusMap[e.Key], gridSize, config.GridAlignment).Do(stack, 
                            selectedComponents, new Action(UpdateProperty));
                    }
                    e.Handled = true;
                    break;
            }
        }
        private void Screen_LostFocus(object sender, RoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null && scrollViewer.IsKeyboardFocusWithin) return;
            CancelAllSelection();
        }
        private void Screen_MouseEnter(object sender, MouseEventArgs e)
        {
            MousePosTip.Visibility = Visibility.Visible;
        }
        private void Screen_MouseLeave(object sender, MouseEventArgs e)
        {
            MousePosTip.Visibility = Visibility.Hidden;
        }
        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
            var center = new Point(config.ScreenWidthOver2, config.ScreenHeightOver2);
            screenMousePos = e.GetPosition(sender as IInputElement);
            int x = (int)screenMousePos.X;
            int y = (int)screenMousePos.Y;
            if (selectedComponents.Count == 1) MousePosTip.Content = $"{selectedComponents[0].X},{selectedComponents[0].Y}";
            else MousePosTip.Content = $"{x - center.X},{y - center.Y}";
            if (TryHandleNoteMouseMove(sender, e)) return;
            //Display a rect with red edge to mark the location that component will be put on.
            if (aimRect != null)
            {
                if (config.GridAlignment)
                {
                    aimRect.SetValue(Canvas.LeftProperty, (double)((x / (config.GridWidth / 2)) * (config.GridWidth / 2)));
                    aimRect.SetValue(Canvas.TopProperty, (double)((y / (config.GridHeight / 2)) * (config.GridHeight / 2)));
                }
                else if (x <= config.ScreenWidth - config.GridWidth && y <= config.ScreenHeight - config.GridHeight)
                {
                    aimRect.SetValue(Canvas.LeftProperty, (double)x);
                    aimRect.SetValue(Canvas.TopProperty, (double)y);
                }
            }
            //Display a coloured rect to mark the range that is selecting.
            if (selectionRect != null)
            {
                var width = x - selectionRectX;
                if (width >= 0)
                {
                    selectionRect.SetValue(Canvas.LeftProperty, selectionRectX);
                    selectionRect.SetValue(WidthProperty, (double)width);
                }
                else
                {
                    selectionRect.SetValue(Canvas.LeftProperty, (double)x);
                    selectionRect.SetValue(WidthProperty, (double)-width);
                }
                var height = y - selectionRectY;
                if (height >= 0)
                {
                    selectionRect.SetValue(Canvas.TopProperty, selectionRectY);
                    selectionRect.SetValue(HeightProperty, (double)height);
                }
                else
                {
                    selectionRect.SetValue(Canvas.TopProperty, (double)y);
                    selectionRect.SetValue(HeightProperty, (double)-height);
                }
            }
            //Update binding lines
            if (bindingLines != null)
            {
                foreach (var line in bindingLines)
                {
                    line.X2 = (int)screenMousePos.X;
                    line.Y2 = (int)screenMousePos.Y;
                }
            }
        }
        private void Screen_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TryHandleNoteMouseRightButtonDown(sender, e))
            {
                e.Handled = true;
                return;
            }
            //Take away the rect.
            if (aimRect != null)
            {
                aimRect.SetValue(OpacityProperty, 0.0d);
                aimRect = null;
            }
        }
        private void Screen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (player != null && !player.Pause) return;
            if (TryHandleNoteMouseLeftButtonDown(sender, e)) return;
            //Show selection rect.
            Point point = e.GetPosition(sender as IInputElement);
            double x = point.X;
            double y = point.Y;
            selectingComponent = true;
            var content = (DependencyObject)ParticleTabControl.SelectedContent;
            selectionRect = VisualHelper.VisualDownwardSearch(content, "SelectingBox");
            selectionRect.SetValue(Canvas.LeftProperty, x);
            selectionRect.SetValue(Canvas.TopProperty, y);
            selectionRectX = x;
            selectionRectY = y;
            //Add component to the place mouse down with left-button.
            if (aimRect != null)
            {
                var center = new Point(config.ScreenWidthOver2, config.ScreenHeightOver2);
                aimRect.SetValue(OpacityProperty, 0.0d);
                var boxX = (double)aimRect.GetValue(Canvas.LeftProperty);
                var boxY = (double)aimRect.GetValue(Canvas.TopProperty);
                aimComponent.ID = selectedSystem.GetComponentIndex();
                var index = selectedSystem.GetAndIncreaseComponentIndex(aimComponent.GetType().ToString());
                aimComponent.Name = (string)FindResource($"{aimComponent.GetType().Name}Str") + (index + 1);
                aimComponent.X = (int)(boxX + (double)aimRect.GetValue(Canvas.WidthProperty) / 2 - center.X);
                aimComponent.Y = (int)(boxY + (double)aimRect.GetValue(Canvas.HeightProperty) / 2 - center.Y);
                new AddComponentCommand().Do(commandStacks[selectedSystem], selectedSystem, selectedLayer, aimComponent);
                UpdateSelectedStatus();
                aimComponent = null;
                aimRect = null;
            }
            //Binding to selected emitter
            if (bindingLines != null)
            {
                binded = true;
                SelectComponents((int)screenMousePos.X, (int)screenMousePos.Y, 1, 1, e.ClickCount);
                if (selectedComponents.Count == 1 && selectedComponents[0] is Emitter)
                {
                    new BindComponentCommand().Do(commandStacks[selectedSystem], bindingLines, selectedComponents.First());
                    var components = new List<Component>();
                    foreach (var line in bindingLines)
                    {
                        var component = line.DataContext as Component;
                        components.Add(component);
                    }
                    SelectComponents(components, false);
                }
                bindingLines = null;
            }
            if (e.ClickCount == 2)
            {
                ParticleTabControl_MouseLeftButtonUp(sender, e);
            }
        }
        private void ParticleTabControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //Cancel selection when mouse leaves.
            if (selectingComponent
                || noteCreatePressed
                || noteDragPending
                || noteEditState == NoteEditState.DragNote
                || noteEditState == NoteEditState.ResizeNote)
                ParticleTabControl_MouseLeftButtonUp(sender, null);
        }
        private void ParticleTabControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var startedAsComponentFlow = selectingComponent;
            if (TryHandleNoteMouseLeftButtonUp(sender, e))
            {
                selectingComponent = false;
                return;
            }
            selectingComponent = false;
            if (!startedAsComponentFlow) return;
            if (binded)
            {
                binded = false;
                selectionRect.SetValue(WidthProperty, 0.0d);
                selectionRect.SetValue(HeightProperty, 0.0d);
                selectionRect = null;
                return;
            }
            //Determine selected components.
            if (selectionRect != null)
            {
                var x = (double)selectionRect.GetValue(LeftProperty);
                var y = (double)selectionRect.GetValue(TopProperty);
                var width = (double)selectionRect.GetValue(WidthProperty);
                var height = (double)selectionRect.GetValue(HeightProperty);
                if (e != null) SelectComponents((int)x, (int)y, (int)width, (int)height, e.ClickCount);
                else
                    SelectComponents((int)x, (int)y, (int)width, (int)height, 0);

                selectionRect.SetValue(WidthProperty, 0.0d);
                selectionRect.SetValue(HeightProperty, 0.0d);
                selectionRect = null;
            }
            else
                CancelAllSelection();
        }
        private void ParticleTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Switch to selected Particle.
            if (e.AddedItems.Count > 0)
            {
                EndNoteTextEdit(true);
                noteCreatePressed = false;
                noteLeftPressOwnedByNote = false;
                noteDragStartRects = null;
                noteResizeTarget = null;
                noteEditState = NoteEditState.Idle;
                ParticleTabControl.Cursor = Cursors.Arrow;
                var tabItem = e.AddedItems[0] as TabItem;
                foreach (var item in file.ParticleSystems)
                {
                    if (item == tabItem.Tag)
                    {
                        selectedSystem = item;
                        break;
                    }
                }
                InitializeLayerAndComponent();
                UpdateSelectedStatus();
            }
        }
        private void StyleItem_Loaded(object sender, RoutedEventArgs e)
        {
            var clickedItem = sender as MenuItem;
            clickedItem.IsChecked = clickedItem.Name == config.Theme;
        }
        private void GlobalSettingItem_Click(object sender, RoutedEventArgs e)
        {
            //Open global setting window.
            GlobalSetting window = new GlobalSetting(config);
            window.OnButtonClick += () => UpdateScreen();
            window.ShowDialog();
            window.Close();
        }

        private void StyleItem_Click(object sender, RoutedEventArgs e)
        {
            var clickedItem = sender as MenuItem; 
            var parent = clickedItem.Parent as MenuItem;
            foreach (MenuItem item in parent.Items)
            {
                item.IsChecked = false;
            }
            clickedItem.IsChecked = true;
            ChangeTheme(clickedItem.Name);
        }
        #endregion
    }
}
