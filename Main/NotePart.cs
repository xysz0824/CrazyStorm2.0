/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CrazyStorm.Core;

namespace CrazyStorm
{
    enum NoteEditState
    {
        Idle,
        CreateNote,
        DragNote,
        ResizeNote,
        EditNoteText
    }
    enum NoteResizeHandle
    {
        None,
        LeftTop,
        Top,
        RightTop,
        Left,
        Right,
        LeftBottom,
        Bottom,
        RightBottom
    }
    public partial class Main
    {
        const double NotePointerThreshold = 2;
        const double NoteResizeHitSize = 10;
        const int NoteDoubleClickFallbackMs = 650;
        static readonly LayerColor[] NoteColorOptions =
        {
            LayerColor.Blue,
            LayerColor.Purple,
            LayerColor.Red,
            LayerColor.Green,
            LayerColor.Yellow,
            LayerColor.Orange,
            LayerColor.Pink
        };

        #region Private Members
        NoteEditState noteEditState;
        Point noteDownPoint;
        Point noteCurrentPoint;
        bool noteCreatePressed;
        bool noteDragPending;
        bool noteDragStarted;
        Vector2 noteDragMove;
        Dictionary<Note, Rect> noteDragStartRects;
        Note noteResizeTarget;
        NoteResizeHandle noteResizeHandle;
        Rect noteResizeStartRect;
        Rect noteResizeCurrentRect;
        bool noteResizeStarted;
        TextBox noteEditor;
        Note noteEditingTarget;
        string noteEditingOriginalComment;
        bool noteEditorTextSyncing;
        bool noteEndingEditor;
        ContextMenu noteColorMenu;
        Note noteColorTarget;
        Note noteLastClickTarget;
        int noteLastClickTimestamp;
        #endregion

