using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp1
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Uri _soundUri;
        private readonly Uri _soundUri2;
        private DispatcherTimer _notificationTimer;
        private double _progressValue;
        private bool _isDarkTheme = true;

        public ObservableCollection<TodoItem> Tasks { get; set; } = new ObservableCollection<TodoItem>();

        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            _soundUri = new Uri("Source/sound.mp3", UriKind.Relative);
            _soundUri2 = new Uri("Source/sound2.mp3", UriKind.Relative);
            
            InitializeComponent();
            DataContext = this;
            TaskListLb.ItemsSource = Tasks;
            
            UpdateCounter();

            _notificationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
            _notificationTimer.Tick += NotificationTimer_Tick;
            _notificationTimer.Start();
        }

        private void ToggleThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            
            if (_isDarkTheme)
            {
                // Темная тема
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#2f3864");
                Foreground = Brushes.White;
                TaskListLb.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#4f5781");
                AddTaskBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#4f5781");
                ToggleThemeBtn.Content = "🌓";
            }
            else
            {
                // Светлая тема
                Background = Brushes.White;
                Foreground = Brushes.Black;
                TaskListLb.Background = Brushes.LightGray;
                AddTaskBtn.Background = Brushes.LightGray;
                ToggleThemeBtn.Content = "🌙";
            }
            
            // Обновляем цвет текста в элементах списка
            foreach (var item in TaskListLb.Items)
            {
                if (TaskListLb.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem listBoxItem)
                {
                    listBoxItem.Foreground = _isDarkTheme ? Brushes.White : Brushes.Black;
                }
            }
        }

        private void NotificationTimer_Tick(object sender, EventArgs e)
        {
            if (Tasks.Any(t => !t.IsDone))
            {
                try
                {
                    mediaPlayer.Stop();
                    mediaPlayer.Source = _soundUri;
                    mediaPlayer.Position = TimeSpan.Zero;
                    mediaPlayer.Play();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось воспроизвести звук: {ex.Message}");
                }
                MessageBox.Show("У тебя есть несделаннные привычки", "Делай привычки чел",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            mediaPlayer.Stop();
            mediaPlayer2.Source = _soundUri2;
            mediaPlayer2.Position = TimeSpan.Zero;
            mediaPlayer2.Play();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TaskInputTb.Text))
            {
                Tasks.Add(new TodoItem { Title = TaskInputTb.Text, IsDone = false });
                TaskInputTb.Text = string.Empty;
                UpdateCounter();
                
                // Обновляем цвет нового элемента
                var newItem = TaskListLb.Items[TaskListLb.Items.Count - 1];
                if (TaskListLb.ItemContainerGenerator.ContainerFromItem(newItem) is ListBoxItem listBoxItem)
                {
                    listBoxItem.Foreground = _isDarkTheme ? Brushes.White : Brushes.Black;
                }
            }
        }

        private void DeleteTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TodoItem item)
            {
                Tasks.Remove(item);
                UpdateCounter();
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateCounter();
        }

        public void UpdateCounter()
        {
            int counter = Tasks.Count(t => !t.IsDone);
            CounterTextTbl.Text = $"Осталось привычек: {counter}";
            ProgressValue = Tasks.Count > 0 ? (Tasks.Count(t => t.IsDone) * 100.0 / Tasks.Count) : 0;
        }
    }

    public class TodoItem
    {
        public string Title { get; set; }
        public bool IsDone { get; set; }
    }
    
    public class DoneToTextDecorationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isDone && isDone ? TextDecorations.Strikethrough : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}