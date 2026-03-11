using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using iiCourse.Core.ViewModels;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 课程表视图 - 纯UI层
    /// </summary>
    public partial class ClassScheduleView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ClassScheduleViewModel),
                typeof(ClassScheduleView),
                new PropertyMetadata(null, OnViewModelChanged));

        public ClassScheduleViewModel? ViewModel
        {
            get => (ClassScheduleViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public ClassScheduleView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 页面加载完成后渲染课程
            RenderClasses();
            // 生成周次选择按钮
            GenerateWeekButtons();
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ClassScheduleView)d;

            // 清理旧ViewModel的事件订阅
            if (e.OldValue is ClassScheduleViewModel oldVm)
            {
                oldVm.Classes.CollectionChanged -= view.OnClassesCollectionChanged;
                oldVm.PropertyChanged -= view.OnViewModelPropertyChanged;
            }

            // 设置DataContext并订阅新ViewModel的事件
            if (e.NewValue is ClassScheduleViewModel newVm)
            {
                view.DataContext = newVm;
                newVm.Classes.CollectionChanged += view.OnClassesCollectionChanged;
                newVm.PropertyChanged += view.OnViewModelPropertyChanged;
                // 初始渲染
                view.RenderClasses();
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 当Classes属性被重新赋值时，重新订阅集合并渲染
            if (e.PropertyName == nameof(ClassScheduleViewModel.Classes))
            {
                if (ViewModel != null)
                {
                    // 重新订阅新集合的事件
                    ViewModel.Classes.CollectionChanged -= OnClassesCollectionChanged;
                    ViewModel.Classes.CollectionChanged += OnClassesCollectionChanged;
                    // 立即渲染
                    Dispatcher.Invoke(RenderClasses);
                }
            }
        }

        private void OnClassesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 在UI线程上执行渲染
            Dispatcher.Invoke(RenderClasses);
        }

        /// <summary>
        /// 渲染课程到表格
        /// </summary>
        private void RenderClasses()
        {
            var vm = ViewModel;
            if (vm == null)
            {
                Debug.WriteLine("[ClassScheduleView] ViewModel is null");
                return;
            }

            Debug.WriteLine($"[ClassScheduleView] Rendering {vm.Classes.Count} classes");

            // 清除旧的课程单元格（保留表头和节次标签）
            ClearClassCells();

            // 渲染新课程
            foreach (var classItem in vm.Classes)
            {
                Debug.WriteLine($"[ClassScheduleView] Adding class: {classItem.CourseName}, Weekday={classItem.Weekday}, StartPeriod={classItem.StartPeriod}, Duration={classItem.Duration}");
                AddClassCell(classItem);
            }
        }

        /// <summary>
        /// 清除课程单元格
        /// </summary>
        private void ClearClassCells()
        {
            // 保留前12行（表头+11个节次标签）
            // 课程单元格是动态添加的，我们需要识别并移除它们
            var cellsToRemove = new List<UIElement>();

            foreach (UIElement child in ScheduleGrid.Children)
            {
                if (child is Border border && border.Tag?.ToString() == "ClassCell")
                {
                    cellsToRemove.Add(child);
                }
            }

            foreach (var cell in cellsToRemove)
            {
                ScheduleGrid.Children.Remove(cell);
            }

            Debug.WriteLine($"[ClassScheduleView] Cleared {cellsToRemove.Count} old cells");
        }

        /// <summary>
        /// 添加单个课程单元格
        /// </summary>
        private void AddClassCell(ScheduleClassItem classItem)
        {
            // 计算行列位置
            // 行：StartPeriod (1-11) -> Row = StartPeriod
            // 列：Weekday (1-7) -> Column = Weekday
            int row = classItem.StartPeriod;
            int column = classItem.Weekday;
            int rowSpan = classItem.Duration;

            // 验证范围
            if (row < 1 || row > 11 || column < 1 || column > 7)
            {
                Debug.WriteLine($"[ClassScheduleView] Invalid position: row={row}, column={column}");
                return;
            }

            // 创建课程卡片
            var classBorder = new Border
            {
                Tag = "ClassCell",
                Style = (Style)FindResource("ClassCellStyle"),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(2),
                Padding = new Thickness(4),
                Background = GetClassColor(classItem.CourseName),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 200, 150)),
                BorderThickness = new Thickness(1)
            };

            // 课程信息面板
            var stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            // 课程名称
            var nameText = new TextBlock
            {
                Text = classItem.CourseName,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 60, 20)),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 2)
            };

            // 教室
            var roomText = new TextBlock
            {
                Text = classItem.Classroom,
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 80, 40)),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 1)
            };

            // 教师
            var teacherText = new TextBlock
            {
                Text = classItem.Teacher,
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 80, 40)),
                TextAlignment = TextAlignment.Center
            };

            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(roomText);
            stackPanel.Children.Add(teacherText);

            classBorder.Child = stackPanel;

            // 设置行列位置
            Grid.SetRow(classBorder, row);
            Grid.SetColumn(classBorder, column);
            Grid.SetRowSpan(classBorder, rowSpan);

            ScheduleGrid.Children.Add(classBorder);
            Debug.WriteLine($"[ClassScheduleView] Added cell at row={row}, column={column}, rowspan={rowSpan}");
        }

        /// <summary>
        /// 根据课程名获取颜色（保持一致性）
        /// </summary>
        private Brush GetClassColor(string courseName)
        {
            // 基于课程名生成一致的颜色
            var colors = new[]
            {
                Color.FromRgb(255, 235, 210), // 浅橙
                Color.FromRgb(255, 240, 220), // 浅黄
                Color.FromRgb(255, 230, 225), // 浅粉
                Color.FromRgb(235, 245, 255), // 浅蓝
                Color.FromRgb(230, 255, 235), // 浅绿
                Color.FromRgb(245, 235, 255), // 浅紫
                Color.FromRgb(255, 245, 230), // 浅杏
                Color.FromRgb(240, 255, 250), // 浅青
            };

            int hash = 0;
            foreach (char c in courseName)
            {
                hash = ((hash << 5) - hash) + c;
                hash = hash & hash;
            }

            var color = colors[Math.Abs(hash) % colors.Length];
            return new SolidColorBrush(color);
        }

        /// <summary>
        /// 生成周次选择按钮（1-20周）
        /// </summary>
        private void GenerateWeekButtons()
        {
            if (WeekButtonsPanel == null) return;

            WeekButtonsPanel.Children.Clear();

            for (int week = 1; week <= 20; week++)
            {
                var button = new Button
                {
                    Content = $"第{week}周",
                    Tag = week,
                    Style = (Style)FindResource("WeekButtonStyle"),
                    Margin = new Thickness(0, 0, 6, 6)
                };

                button.Click += OnWeekButtonClick;
                WeekButtonsPanel.Children.Add(button);
            }

            Debug.WriteLine("[ClassScheduleView] Generated 20 week buttons");
        }

        /// <summary>
        /// 周次按钮点击事件
        /// </summary>
        private void OnWeekButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int week)
            {
                // 更新按钮样式
                foreach (UIElement child in WeekButtonsPanel.Children)
                {
                    if (child is Button btn)
                    {
                        btn.Style = (Style)FindResource("WeekButtonStyle");
                    }
                }

                // 设置选中样式
                button.Style = (Style)FindResource("WeekButtonSelectedStyle");

                // 执行选择周次命令
                if (ViewModel?.SelectWeekCommand is ICommand command && command.CanExecute(week))
                {
                    command.Execute(week);
                }

                Debug.WriteLine($"[ClassScheduleView] Selected week: {week}");
            }
        }
    }
}