        #region Private Methods
        double GetCanvasWidth()
        {
            return Math.Max(Note.DefaultWidth, config.ScreenWidth);
        }
        double GetCanvasHeight()
        {
            return Math.Max(Note.DefaultHeight, config.ScreenHeight);
        }
        Rect GetNoteRect(Note note)
        {
            return new Rect(
                note.X,
                note.Y,
                Math.Max(Note.DefaultWidth, note.Width),
                Math.Max(Note.DefaultHeight, note.Height));
        }
        string NormalizeNoteComment(string text)
        {
            return (text ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
        }
        Canvas GetCurrentNoteLayer()
        {
            var selected = ParticleTabControl.SelectedItem as TabItem;
            var content = selected?.Content as Canvas;
            if (content == null) return null;
            return VisualHelper.VisualDownwardSearch(content, "NoteLayer") as Canvas;
        }
        void FocusParticleTabControl()
        {
            ParticleTabControl.Focus();
            Keyboard.Focus(ParticleTabControl);
        }
        Rect ClampNoteRect(Rect rect)
        {
            var minWidth = (double)Note.DefaultWidth;
            var minHeight = (double)Note.DefaultHeight;
            var canvasWidth = GetCanvasWidth();
            var canvasHeight = GetCanvasHeight();
            var width = Math.Max(minWidth, rect.Width);
            var height = Math.Max(minHeight, rect.Height);
            width = Math.Min(width, canvasWidth);
            height = Math.Min(height, canvasHeight);
            var x = rect.X;
            var y = rect.Y;
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x + width > canvasWidth) x = canvasWidth - width;
            if (y + height > canvasHeight) y = canvasHeight - height;
            return new Rect(x, y, width, height);
        }
        Rect GetRectFromPoints(Point start, Point end)
        {
            var x = Math.Min(start.X, end.X);
            var y = Math.Min(start.Y, end.Y);
            var width = Math.Abs(start.X - end.X);
            var height = Math.Abs(start.Y - end.Y);
            return new Rect(x, y, width, height);
        }
        Rect BuildCreateRect(Point start, Point end)
        {
            var rect = GetRectFromPoints(start, end);
            if (rect.Width <= NotePointerThreshold && rect.Height <= NotePointerThreshold)
            {
                rect = new Rect(start.X, start.Y, Note.DefaultWidth, Note.DefaultHeight);
            }
            return ClampNoteRect(rect);
        }
        List<Note> GetNotesForHitTest()
        {
            var ordered = GetOrderedNotesForRender();
            ordered.Reverse();
            return ordered;
        }
        NoteResizeHandle HitTestResizeHandle(Note note, Point point)
        {
            if (note == null) return NoteResizeHandle.None;
            var rect = GetNoteRect(note);
            var points = new Dictionary<NoteResizeHandle, Point>()
            {
                { NoteResizeHandle.LeftTop, new Point(rect.Left, rect.Top) },
                { NoteResizeHandle.Top, new Point(rect.Left + rect.Width / 2, rect.Top) },
                { NoteResizeHandle.RightTop, new Point(rect.Right, rect.Top) },
                { NoteResizeHandle.Left, new Point(rect.Left, rect.Top + rect.Height / 2) },
                { NoteResizeHandle.Right, new Point(rect.Right, rect.Top + rect.Height / 2) },
                { NoteResizeHandle.LeftBottom, new Point(rect.Left, rect.Bottom) },
                { NoteResizeHandle.Bottom, new Point(rect.Left + rect.Width / 2, rect.Bottom) },
                { NoteResizeHandle.RightBottom, new Point(rect.Right, rect.Bottom) }
            };
            foreach (var pair in points)
            {
                var hitRect = new Rect(
                    pair.Value.X - NoteResizeHitSize / 2,
                    pair.Value.Y - NoteResizeHitSize / 2,
                    NoteResizeHitSize,
                    NoteResizeHitSize);
                if (hitRect.Contains(point)) return pair.Key;
            }
            return NoteResizeHandle.None;
        }
        bool TryHitResizeHandle(Point point, out Note note, out NoteResizeHandle handle)
        {
            note = null;
            handle = NoteResizeHandle.None;
            if (GetSelectedNoteCount() != 1) return false;

            var selected = GetSelectedNotes();
            if (selected.Count != 1) return false;
            note = selected[0];
            handle = HitTestResizeHandle(note, point);
            if (handle == NoteResizeHandle.None)
            {
                note = null;
                return false;
            }
            return true;
        }
        bool TryHitNoteBody(Point point, out Note note)
        {
            note = null;
            var notes = GetNotesForHitTest();
            foreach (var item in notes)
            {
                var rect = GetNoteRect(item);
                if (rect.Contains(point))
                {
                    note = item;
                    return true;
                }
            }
            return false;
        }
        bool TryHitTopComponent(Point point, out Component component)
        {
            component = null;
            if (selectedSystem == null || selectedSystem.Layers == null) return false;

            var center = new Point(config.ScreenWidthOver2, config.ScreenHeightOver2);
            foreach (var layer in selectedSystem.Layers)
            {
                if (!layer.Visible) continue;
                foreach (var item in layer.Components)
                {
                    float absoluteX = item.X;
                    float absoluteY = item.Y;
                    if (item.Parent != null)
                    {
                        Vector2 parent = item.Parent.GetAbsolutePosition();
                        absoluteX += parent.x;
                        absoluteY += parent.y;
                    }
                    var rect = new Rect(
                        absoluteX - config.GridWidth / 2 + center.X,
                        absoluteY - config.GridHeight / 2 + center.Y,
                        config.GridWidth,
                        config.GridHeight);
                    if (rect.Contains(point)) component = item;
                }
            }
            return component != null;
        }
        ContextMenu BuildNoteColorMenu()
        {
            var menu = new ContextMenu();
            foreach (var color in NoteColorOptions)
            {
                var item = new MenuItem();
                item.Header = FindResource($"{color}Str");
                item.Tag = color;
                item.IsCheckable = true;
                item.Click += NoteColorMenuItem_Click;
                menu.Items.Add(item);
            }
            return menu;
        }
        void OpenNoteColorMenu(UIElement placementTarget, Note note)
        {
            if (placementTarget == null || note == null) return;
            if (noteColorMenu == null) noteColorMenu = BuildNoteColorMenu();
            noteColorTarget = note;
            foreach (MenuItem item in noteColorMenu.Items)
            {
                if (item.Tag is LayerColor color)
                    item.IsChecked = color == note.Color;
            }
            noteColorMenu.PlacementTarget = placementTarget;
            noteColorMenu.Placement = PlacementMode.MousePoint;
            noteColorMenu.IsOpen = true;
        }
        void ClearSelectedNotes()
        {
            if (selectedSystem == null || selectedSystem.Notes == null) return;
            foreach (var note in selectedSystem.Notes) note.Selected = false;
        }
        void ClearSelectedComponents()
        {
            if (selectedSystem == null || selectedSystem.Layers == null) return;
            foreach (var layer in selectedSystem.Layers)
                foreach (var component in layer.Components)
                    component.Selected = false;
        }
        void SelectSingleNote(Note note)
        {
            ClearSelectedNotes();
            if (note != null) note.Selected = true;
            ClearSelectedComponents();
        }
        Rect CalculateResizeRect(Rect source, NoteResizeHandle handle, Vector2 move)
        {
            var left = source.Left;
            var top = source.Top;
            var right = source.Right;
            var bottom = source.Bottom;
            var minWidth = (double)Note.DefaultWidth;
            var minHeight = (double)Note.DefaultHeight;
            var canvasWidth = GetCanvasWidth();
            var canvasHeight = GetCanvasHeight();
            var moveLeft = false;
            var moveRight = false;
            var moveTop = false;
            var moveBottom = false;
            switch (handle)
            {
                case NoteResizeHandle.LeftTop:
                    moveLeft = true;
                    moveTop = true;
                    break;
                case NoteResizeHandle.Top:
                    moveTop = true;
                    break;
                case NoteResizeHandle.RightTop:
                    moveRight = true;
                    moveTop = true;
                    break;
                case NoteResizeHandle.Left:
                    moveLeft = true;
                    break;
                case NoteResizeHandle.Right:
                    moveRight = true;
                    break;
                case NoteResizeHandle.LeftBottom:
                    moveLeft = true;
                    moveBottom = true;
                    break;
                case NoteResizeHandle.Bottom:
                    moveBottom = true;
                    break;
                case NoteResizeHandle.RightBottom:
                    moveRight = true;
                    moveBottom = true;
                    break;
            }
            if (moveLeft) left += move.x;
            if (moveRight) right += move.x;
            if (moveTop) top += move.y;
            if (moveBottom) bottom += move.y;
            if (moveLeft && !moveRight) left = MathHelper.Clamp(left, 0, right - minWidth);
            if (moveRight && !moveLeft) right = MathHelper.Clamp(right, left + minWidth, canvasWidth);
            if (moveTop && !moveBottom) top = MathHelper.Clamp(top, 0, bottom - minHeight);
            if (moveBottom && !moveTop) bottom = MathHelper.Clamp(bottom, top + minHeight, canvasHeight);
            var rect = new Rect(left, top, right - left, bottom - top);
            return ClampNoteRect(rect);
        }
        Vector2 ClampGroupMove(Dictionary<Note, Rect> startRects, Vector2 move)
        {
            if (startRects == null || startRects.Count == 0) return Vector2.Zero;
            var minLeft = startRects.Values.Min(rect => rect.Left);
            var minTop = startRects.Values.Min(rect => rect.Top);
            var maxRight = startRects.Values.Max(rect => rect.Right);
            var maxBottom = startRects.Values.Max(rect => rect.Bottom);
            var minMoveX = -minLeft;
            var minMoveY = -minTop;
            var maxMoveX = GetCanvasWidth() - maxRight;
            var maxMoveY = GetCanvasHeight() - maxBottom;
            var result = move;
            result.x = MathHelper.Clamp(result.x, (float)minMoveX, (float)maxMoveX);
            result.y = MathHelper.Clamp(result.y, (float)minMoveY, (float)maxMoveY);
            return result;
        }
        void ApplyDragPreview(Vector2 move)
        {
            if (noteDragStartRects == null) return;
            foreach (var pair in noteDragStartRects)
            {
                pair.Key.X = (float)(pair.Value.X + move.x);
                pair.Key.Y = (float)(pair.Value.Y + move.y);
            }
        }
        void BeginNoteDrag(Point point)
        {
            var selected = GetSelectedNotes();
            if (selected.Count == 0) return;
            noteDragPending = false;
            noteEditState = NoteEditState.DragNote;
            noteDownPoint = point;
            noteCurrentPoint = point;
            noteDragMove = Vector2.Zero;
            noteDragStarted = false;
            noteDragStartRects = new Dictionary<Note, Rect>();
            foreach (var note in selected)
                noteDragStartRects[note] = GetNoteRect(note);
        }
        void EndNoteDrag()
        {
            if (noteDragStartRects == null)
            {
                noteEditState = NoteEditState.Idle;
                return;
            }

            if (noteDragStarted && (noteDragMove.x != 0 || noteDragMove.y != 0))
            {
                var selected = new List<Note>(noteDragStartRects.Keys);
                foreach (var pair in noteDragStartRects)
                {
                    pair.Key.X = (float)pair.Value.X;
                    pair.Key.Y = (float)pair.Value.Y;
                }
                new MoveNoteCommand(noteDragMove).Do(commandStacks[selectedSystem], selected);
            }

            noteDragMove = Vector2.Zero;
            noteDragStartRects = null;
            noteDragPending = false;
            noteDragStarted = false;
            noteEditState = NoteEditState.Idle;
            UpdateSelectedStatus();
        }
        void BeginNoteResize(Note note, NoteResizeHandle handle, Point point)
        {
            noteResizeTarget = note;
            noteResizeHandle = handle;
            noteResizeStartRect = GetNoteRect(note);
            noteResizeCurrentRect = noteResizeStartRect;
            noteDownPoint = point;
            noteCurrentPoint = point;
            noteResizeStarted = false;
            noteEditState = NoteEditState.ResizeNote;
        }
        void EndNoteResize()
        {
            if (noteResizeTarget == null)
            {
                noteEditState = NoteEditState.Idle;
                return;
            }

            if (noteResizeStarted && noteResizeCurrentRect != noteResizeStartRect)
            {
                noteResizeTarget.X = (float)noteResizeStartRect.X;
                noteResizeTarget.Y = (float)noteResizeStartRect.Y;
                noteResizeTarget.Width = (float)noteResizeStartRect.Width;
                noteResizeTarget.Height = (float)noteResizeStartRect.Height;
                new ResizeNoteCommand(noteResizeStartRect, noteResizeCurrentRect).Do(
                    commandStacks[selectedSystem], noteResizeTarget);
            }

            noteResizeTarget = null;
            noteResizeHandle = NoteResizeHandle.None;
            noteResizeStartRect = Rect.Empty;
            noteResizeCurrentRect = Rect.Empty;
            noteResizeStarted = false;
            noteEditState = NoteEditState.Idle;
            UpdateSelectedStatus();
        }
        void EndNoteCreate()
        {
            if (!noteCreatePressed)
            {
                noteEditState = NoteEditState.Idle;
                return;
            }

            var rect = BuildCreateRect(noteDownPoint, noteCurrentPoint);
            var note = new Note();
            note.X = (float)rect.X;
            note.Y = (float)rect.Y;
            note.Width = (float)rect.Width;
            note.Height = (float)rect.Height;
            note.Comment = NormalizeNoteComment((string)FindResource("DefaultCommentStr"));
            ClearSelectedNotes();
            ClearSelectedComponents();
            new AddNoteCommand().Do(commandStacks[selectedSystem], selectedSystem, note);
            note.Selected = true;
            noteCreatePressed = false;
            noteEditState = NoteEditState.Idle;
            ParticleTabControl.Cursor = Cursors.Arrow;
            FocusParticleTabControl();
            UpdateSelectedStatus();
        }
        void SetNoteEditorLayout()
        {
            if (noteEditor == null || noteEditingTarget == null) return;
            var rect = GetNoteRect(noteEditingTarget);
            noteEditor.Width = Math.Max(16, rect.Width - 14);
            noteEditor.Height = Math.Max(16, rect.Height - 10);
            noteEditor.SetValue(Canvas.LeftProperty, rect.Left + 7);
            noteEditor.SetValue(Canvas.TopProperty, rect.Top + 5);
        }
        void BeginNoteTextEdit(Note note)
        {
            if (note == null) return;
            if (!note.Selected || GetSelectedNoteCount() != 1)
            {
                SelectSingleNote(note);
                UpdateSelectedStatus();
            }
            if (noteEditor != null && noteEditingTarget == note)
            {
                noteEditor.Focus();
                return;
            }

            EndNoteTextEdit(true);
            noteEditingTarget = note;
            noteEditingOriginalComment = note.Comment ?? string.Empty;
            noteEditState = NoteEditState.EditNoteText;
            noteEditor = new TextBox();
            noteEditor.AcceptsReturn = false;
            noteEditor.AcceptsTab = false;
            noteEditor.TextWrapping = TextWrapping.Wrap;
            noteEditor.VerticalContentAlignment = VerticalAlignment.Top;
            noteEditor.HorizontalContentAlignment = HorizontalAlignment.Left;
            noteEditor.Padding = new Thickness(0);
            noteEditor.BorderThickness = new Thickness(1);
            noteEditor.BorderBrush = Brushes.White;
            noteEditor.Background = Brushes.Transparent;
            noteEditor.Foreground = Brushes.White;
            noteEditor.Text = noteEditingOriginalComment;
            noteEditor.TextChanged += NoteEditor_TextChanged;
            noteEditor.PreviewKeyDown += NoteEditor_PreviewKeyDown;
            noteEditor.LostKeyboardFocus += NoteEditor_LostKeyboardFocus;
            SetNoteEditorLayout();
            var layer = GetCurrentNoteLayer();
            layer?.Children.Add(noteEditor);
            noteEditor.Focus();
            noteEditor.CaretIndex = noteEditor.Text.Length;
            noteEditor.Select(noteEditor.CaretIndex, 0);
        }
        void EndNoteTextEdit(bool commit)
        {
            if (noteEndingEditor) return;
            noteEndingEditor = true;
            try
            {
                if (noteEditor != null)
                {
                    noteEditor.TextChanged -= NoteEditor_TextChanged;
                    noteEditor.PreviewKeyDown -= NoteEditor_PreviewKeyDown;
                    noteEditor.LostKeyboardFocus -= NoteEditor_LostKeyboardFocus;
                    var parent = noteEditor.Parent as Panel;
                    parent?.Children.Remove(noteEditor);
                }

                var editorText = noteEditor != null ? noteEditor.Text : string.Empty;
                var text = NormalizeNoteComment(editorText);
                var note = noteEditingTarget;
                var oldText = noteEditingOriginalComment ?? string.Empty;
                noteEditor = null;
                noteEditingTarget = null;
                noteEditingOriginalComment = null;
                noteEditorTextSyncing = false;
                noteEditState = NoteEditState.Idle;
                if (note == null)
                {
                    UpdateScreen();
                    return;
                }

                if (commit)
                {
                    if (!string.Equals(text, oldText, StringComparison.Ordinal))
                    {
                        new SetNoteCommentCommand().Do(commandStacks[selectedSystem], note, text);
                    }
                }
                else
                {
                    note.Comment = oldText;
                }
                UpdateSelectedStatus();
            }
            finally
            {
                noteEndingEditor = false;
            }
        }
        void UpdateNoteMenuStatus()
        {
            var selectedCount = GetSelectedNoteCount();
            if (AddNoteItem != null) AddNoteItem.IsEnabled = selectedSystem != null;
            if (EditNoteItem != null) EditNoteItem.IsEnabled = selectedCount == 1;
            if (DeleteNoteItem != null) DeleteNoteItem.IsEnabled = selectedCount > 0;
            if (AddNoteButton != null) AddNoteButton.IsEnabled = selectedSystem != null;
        }
        void StartCreateNoteMode()
        {
            EndNoteTextEdit(true);
            noteEditState = NoteEditState.CreateNote;
            noteCreatePressed = false;
            noteDragPending = false;
            noteDragMove = Vector2.Zero;
            noteDragStartRects = null;
            noteResizeTarget = null;
            noteResizeHandle = NoteResizeHandle.None;
            noteResizeStartRect = Rect.Empty;
            noteResizeCurrentRect = Rect.Empty;
            if (aimRect != null)
            {
                aimRect.SetValue(OpacityProperty, 0.0d);
                aimRect = null;
                aimComponent = null;
            }
            ParticleTabControl.Cursor = Cursors.Cross;
            UpdateScreen();
        }
        void CancelCreateNoteMode()
        {
            noteCreatePressed = false;
            if (noteEditState == NoteEditState.CreateNote)
            {
                noteEditState = NoteEditState.Idle;
                ParticleTabControl.Cursor = Cursors.Arrow;
                UpdateScreen();
            }
        }
        void EditSelectedNote()
        {
            var selected = GetSelectedNotes();
            if (selected.Count != 1) return;
            BeginNoteTextEdit(selected[0]);
        }
        void DeleteSelectedNotes()
        {
            if (noteEditState == NoteEditState.EditNoteText) return;
            var selected = GetSelectedNotes();
            if (selected.Count == 0) return;
            new DelNoteCommand().Do(commandStacks[selectedSystem], selectedSystem, selected);
            UpdateSelectedStatus();
        }
        int GetSelectedNoteCount()
        {
            if (selectedSystem == null || selectedSystem.Notes == null) return 0;
            return selectedSystem.Notes.Count(note => note.Selected);
        }
        List<Note> GetSelectedNotes()
        {
            var selected = new List<Note>();
            if (selectedSystem == null || selectedSystem.Notes == null) return selected;
            foreach (var note in selectedSystem.Notes)
                if (note.Selected) selected.Add(note);

            return selected;
        }
        bool IsEditingNoteText()
        {
            return noteEditState == NoteEditState.EditNoteText && noteEditor != null;
        }
        bool HandleNoteEscapeKey()
        {
            if (IsEditingNoteText())
            {
                EndNoteTextEdit(false);
                return true;
            }
            if (noteEditState == NoteEditState.CreateNote)
            {
                CancelCreateNoteMode();
                return true;
            }
            if (GetSelectedNoteCount() > 0)
            {
                noteDragPending = false;
                ClearSelectedNotes();
                UpdateSelectedStatus();
                return true;
            }
            return false;
        }
        bool TryHandleNoteMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedSystem == null || selectedSystem.Notes == null) return false;
            noteDragPending = false;
            if (noteEditState == NoteEditState.DragNote) EndNoteDrag();
            if (noteEditState == NoteEditState.ResizeNote) EndNoteResize();
            if (noteEditState != NoteEditState.CreateNote && (aimRect != null || bindingLines != null)) return false;
            var point = e.GetPosition(sender as IInputElement);

