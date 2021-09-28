// © 2021 Jong-il Hong

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        public FileInfo CurrentFile { get => (FileInfo)GetValue(CurrentFileProperty); private set => SetValue(CurrentFileProperty, value); }
        public Tartan Tartan { get => (Tartan)GetValue(TartanProperty); private set => SetValue(TartanProperty, value); }

        public HistoryState? CurrentHistoryState => undoStack.Last?.Value;
        private HistoryState? lastSavedState = null;

        // Haruby Tartan Plaid proJect
        public const string FileExtension = "htpj";
        static readonly string FileFilter = $"Haruby Tartan Plaid Project|*.{FileExtension}";

        public int MaxUndoRedoCount { get; set; } = 100;

        private bool historyLock = false;

        private readonly LinkedList<HistoryState> undoStack = new(), redoStack = new();

        private readonly string title;

        public MainWindow()
        {
            Tartan = new();

            InitializeComponent();

            title = Title;
        }

        private Task<bool> Save(bool selectFile)
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
                Dispatcher.Invoke(() => Cursor = Cursors.AppStarting);
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
                    Dispatcher.Invoke(() =>
                    {
                        Cursor = Cursors.Arrow;
                        lastSavedState = state;
                    });
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

            Tartan = tartan;
        }

        private void SpoolColorButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;
            ColorWindow colorWindow = new() { Owner = this, SelectedColor = spool.Color, };
            if (colorWindow.ShowDialog() is not true)
            {
                return;
            }
            spool.Color = colorWindow.SelectedColor;
        }

        private void SpoolCopyButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;

            Spool? anchor = (Spool?)SpoolsListBox.SelectedItem;
            if (anchor is null)
            {
                Tartan.Spools.Add(new(spool));
            }
            else
            {
                ObservableCollection<Spool> spools = Tartan.Spools;
                int index = spools.IndexOf(anchor);
                spools.Insert(index, new(spool));
            }
        }

        private void SpoolDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;
            Tartan.Spools.Remove(spool);
        }

        private void AddSpoolButton_Click(object sender, RoutedEventArgs e)
        {
            Spool? anchor = (Spool?)SpoolsListBox.SelectedItem;
            if (anchor is null)
            {
                Tartan.Spools.Add(new());
            }
            else
            {
                ObservableCollection<Spool> spools = Tartan.Spools;
                int index = spools.IndexOf(anchor);
                spools.Insert(index, new());
            }
        }

        private void SpoolUpButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;

            ObservableCollection<Spool> spools = Tartan.Spools;
            int index = spools.IndexOf(spool);
            if (index > 0)
            {
                try
                {
                    historyLock = true;

                    spools.RemoveAt(index);
                    spools.Insert(index - 1, spool);
                }
                finally
                {
                    historyLock = false;
                    ManualUpdated();
                }
            }
        }

        private void SpoolDownButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Spool spool = (Spool)element.Tag;

            ObservableCollection<Spool> spools = Tartan.Spools;
            int index = spools.IndexOf(spool);
            if (index < spools.Count - 1)
            {
                try
                {
                    historyLock = true;

                    spools.RemoveAt(index);
                    spools.Insert(index + 1, spool);
                }
                finally
                {
                    historyLock = false;
                    ManualUpdated();
                }
            }
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
            try
            {
                historyLock = true;

                ObservableCollection<Spool> spools = Tartan.Spools;
                int sourceIndex = spools.IndexOf(source), targetIndex = spools.IndexOf(spool);
                spools.Remove(source);
                spools.Insert(targetIndex, source);
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
        }

        private void ManualUpdated()
        {
            TakeHistory();
            UpdateTartanView();
        }

        private void UpdateTartanView()
        {
            if (MainCanvas is null)
            {
                return;
            }
            MainCanvas.Children.Clear();
            double size = CreateTartanView(Tartan, MainCanvas, 10d, 3);
            MainCanvas.Width = MainCanvas.Height = size;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            HistoryState? state = CurrentHistoryState;
            if (state is null || state == lastSavedState)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show("Any unsaved states will be lost.\nWould you want to save before closing?", "Close", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (!Save(false).Result)
                {
                    e.Cancel = true;
                }
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        public static double CreateTartanView(Tartan tartan, Canvas canvas, double unitWidth, int repeat)
        {
            int patternCount = tartan.Spools.Sum(s => s.Count);
            double patternSize = patternCount * unitWidth;
            double totalSize = patternSize * repeat;

            void ForEach(Action<Spool, double, double> action)
            {
                for (int i = 0; i < repeat; i++)
                {
                    double location = patternSize * i;
                    foreach (Spool spool in tartan.Spools)
                    {
                        double size = unitWidth * spool.Count;
                        action(spool, location, size);
                        location += size;
                    }
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

            return totalSize;
        }

        public static RenderTargetBitmap Render(Canvas canvas)
        {
            DpiScale dpiScale = VisualTreeHelper.GetDpi(canvas);
            RenderTargetBitmap bitmap = new((int)canvas.Width, (int)canvas.Height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY, PixelFormats.Pbgra32);
            bitmap.Render(canvas);
            return bitmap;
        }

        private void Tartan_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            TakeHistory();
            UpdateTartanView();
        }

        private void OnCurrentFileChanged(FileInfo? prev, FileInfo? next)
        {
            string t = title;
            if (next is not null)
            {
                t += " - " + next.FullName;
            }
            Title = t;
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
            ManualUpdated();
            lastSavedState = CurrentHistoryState;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == CurrentFileProperty)
            {
                OnCurrentFileChanged((FileInfo?)e.OldValue, (FileInfo?)e.NewValue);
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

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Open();
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save(false);
        }
        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save(true);
        }
    }
}
