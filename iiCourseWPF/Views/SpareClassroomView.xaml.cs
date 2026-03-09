using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using iiCourse.Core;
using iiCourse.Core.Models;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 空教室查询视图 - 按课时维度展示
    /// </summary>
    public partial class SpareClassroomView : UserControl
    {
        private ZHSSService? _service;
        private List<SpareClassroom> _classrooms = new();
        private List<BuildingInfo> _buildings = new();
        private Button? _currentCampusButton;
        private Button? _currentBuildingButton;
        private string? _selectedPeriod; // 当前选中的课时筛选
        private string? _currentCampusId; // 当前选中的校区ID
        private string? _currentBuildingName; // 当前选中的教学楼名称

        public SpareClassroomView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 设置服务实例
        /// </summary>
        public void SetService(ZHSSService service)
        {
            _service = service;
        }

        /// <summary>
        /// 校区按钮点击事件
        /// </summary>
        private async void OnCampusClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                // 更新校区按钮状态
                UpdateCampusButtonStates(button);
                _currentCampusButton = button;
                _currentCampusId = tag;

                // 加载对应校区的教学楼
                await LoadBuildingsAsync(tag);
            }
        }

        /// <summary>
        /// 加载教学楼列表
        /// </summary>
        private async Task LoadBuildingsAsync(string campusId)
        {
            if (_service == null)
            {
                ShowStatus("服务未初始化");
                return;
            }

            try
            {
                SetLoadingState(true);
                ShowStatus("正在加载教学楼列表...");

                // 清空现有教学楼按钮
                BuildingButtonsPanel.Children.Clear();
                _currentBuildingButton = null;

                // 获取教学楼列表
                _buildings = await _service.GetBuildingsAsync(campusId);

                if (_buildings.Any())
                {
                    // 动态创建教学楼按钮
                    foreach (var building in _buildings)
                    {
                        var btn = CreateBuildingButton(building);
                        BuildingButtonsPanel.Children.Add(btn);
                    }
                    ShowStatus($"已加载 {_buildings.Count} 个教学楼");
                }
                else
                {
                    ShowStatus("该校区暂无教学楼数据");
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"加载教学楼失败: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// 创建教学楼按钮
        /// </summary>
        private Button CreateBuildingButton(BuildingInfo building)
        {
            var button = new Button
            {
                Style = FindResource("SecondaryButtonStyle") as Style,
                Padding = new Thickness(20, 10, 20, 10),
                FontSize = 13,
                Tag = building.ID,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var content = new StackPanel { Orientation = Orientation.Horizontal };

            // 图标
            var icon = new Path
            {
                Width = 16,
                Height = 16,
                Data = FindResource("SchoolIcon") as Geometry,
                Fill = new SolidColorBrush(Color.FromRgb(255, 107, 53)),
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // 文字
            var text = new TextBlock
            {
                Text = building.名称,
                VerticalAlignment = VerticalAlignment.Center
            };

            content.Children.Add(icon);
            content.Children.Add(text);
            button.Content = content;

            button.Click += OnBuildingClick;

            return button;
        }

        /// <summary>
        /// 教学楼按钮点击事件
        /// </summary>
        private async void OnBuildingClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                if (int.TryParse(tag, out int buildingId))
                {
                    _currentBuildingButton = button;
                    _selectedPeriod = null; // 重置课时筛选
                    // 获取教学楼名称用于时间计算
                    _currentBuildingName = ((button.Content as StackPanel)?.Children[1] as TextBlock)?.Text;
                    await LoadSpareClassroomsAsync(buildingId);
                    UpdateBuildingButtonStates(button);
                }
            }
        }

        /// <summary>
        /// 加载空教室数据
        /// </summary>
        private async Task LoadSpareClassroomsAsync(int buildingId)
        {
            if (_service == null)
            {
                ShowStatus("服务未初始化");
                return;
            }

            try
            {
                SetLoadingState(true);
                ShowStatus("正在查询空教室...");

                _classrooms = await _service.GetSpareClassroomAsync(buildingId);

                if (_classrooms.Any())
                {
                    // 生成课时筛选按钮
                    GeneratePeriodFilterButtons();
                    // 显示按课时分组的结果
                    DisplayClassroomsByPeriod();
                    ShowStatus($"共找到 {_classrooms.Count} 个空闲时段");
                }
                else
                {
                    ShowEmptyState();
                    PeriodFilterPanel.Visibility = Visibility.Collapsed;
                    ShowStatus("暂无空教室");
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"查询失败: {ex.Message}");
                ShowEmptyState();
                PeriodFilterPanel.Visibility = Visibility.Collapsed;
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// 生成课时筛选按钮
        /// </summary>
        private void GeneratePeriodFilterButtons()
        {
            PeriodFilterButtons.Children.Clear();
            PeriodFilterPanel.Visibility = Visibility.Visible;

            // 获取所有可用的课时
            var availablePeriods = _classrooms
                .Select(c => c.节次)
                .Distinct()
                .OrderBy(p => int.TryParse(p, out var n) ? n : 999)
                .ToList();

            // 添加"全部"按钮
            var allButton = CreatePeriodFilterButton("全部", null, true);
            PeriodFilterButtons.Children.Add(allButton);

            foreach (var period in availablePeriods)
            {
                var btn = CreatePeriodFilterButton(period, period, false);
                PeriodFilterButtons.Children.Add(btn);
            }
        }

        /// <summary>
        /// 创建课时筛选按钮 - 使用紧凑样式
        /// </summary>
        private Button CreatePeriodFilterButton(string displayText, string? periodValue, bool isActive)
        {
            var button = new Button
            {
                Style = FindResource("PeriodFilterButtonStyle") as Style,
                Tag = periodValue,
                Margin = new Thickness(3)
            };

            // 获取课时显示文本 - 简化为只显示节次
            string buttonText = displayText;
            if (periodValue != null)
            {
                buttonText = $"第{periodValue}节";
            }

            button.Content = new TextBlock
            {
                Text = buttonText,
                FontSize = 12,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            // 设置初始状态
            if (isActive)
            {
                SetPeriodButtonActive(button);
            }

            button.Click += OnPeriodFilterClick;
            return button;
        }

        /// <summary>
        /// 课时筛选按钮点击事件
        /// </summary>
        private void OnPeriodFilterClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                _selectedPeriod = button.Tag as string;

                // 更新所有课时按钮状态
                foreach (var child in PeriodFilterButtons.Children)
                {
                    if (child is Button btn)
                    {
                        if (btn == button)
                        {
                            SetPeriodButtonActive(btn);
                        }
                        else
                        {
                            SetPeriodButtonInactive(btn);
                        }
                    }
                }

                // 刷新显示
                DisplayClassroomsByPeriod();
            }
        }

        /// <summary>
        /// 设置课时按钮为激活状态
        /// </summary>
        private void SetPeriodButtonActive(Button button)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(255, 107, 53));
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 107, 53));
            (button.Content as TextBlock)!.Foreground = Brushes.White;
        }

        /// <summary>
        /// 设置课时按钮为非激活状态
        /// </summary>
        private void SetPeriodButtonInactive(Button button)
        {
            button.Background = Brushes.White;
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224));
            (button.Content as TextBlock)!.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
        }

        /// <summary>
        /// 按课时分组显示空教室
        /// </summary>
        private void DisplayClassroomsByPeriod()
        {
            ClassroomByPeriodPanel.Children.Clear();

            // 筛选数据
            var filteredClassrooms = _selectedPeriod == null
                ? _classrooms
                : _classrooms.Where(c => c.节次 == _selectedPeriod).ToList();

            // 按课时分组
            var groupedByPeriod = filteredClassrooms
                .GroupBy(c => c.节次)
                .OrderBy(g => int.TryParse(g.Key, out var n) ? n : 999);

            int totalCount = 0;

            foreach (var periodGroup in groupedByPeriod)
            {
                var periodRow = CreatePeriodRow(periodGroup.Key, periodGroup.ToList());
                ClassroomByPeriodPanel.Children.Add(periodRow);
                totalCount += periodGroup.Count();
            }

            // 更新结果计数
            ResultCountText.Text = _selectedPeriod == null
                ? $"共 {totalCount} 个空闲时段"
                : $"第{_selectedPeriod}节 共 {totalCount} 个教室";
        }

        /// <summary>
        /// 创建课时行
        /// </summary>
        private Border CreatePeriodRow(string period, List<SpareClassroom> classrooms)
        {
            var border = new Border
            {
                Style = Resources["PeriodRowStyle"] as Style,
                Background = Brushes.White
            };

            var mainStack = new StackPanel();

            // 课时标题行
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // 课时标签
            var periodBadge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 107, 53)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 12, 0)
            };

            // 使用 ClassTime 获取课时显示文本
            string periodDisplay = GetPeriodDisplayText(period);
            var periodText = new TextBlock
            {
                Text = periodDisplay,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            periodBadge.Child = periodText;
            Grid.SetColumn(periodBadge, 0);

            // 分隔线
            var separator = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Height = 1,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(separator, 1);

            // 教室数量
            var countText = new TextBlock
            {
                Text = $"{classrooms.Count} 间空教室",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(countText, 2);

            headerGrid.Children.Add(periodBadge);
            headerGrid.Children.Add(separator);
            headerGrid.Children.Add(countText);

            mainStack.Children.Add(headerGrid);

            // 教室列表 - 使用UniformGrid实现响应式布局
            var classroomGrid = new UniformGrid
            {
                Columns = 4,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 12, 0, 0)
            };

            // 按教学楼分组显示教室
            var byBuilding = classrooms
                .GroupBy(c => c.教学楼)
                .OrderBy(g => g.Key);

            foreach (var buildingGroup in byBuilding)
            {
                foreach (var classroom in buildingGroup.OrderBy(c => c.教室名称))
                {
                    var classroomTag = CreateClassroomTag(classroom);
                    classroomGrid.Children.Add(classroomTag);
                }
            }

            mainStack.Children.Add(classroomGrid);
            border.Child = mainStack;

            return border;
        }

        /// <summary>
        /// 创建教室标签 - 响应式布局
        /// </summary>
        private Border CreateClassroomTag(SpareClassroom classroom)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 6, 8, 6),
                Margin = new Thickness(4),
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 230, 201)),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var tooltip = new ToolTip
            {
                Content = $"{classroom.教学楼}\n{classroom.教室名称}"
            };
            border.ToolTip = tooltip;

            var text = new TextBlock
            {
                Text = classroom.教室名称,
                FontSize = 12,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(45, 90, 61)),
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.NoWrap
            };

            border.Child = text;
            return border;
        }

        /// <summary>
        /// 获取课时显示文本，根据当前教学楼自动判断时间类型
        /// </summary>
        private string GetPeriodDisplayText(string period)
        {
            if (!int.TryParse(period, out int periodNumber))
                return $"第{period}节";

            // 根据教学楼名称和校区获取时间显示
            if (!string.IsNullOrEmpty(_currentBuildingName))
            {
                return ClassTime.GetPeriodDisplayText(_currentBuildingName, periodNumber);
            }

            // 默认使用 TypeA 的时间
            return ClassTime.GetPeriodDisplayText(periodNumber);
        }

        /// <summary>
        /// 显示空状态
        /// </summary>
        private void ShowEmptyState()
        {
            ClassroomByPeriodPanel.Children.Clear();
            ResultCountText.Text = "";

            var emptyBorder = new Border
            {
                Style = Resources["PeriodRowStyle"] as Style,
                Background = new SolidColorBrush(Color.FromRgb(249, 249, 249))
            };

            var stackPanel = new StackPanel();

            var icon = new TextBlock
            {
                Text = "🔍",
                FontSize = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var text = new TextBlock
            {
                Text = "暂无空教室",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)),
                TextAlignment = TextAlignment.Center
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(text);

            emptyBorder.Child = stackPanel;
            ClassroomByPeriodPanel.Children.Add(emptyBorder);
        }

        /// <summary>
        /// 更新校区按钮状态
        /// </summary>
        private void UpdateCampusButtonStates(Button activeButton)
        {
            ResetButtonStyle(EastCampusButton);
            ResetButtonStyle(WestCampusButton);
            SetActiveButtonStyle(activeButton);
        }

        /// <summary>
        /// 更新教学楼按钮状态
        /// </summary>
        private void UpdateBuildingButtonStates(Button activeButton)
        {
            foreach (var child in BuildingButtonsPanel.Children)
            {
                if (child is Button button)
                {
                    ResetButtonStyle(button);
                }
            }
            SetActiveButtonStyle(activeButton);
        }

        /// <summary>
        /// 重置按钮样式
        /// </summary>
        private void ResetButtonStyle(Button button)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
            button.Foreground = new SolidColorBrush(Color.FromRgb(45, 90, 61));
        }

        /// <summary>
        /// 设置激活按钮样式
        /// </summary>
        private void SetActiveButtonStyle(Button button)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(45, 90, 61));
            button.Foreground = Brushes.White;
        }

        /// <summary>
        /// 显示状态信息
        /// </summary>
        private void ShowStatus(string message)
        {
            StatusText.Text = message;
        }

        /// <summary>
        /// 设置加载状态
        /// </summary>
        private void SetLoadingState(bool isLoading)
        {
            EastCampusButton.IsEnabled = !isLoading;
            WestCampusButton.IsEnabled = !isLoading;

            foreach (var child in BuildingButtonsPanel.Children)
            {
                if (child is Button button)
                {
                    button.IsEnabled = !isLoading;
                }
            }
        }
    }
}