            if (IsEditingNoteText() && noteEditor != null)
            {
                var source = e.OriginalSource as DependencyObject;
                var sourceEditor = source as TextBox ?? VisualHelper.FindParent<TextBox>(source);
                if (sourceEditor == noteEditor) return true;
            }

            if (noteEditState == NoteEditState.CreateNote)
            {
                noteDownPoint = point;
                noteCurrentPoint = point;
                noteCreatePressed = true;
                e.Handled = true;
                UpdateScreen();
                return true;
            }

            Note note;
            NoteResizeHandle handle;
            if (TryHitResizeHandle(point, out note, out handle))
            {
                BeginNoteResize(note, handle, point);
                e.Handled = true;
                return true;
            }

            var hitNoteBody = TryHitNoteBody(point, out note);
            if (IsEditingNoteText())
            {
                if (!hitNoteBody)
                {
                    EndNoteTextEdit(true);
                    ClearSelectedNotes();
                    UpdateSelectedStatus();
                    e.Handled = true;
                    return true;
                }

                if (noteEditingTarget != null && (!noteEditingTarget.Selected || GetSelectedNoteCount() != 1))
                {
                    SelectSingleNote(noteEditingTarget);
                    UpdateSelectedStatus();
                }
                e.Handled = true;
                return true;
            }

