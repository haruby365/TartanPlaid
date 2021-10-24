// © 2021 Jong-il Hong

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Haruby.TartanPlaid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty CurrentFileProperty = DependencyProperty.Register(
            nameof(CurrentFile), typeof(FileInfo), typeof(MainWindow));
        public static readonly DependencyProperty TartanProperty = DependencyProperty.Register(
            nameof(Tartan), typeof(Tartan), typeof(MainWindow));
        public static readonly DependencyProperty IsEditedProperty = DependencyProperty.Register(
            nameof(IsEdited), typeof(bool), typeof(MainWindow));

        public FileInfo? CurrentFile { get => (FileInfo)GetValue(CurrentFileProperty); private set => SetValue(CurrentFileProperty, value); }
        public Tartan Tartan { get => (Tartan)GetValue(TartanProperty); private set => SetValue(TartanProperty, value); }
        public bool IsEdited { get => (bool)GetValue(IsEditedProperty); private set => SetValue(IsEditedProperty, value); }

        public HistoryState? CurrentHistoryState => undoStack.Last?.Value;
        private HistoryState? lastSavedState = null;

        // Haruby Tartan Plaid proJect
        public const string FileExtension = "htpj";
        public static readonly string FileFilter = $"HARUBY Tartan Plaid Project|*.{FileExtension}";

        public int MaxUndoRedoCount { get; set; } = 100;

        private bool historyLock = false;

        private readonly LinkedList<HistoryState> undoStack = new(), redoStack = new();

        private AboutWindow? aboutWindow;

        private readonly string title;

        public MainWindow()
        {
            Tartan = new();

            {
                InputGestureCollection gestureCollection = ApplicationCommands.Redo.InputGestures;
                InputGesture? originalGesture = null;
                if (gestureCollection.Count > 0)
                {
                    originalGesture = gestureCollection[0];
                    gestureCollection.Clear();
                }
                gestureCollection.Add(new KeyGesture(Key.Z, ModifierKeys.Control | ModifierKeys.Shift));
                if (originalGesture is not null)
                {
                    gestureCollection.Add(originalGesture);
                }
            }

            InitializeComponent();

            title = Title;
        }

        private Task<bool> Save(bool selectFile, bool allowMainThreadDispatch)
        {
            if (selectFile || CurrentFile is null)
            {
                SaveFileDialog dialog = new() { Filter = FileFilter, };
                if (dialog.ShowDialog() is not true)
                {
                    return Task.FromResult(false);
                }
                CurrentFile = new(dialog.FileName);
            }

            HistoryState? state = CurrentHistoryState;
            if (state is null)
            {
                throw new InvalidOperationException("History is empty.");
            }

            FileInfo fileInfo = CurrentFile;
            string json = state.Json;

            return Task.Run(() =>
            {
                if (allowMainThreadDispatch)
                {
                    Dispatcher.Invoke(() => Cursor = Cursors.AppStarting);
                }
                try
                {
                    File.WriteAllText(fileInfo.FullName, state.Json);
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unexpected error occured.\nFailed to save file.\n\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                finally
                {
                    if (allowMainThreadDispatch)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Cursor = Cursors.Arrow;
                            lastSavedState = state;
                            IsEdited = false;
                        });
                    }
                }
                return true;
            });
        }
        private void Open()
        {
            OpenFileDialog dialog = new() { Filter = FileFilter, };
            if (dialog.ShowDialog() is not true)
            {
                return;
            }

            string json;
            try
            {
                json = File.ReadAllText(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error occured.\nFailed to read file.\n\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Tartan tartan = new();
            try
            {
                tartan.Deserialize(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error occured.\nFailed to open file.\n\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CurrentFile = new(dialog.FileName);
            Tartan = tartan;
        }

        private Color? OpenColorWindow(string title, Color sourceColor)
        {
            ColorWindow colorWindow = new() { Owner = this, Title = title, SourceColor = sourceColor, SelectedColor = sourceColor, OtherColors = Tartan.Spools.Select(s => s.Color).Distinct().Select(c => new ColorItem(c)).ToArray(), };
            if (colorWindow.ShowDialog() is not true)
            {
                return null;
            }
            Color targetColor = colorWindow.SelectedColor;
            return sourceColor == targetColor ? null : targetColor;
        }

        private void SpoolColorButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;
            Color? targetColor = OpenColorWindow("Change Color", spool.Color);
            if (targetColor is null)
            {
                return;
            }
            spool.Color = targetColor.Value;
        }

        private void SpoolCopyButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;

            IReadOnlyList<Spool> spools = Tartan.Spools;
            List<Spool> dest = new(spools.Count + 1);
            dest.AddRange(spools);

            Spool? anchor = (Spool?)SpoolsListBox.SelectedItem;
            if (anchor is null)
            {
                dest.Add(new(spool));
            }
            else
            {
                int index = dest.IndexOf(anchor);
                dest.Insert(index, new(spool));
            }

            Tartan.Spools = dest;
        }

        private void SpoolDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;
            spool.Count--;
        }

        private void SpoolIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;
            spool.Count++;
        }

        private void SpoolDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;

            IReadOnlyList<Spool> spools = Tartan.Spools;
            List<Spool> dest = new(spools.Count);
            dest.AddRange(from s in spools where s != spool select s);

            Tartan.Spools = dest;
        }

        private void AddSpoolButton_Click(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<Spool> spools = Tartan.Spools;
            List<Spool> dest = new(spools.Count);
            dest.AddRange(spools);

            Spool? anchor = (Spool?)SpoolsListBox.SelectedItem;
            if (anchor is null)
            {
                dest.Add(new());
            }
            else
            {
                int index = dest.IndexOf(anchor);
                dest.Insert(index, new());
            }

            Tartan.Spools = dest;
        }

        private void SpoolUpButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;

            IReadOnlyList<Spool> spools = Tartan.Spools;
            List<Spool> dest = new(spools.Count);
            dest.AddRange(spools);

            int index = dest.IndexOf(spool);
            if (index <= 0)
            {
                return;
            }

            dest.RemoveAt(index);
            dest.Insert(index - 1, spool);

            Tartan.Spools = dest;
        }

        private void SpoolDownButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;

            IReadOnlyList<Spool> spools = Tartan.Spools;
            List<Spool> dest = new(spools.Count);
            dest.AddRange(spools);

            int index = dest.IndexOf(spool);
            if (index >= dest.Count - 1)
            {
                return;
            }

            dest.RemoveAt(index);
            dest.Insert(index + 1, spool);

            Tartan.Spools = dest;
        }

        private void SpoolMoveImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;
            DragDrop.DoDragDrop(element, spool, DragDropEffects.Move);
        }

        private void SpoolGrid_Drop(object sender, DragEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;
            Spool source = (Spool)e.Data.GetData(typeof(Spool));
            if (spool == source)
            {
                return;
            }

            IReadOnlyList<Spool> spools = Tartan.Spools;
            List<Spool> dest = new(spools.Count);
            dest.AddRange(spools);

            int targetIndex = dest.IndexOf(spool);
            dest.Remove(source);
            dest.Insert(targetIndex, source);

            Tartan.Spools = dest;
        }

        private void SpoolSwapColorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;
            Color sourceColor = spool.Color;
            Color? targetColor = OpenColorWindow("Swap Color", sourceColor);
            if (targetColor is null)
            {
                return;
            }
            try
            {
                historyLock = true;
                foreach (Spool s in Tartan.Spools)
                {
                    if (s == spool || s.Color == sourceColor)
                    {
                        s.Color = targetColor.Value;
                    }
                }
            }
            finally
            {
                historyLock = false;
                ManualUpdated();
            }
        }

        private void ExportPngMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Tartan tartan = Tartan;
            if (tartan.Spools.Count <= 0)
            {
                MessageBox.Show("Please add any spool first.", "Empty", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog dialog = new() { Filter = "PNG image files|*.png", };
            if (dialog.ShowDialog() is not true)
            {
                return;
            }

            Cursor = Cursors.Wait;
            try
            {
                RenderTargetBitmap bitmap;
                try
                {
                    bitmap = Render(MainCanvas);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unexpected error occured.\nFailed to render image.\n\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                PngBitmapEncoder encoder = new();
                try
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unexpected error occured.\nFailed to create image.\n\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    using Stream stream = File.Create(dialog.FileName);
                    encoder.Save(stream);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unexpected error occured.\nFailed to write file.\n\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            aboutWindow ??= new() { Owner = this, };
            aboutWindow.Show();
        }

        private void TakeHistory()
        {
            if (historyLock)
            {
                return;
            }

            redoStack.Clear();

            string json = Tartan.Serialize();
            undoStack.AddLast(new HistoryState(json));

            while (undoStack.Count > MaxUndoRedoCount)
            {
                undoStack.RemoveFirst();
            }
        }
        private void ClearHistory()
        {
            redoStack.Clear();
            undoStack.Clear();
        }

        private void Undo(int maxCount)
        {
            for (int i = 0; i < maxCount && undoStack.Last is not null; i++)
            {
                LinkedListNode<HistoryState> node = undoStack.Last;
                undoStack.Remove(node);
                redoStack.AddLast(node);
            }
            SyncCurrentState();
        }
        private void Redo(int maxCount)
        {
            for (int i = 0; i < maxCount && redoStack.Last is not null; i++)
            {
                LinkedListNode<HistoryState> node = redoStack.Last;
                redoStack.Remove(node);
                undoStack.AddLast(node);
            }
            SyncCurrentState();
        }
        private void SyncCurrentState()
        {
            HistoryState? state = CurrentHistoryState;
            if (state is null)
            {
                return;
            }
            try
            {
                historyLock = true;
                Tartan.Deserialize(state.Json);
            }
            finally
            {
                historyLock = false;
            }
            IsEdited = state != lastSavedState;
        }

        private void UpdateIsEdited()
        {
            IsEdited = CurrentHistoryState != lastSavedState;
        }

        private void ManualUpdated()
        {
            TakeHistory();
            UpdateIsEdited();
            UpdateTartanView();
        }

        private void UpdateTartanView()
        {
            if (MainCanvas is null)
            {
                return;
            }
            MainCanvas.Children.Clear();
            double size = CreateTartanView(Tartan, MainCanvas);
            MainCanvas.Width = MainCanvas.Height = size;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!IsEdited)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show("Any unsaved states will be lost.\nWould you want to save before closing?", "Close", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (!Save(false, false).Result)
                {
                    e.Cancel = true;
                }
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        public static double CreateTartanView(Tartan tartan, Canvas canvas)
        {
            TartanSettings settings = tartan.Settings ?? TartanSettings.Default;
            int unitWidth = settings.UnitWidth;
            int repeat = settings.Repeat;

            int patternCount = tartan.Spools.Sum(s => s.Count);
            double patternSize = patternCount * unitWidth * 2;
            double totalSize = patternSize * repeat;

            void ForEach(Action<Spool, double, double> action)
            {
                for (int i = 0; i < repeat; i++)
                {
                    double location = patternSize * i;

                    void Loop(IEnumerable<Spool> sources)
                    {
                        foreach (Spool spool in sources)
                        {
                            double size = unitWidth * spool.Count;
                            action(spool, location, size);
                            location += size;
                        }
                    }

                    Loop(tartan.Spools);
                    Loop(tartan.Spools.Reverse());
                }
            }

            ForEach((spool, location, size) =>
            {
                SolidColorBrush brush = new(spool.Color);
                Rectangle horizontal = new() { Height = size, Width = totalSize, Opacity = 0.9d, Fill = brush, };

                Canvas.SetTop(horizontal, location);

                canvas.Children.Add(horizontal);
            });
            ForEach((spool, location, size) =>
            {
                SolidColorBrush brush = new(spool.Color);
                Rectangle vertical = new() { Width = size, Height = totalSize, Opacity = 0.5d, Fill = brush, };

                Canvas.SetLeft(vertical, location);

                canvas.Children.Add(vertical);
            });
            canvas.UpdateLayout();

            return totalSize;
        }

        public static RenderTargetBitmap Render(FrameworkElement element)
        {
            DpiScale dpiScale = VisualTreeHelper.GetDpi(element);
            RenderTargetBitmap bitmap = new((int)element.ActualWidth, (int)element.ActualHeight, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY, PixelFormats.Pbgra32);
            bitmap.Render(element);
            return bitmap;
        }

        private void Tartan_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            TakeHistory();
            UpdateIsEdited();
            UpdateTartanView();
        }

        private void OnCurrentFileOrIsEditedChanged(FileInfo? currentFile, bool isEdited)
        {
            StringBuilder builder = new();
            if (isEdited)
            {
                builder.Append("● ");
            }
            if (currentFile is not null)
            {
                builder.Append(System.IO.Path.GetFileName(currentFile.FullName));
            }
            else
            {
                builder.Append("Untitled");
            }
            builder.Append(" - ");
            builder.Append(title);
            Title = builder.ToString();
        }
        private void OnTartanChanged(Tartan prev, Tartan next)
        {
            if (next is null)
            {
                throw new InvalidOperationException("New tartan is null.");
            }
            else
            {
                next.PropertyChanged += Tartan_PropertyChanged;
            }

            if (prev is not null)
            {
                prev.PropertyChanged -= Tartan_PropertyChanged;
            }
            ClearHistory();
            TakeHistory();
            lastSavedState = CurrentHistoryState;
            UpdateIsEdited();
            UpdateTartanView();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == CurrentFileProperty || e.Property == IsEditedProperty)
            {
                OnCurrentFileOrIsEditedChanged(CurrentFile, IsEdited);
            }
            else if (e.Property == TartanProperty)
            {
                OnTartanChanged((Tartan)e.OldValue, (Tartan)e.NewValue);
            }
        }

        private void AlwaysCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void UndoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = undoStack.Count > 1;
        }
        private void RedoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = redoStack.Count > 0;
        }

        private void UndoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Undo(1);
        }
        private void RedoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Redo(1);
        }

        private void SettingsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new() { Owner = this, SelectedSettings = Tartan.Settings, };
            if (settingsWindow.ShowDialog() is not true)
            {
                return;
            }

            TartanSettings selected = settingsWindow.SelectedSettings;
            if (selected.Equals(Tartan.Settings))
            {
                return;
            }

            Tartan.Settings = selected;
        }

        private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsEdited)
            {
                MessageBoxResult result = MessageBox.Show("Any unsaved states will be lost.\nWould you want to save before creating new?", "New", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (!Save(false, false).Result)
                    {
                        return;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            Tartan = new();
            CurrentFile = null;
        }
        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsEdited)
            {
                MessageBoxResult result = MessageBox.Show("Any unsaved states will be lost.\nWould you want to save before open?", "Open", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (!Save(false, false).Result)
                    {
                        return;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            Open();
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save(false, true);
        }
        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save(true, true);
        }

        static MainWindow()
        {
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
        }
    }
}
