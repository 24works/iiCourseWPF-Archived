using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iiCourse.Core.ViewModels;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 成绩查询视图 - 纯UI层
    /// </summary>
    public partial class ScoreView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ScoreViewModel),
                typeof(ScoreView),
                new PropertyMetadata(null, OnViewModelChanged));

        public ScoreViewModel? ViewModel
        {
            get => (ScoreViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public ScoreView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 页面加载完成后渲染成绩
            RenderScores();
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ScoreView)d;

            // 清理旧ViewModel的事件订阅
            if (e.OldValue is ScoreViewModel oldVm)
            {
                oldVm.Scores.CollectionChanged -= view.OnScoresCollectionChanged;
                oldVm.PropertyChanged -= view.OnViewModelPropertyChanged;
            }

            // 设置DataContext并订阅新ViewModel的事件
            if (e.NewValue is ScoreViewModel newVm)
            {
                view.DataContext = newVm;
                newVm.Scores.CollectionChanged += view.OnScoresCollectionChanged;
                newVm.PropertyChanged += view.OnViewModelPropertyChanged;
                // 初始渲染
                view.RenderScores();
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 当Scores属性被重新赋值时，重新订阅集合并渲染
            if (e.PropertyName == nameof(ScoreViewModel.Scores))
            {
                Debug.WriteLine("[ScoreView] Scores property changed");
                // 重新订阅新集合的事件
                if (ViewModel != null)
                {
                    ViewModel.Scores.CollectionChanged -= OnScoresCollectionChanged;
                    ViewModel.Scores.CollectionChanged += OnScoresCollectionChanged;
                }
                // 立即渲染
                Dispatcher.BeginInvoke(RenderScores);
            }
            // 当IsLoading变化时也刷新UI
            else if (e.PropertyName == nameof(ScoreViewModel.IsLoading))
            {
                Dispatcher.BeginInvoke(RenderScores);
            }
        }

        private void OnScoresCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 在UI线程上执行渲染
            Dispatcher.BeginInvoke(RenderScores);
        }

        /// <summary>
        /// 渲染成绩列表
        /// </summary>
        private void RenderScores()
        {
            var vm = ViewModel;
            if (vm == null)
            {
                Debug.WriteLine("[ScoreView] ViewModel is null");
                return;
            }

            Debug.WriteLine($"[ScoreView] Rendering {vm.Scores.Count} scores, IsLoading={vm.IsLoading}");

            // 清除旧的成绩行（保留表头）
            ClearScoreRows();

            // 如果没有数据，显示提示
            if (vm.Scores.Count == 0)
            {
                AddEmptyRow(vm.IsLoading);
                return;
            }

            // 渲染成绩数据
            int index = 0;
            foreach (var scoreItem in vm.Scores)
            {
                AddScoreRow(scoreItem, index % 2 == 1);
                index++;
            }
        }

        /// <summary>
        /// 清除成绩行（保留表头，移除提示行和数据行）
        /// </summary>
        private void ClearScoreRows()
        {
            // 只保留第1个元素（表头），移除其他所有行
            while (ScorePanel.Children.Count > 1)
            {
                ScorePanel.Children.RemoveAt(1);
            }
        }

        /// <summary>
        /// 添加空数据提示行
        /// </summary>
        private void AddEmptyRow(bool isLoading)
        {
            // 创建提示行
            var rowBorder = new Border
            {
                Style = (Style)FindResource("RowStyle")
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var iconPath = new System.Windows.Shapes.Path
            {
                Width = 18,
                Height = 18,
                Data = (Geometry)FindResource("InfoIcon"),
                Fill = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var messageText = new TextBlock
            {
                Text = isLoading ? "正在加载..." : "暂无成绩数据",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(iconPath);
            stackPanel.Children.Add(messageText);
            rowBorder.Child = stackPanel;
            ScorePanel.Children.Add(rowBorder);
        }

        /// <summary>
        /// 添加单个成绩行
        /// </summary>
        private void AddScoreRow(ScoreItem scoreItem, bool isAlternate)
        {
            // 创建行边框
            var rowBorder = new Border
            {
                Style = isAlternate ? (Style)FindResource("AlternateRowStyle") : (Style)FindResource("RowStyle")
            };

            // 创建网格布局
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star), MinWidth = 120 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 50 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 50 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 50 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 60 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 80 });

            // 课程名称
            var courseNameText = new TextBlock
            {
                Text = scoreItem.CourseName,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(courseNameText, 0);
            grid.Children.Add(courseNameText);

            // 学分
            var creditText = new TextBlock
            {
                Text = scoreItem.Credit,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(creditText, 1);
            grid.Children.Add(creditText);

            // 成绩（根据分数显示不同颜色）
            var scoreBrush = GetScoreColorBrush(scoreItem.ScoreValue);
            var scoreText = new TextBlock
            {
                Text = scoreItem.Score,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = scoreBrush,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(scoreText, 2);
            grid.Children.Add(scoreText);

            // 绩点
            var gpaText = new TextBlock
            {
                Text = scoreItem.GPA,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(gpaText, 3);
            grid.Children.Add(gpaText);

            // 性质
            var natureText = new TextBlock
            {
                Text = scoreItem.Nature,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(natureText, 4);
            grid.Children.Add(natureText);

            // 学期
            var semesterText = new TextBlock
            {
                Text = scoreItem.Semester,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(semesterText, 5);
            grid.Children.Add(semesterText);

            rowBorder.Child = grid;
            ScorePanel.Children.Add(rowBorder);
        }

        /// <summary>
        /// 根据分数获取颜色
        /// </summary>
        private Brush GetScoreColorBrush(double score)
        {
            if (score >= 90)
                return new SolidColorBrush(Color.FromRgb(39, 174, 96)); // 绿色-优秀
            if (score >= 80)
                return new SolidColorBrush(Color.FromRgb(52, 152, 219)); // 蓝色-良好
            if (score >= 60)
                return new SolidColorBrush(Color.FromRgb(243, 156, 18)); // 橙色-及格
            if (score > 0)
                return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // 红色-不及格
            return new SolidColorBrush(Color.FromRgb(127, 140, 141)); // 灰色-无数据
        }
    }
}