            Component component;
            if (TryHitTopComponent(point, out component)) return false;

            if (!hitNoteBody)
            {
                noteLastClickTarget = null;
                noteLastClickTimestamp = 0;
                if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control && GetSelectedNoteCount() > 0)
                {
                    ClearSelectedNotes();
                    UpdateSelectedStatus();
                    e.Handled = true;
                    return true;
                }
                return false;
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                note.Selected = !note.Selected;
                if (note.Selected) ClearSelectedComponents();
                e.Handled = true;
                noteDragPending = false;
                noteLastClickTarget = null;
                noteLastClickTimestamp = 0;
                FocusParticleTabControl();
                UpdateSelectedStatus();
                return true;
            }

            if (!note.Selected || GetSelectedNoteCount() != 1)
            {
                SelectSingleNote(note);
                UpdateSelectedStatus();
            }
            else
            {
                UpdateScreen();
            }

            var fallbackDoubleClick = noteLastClickTarget == note
                && noteLastClickTimestamp > 0
                && e.Timestamp > noteLastClickTimestamp
                && e.Timestamp - noteLastClickTimestamp <= NoteDoubleClickFallbackMs;
            var isDoubleClick = e.ClickCount == 2 || fallbackDoubleClick;
            noteLastClickTarget = note;
            noteLastClickTimestamp = e.Timestamp;

            if (isDoubleClick)
            {
                noteDragPending = false;
                BeginNoteTextEdit(note);
                e.Handled = true;
                return true;
            }

            noteDownPoint = point;
            noteCurrentPoint = point;
            noteDragPending = true;
            e.Handled = true;
            FocusParticleTabControl();
            return true;
        }
        bool TryHandleNoteMouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(sender as IInputElement);
            if (noteDragPending)
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                {
                    noteDragPending = false;
                    return false;
                }

                var pendingMove = new Vector2((float)(point.X - noteDownPoint.X), (float)(point.Y - noteDownPoint.Y));
                if (Math.Abs(pendingMove.x) <= NotePointerThreshold &&
                    Math.Abs(pendingMove.y) <= NotePointerThreshold)
                    return true;

                BeginNoteDrag(noteDownPoint);
                noteDragStarted = true;
                noteDragMove = ClampGroupMove(noteDragStartRects, pendingMove);
                ApplyDragPreview(noteDragMove);
                UpdateScreen();
                return true;
            }
            if (noteEditState == NoteEditState.DragNote && e.LeftButton != MouseButtonState.Pressed)
            {
                EndNoteDrag();
                return false;
            }
            if (noteEditState == NoteEditState.ResizeNote && e.LeftButton != MouseButtonState.Pressed)
            {
                EndNoteResize();
                return false;
            }
            if (noteEditState == NoteEditState.CreateNote && noteCreatePressed)
            {
                noteCurrentPoint = point;
                UpdateScreen();
                return true;
            }
            if (noteEditState == NoteEditState.DragNote && noteDragStartRects != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var move = new Vector2((float)(point.X - noteDownPoint.X), (float)(point.Y - noteDownPoint.Y));
                if (!noteDragStarted &&
                    Math.Abs(move.x) <= NotePointerThreshold &&
                    Math.Abs(move.y) <= NotePointerThreshold)
                    return true;

                noteDragStarted = true;
                noteDragMove = ClampGroupMove(noteDragStartRects, move);
                ApplyDragPreview(noteDragMove);
                UpdateScreen();
                return true;
            }
            if (noteEditState == NoteEditState.ResizeNote && noteResizeTarget != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var move = new Vector2((float)(point.X - noteDownPoint.X), (float)(point.Y - noteDownPoint.Y));
                if (!noteResizeStarted &&
                    Math.Abs(move.x) <= NotePointerThreshold &&
                    Math.Abs(move.y) <= NotePointerThreshold)
                    return true;

                noteResizeStarted = true;
                noteResizeCurrentRect = CalculateResizeRect(noteResizeStartRect, noteResizeHandle, move);
                noteResizeTarget.X = (float)noteResizeCurrentRect.X;
                noteResizeTarget.Y = (float)noteResizeCurrentRect.Y;
                noteResizeTarget.Width = (float)noteResizeCurrentRect.Width;
                noteResizeTarget.Height = (float)noteResizeCurrentRect.Height;
                UpdateScreen();
                return true;
            }
            return false;
        }
        bool TryHandleNoteMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (noteEditState == NoteEditState.CreateNote)
            {
                EndNoteCreate();
                return true;
            }
            if (noteDragPending)
            {
                noteDragPending = false;
                return true;
            }
            if (noteEditState == NoteEditState.DragNote)
            {
                EndNoteDrag();
                return true;
            }
            if (noteEditState == NoteEditState.ResizeNote)
            {
                EndNoteResize();
                return true;
            }
            return false;
        }
        bool TryHandleNoteMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            noteDragPending = false;
            if (noteEditState == NoteEditState.CreateNote)
            {
                CancelCreateNoteMode();
                return true;
            }
            if (aimRect != null || bindingLines != null) return false;
            if (selectedSystem == null || selectedSystem.Notes == null) return false;
            var target = sender as UIElement;
            if (target == null) return false;

            Note note;
            if (!TryHitNoteBody(e.GetPosition(target), out note)) return false;
            if (!note.Selected || GetSelectedNoteCount() != 1)
            {
                SelectSingleNote(note);
                UpdateSelectedStatus();
            }
            OpenNoteColorMenu(target, note);
            return true;
        }
        void RenderNoteInteractionOverlay(Canvas canvas)
        {
            if (canvas == null) return;
            if (noteEditState == NoteEditState.CreateNote && noteCreatePressed)
            {
                var rect = BuildCreateRect(noteDownPoint, noteCurrentPoint);
                var preview = new Border
                {
                    Width = rect.Width,
                    Height = rect.Height,
                    CornerRadius = new CornerRadius(6),
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                    IsHitTestVisible = false
                };
                preview.SetValue(Canvas.LeftProperty, rect.X);
                preview.SetValue(Canvas.TopProperty, rect.Y);
                Panel.SetZIndex(preview, int.MaxValue - 2);
                canvas.Children.Add(preview);
            }
            if (noteEditor != null && noteEditingTarget != null)
            {
                SetNoteEditorLayout();
                if (noteEditor.Parent != canvas)
                {
                    var parent = noteEditor.Parent as Panel;
                    parent?.Children.Remove(noteEditor);
                    canvas.Children.Add(noteEditor);
                }
                Panel.SetZIndex(noteEditor, int.MaxValue - 1);
            }
        }
        #endregion

        #region Window EventHandlers
        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            StartCreateNoteMode();
        }
        private void NoteEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    EndNoteTextEdit(true);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    EndNoteTextEdit(false);
                    e.Handled = true;
                    break;
            }
        }
        private void NoteEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (noteEditorTextSyncing || noteEditor == null) return;
            var normalized = NormalizeNoteComment(noteEditor.Text);
            if (normalized == noteEditor.Text) return;
            var caret = noteEditor.CaretIndex;
            noteEditorTextSyncing = true;
            noteEditor.Text = normalized;
            noteEditor.CaretIndex = Math.Min(caret, normalized.Length);
            noteEditorTextSyncing = false;
        }
        private void NoteEditor_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!noteEndingEditor) EndNoteTextEdit(true);
        }
        private void NoteColorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if (item == null || !(item.Tag is LayerColor) || noteColorTarget == null || selectedSystem == null) return;
            var color = (LayerColor)item.Tag;
            if (noteColorTarget.Color == color) return;
            new SetNoteColorCommand().Do(commandStacks[selectedSystem], noteColorTarget, color);
            UpdateSelectedStatus();
        }
        #endregion
    }
}
