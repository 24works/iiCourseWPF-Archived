using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iiCourse.Core;
using iiCourse.Core.Models;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 课程表视图
    /// </summary>
    public partial class ClassScheduleView : UserControl
    {
        private iiCoreService? _service;
        private List<ClassInfo> _classes = new();
        private List<SelectedTimeClassInfo> _customClasses = new();
        private List<SchoolYearInfo> _schoolYears = new();
        private WeekDateInfo? _currentWeekDates;
        private int _selectedWeek = 1;
        private bool _isCustomQueryMode = false;

        // 周次按钮列表
        private readonly List<Button> _weekButtons = new();

        public ClassScheduleView()
        {
            InitializeComponent();
            InitializeWeekButtons();
        }

        /// <summary>
        /// 设置服务实例
        /// </summary>
        public void SetService(iiCoreService service)
        {
            _service = service;
        }

        /// <summary>
        /// 初始化周次按钮
        /// </summary>
        private void InitializeWeekButtons()
        {
            WeekButtonsPanel.Children.Clear();
            _weekButtons.Clear();

            for (int i = 1; i <= 20; i++)
            {
                var button = new Button
                {
                    Content = $"第{i}周",
                    Style = Resources["WeekButtonStyle"] as Style,
                    Margin = new Thickness(0, 0, 8, 8),
                    Tag = i
                };
                button.Click += OnWeekButtonClick;
                WeekButtonsPanel.Children.Add(button);
                _weekButtons.Add(button);
            }
        }

        /// <summary>
        /// 加载学年列表
        /// </summary>
        public async Task LoadSchoolYearsAsync()
        {
            if (_service == null) return;

            try
            {
                _schoolYears = await _service.GetSchoolYearsAsync();

                // 清空并重新填充学年下拉框
                YearComboBox.Items.Clear();
                YearComboBox.Items.Add(new ComboBoxItem { Content = "请选择", Tag = "" });

                foreach (var year in _schoolYears)
                {
                    YearComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = year.SCHOOL_YEAR,
                        Tag = year.SCHOOL_YEAR
                    });
                }

                YearComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowStatus($"加载学年列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载课程表（默认当前周）
        /// </summary>
        public async Task LoadClassScheduleAsync()
        {
            if (_service == null)
            {
                ShowStatus("服务未初始化");
                return;
            }

            try
            {
                SetLoadingState(true);
                ShowStatus("正在加载课程表...");

                _classes = await _service.GetClassInfoAsync();
                _isCustomQueryMode = false;

                if (_classes.Any())
                {
                    // 确保 UI 布局完成后再显示课程
                    await EnsureLayoutUpdatedAsync();
                    DisplaySchedule();
                    ShowStatus($"共加载 {_classes.Count} 门课程");
                    DateRangeText.Text = "";
                }
                else
                {
                    ShowStatus("暂无课程信息");
                    ClearSchedule();
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("假期"))
            {
                ShowStatus(ex.Message);
                ClearSchedule();
            }
            catch (Exception ex)
            {
                ShowStatus($"加载课程表失败: {ex.Message}");
                ClearSchedule();
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// 自定义查询课程表
        /// </summary>
        private async Task QueryCustomScheduleAsync()
        {
            if (_service == null)
            {
                ShowStatus("服务未初始化");
                return;
            }

            // 获取选择的参数
            var yearItem = YearComboBox.SelectedItem as ComboBoxItem;
            var semesterItem = SemesterComboBox.SelectedItem as ComboBoxItem;

            var schoolYear = yearItem?.Tag?.ToString();
            var semester = semesterItem?.Tag?.ToString();

            if (string.IsNullOrEmpty(schoolYear) || string.IsNullOrEmpty(semester))
            {
                ShowStatus("请选择学年和学期");
                return;
            }

            var parameters = new CustomQueryParams
            {
                SchoolYear = schoolYear,
                Semester = semester,
                LearnWeek = _selectedWeek.ToString()
            };

            try
            {
                SetLoadingState(true);
                ShowStatus($"正在查询第{_selectedWeek}周课程表...");

                var result = await _service.QueryCustomScheduleAsync(parameters);
                _customClasses = result.classes;
                _currentWeekDates = result.dates;
                _isCustomQueryMode = true;

                if (_customClasses.Any())
                {
                    await EnsureLayoutUpdatedAsync();
                    DisplayCustomSchedule();
                    ShowStatus(result.message);

                    // 显示日期范围
                    if (_currentWeekDates != null)
                    {
                        DateRangeText.Text = $"({_currentWeekDates.Date1} - {_currentWeekDates.Date7})";
                        UpdateDayHeadersWithDates();
                    }
                }
                else
                {
                    ShowStatus("该时间段暂无课程");
                    ClearSchedule();
                    DateRangeText.Text = "";
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"查询失败: {ex.Message}");
                ClearSchedule();
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// 更新表头显示日期
        /// </summary>
        private void UpdateDayHeadersWithDates()
        {
            if (_currentWeekDates == null) return;

            MondayHeader.Text = $"周一\n{_currentWeekDates.Date1}";
            TuesdayHeader.Text = $"周二\n{_currentWeekDates.Date2}";
            WednesdayHeader.Text = $"周三\n{_currentWeekDates.Date3}";
            ThursdayHeader.Text = $"周四\n{_currentWeekDates.Date4}";
            FridayHeader.Text = $"周五\n{_currentWeekDates.Date5}";
            SaturdayHeader.Text = $"周六\n{_currentWeekDates.Date6}";
            SundayHeader.Text = $"周日\n{_currentWeekDates.Date7}";
        }

        /// <summary>
        /// 重置表头
        /// </summary>
        private void ResetDayHeaders()
        {
            MondayHeader.Text = "周一";
            TuesdayHeader.Text = "周二";
            WednesdayHeader.Text = "周三";
            ThursdayHeader.Text = "周四";
            FridayHeader.Text = "周五";
            SaturdayHeader.Text = "周六";
            SundayHeader.Text = "周日";
        }

        /// <summary>
        /// 确保 UI 布局更新完成
        /// </summary>
        private async Task EnsureLayoutUpdatedAsync()
        {
            // 强制立即更新布局
            UpdateLayout();

            // 使用 Task.Yield 让出当前线程，等待 UI 线程完成布局
            await Task.Yield();

            // 再次确保布局更新
            await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// 显示课程表（默认模式）
        /// </summary>
        private void DisplaySchedule()
        {
            ClearSchedule();
            ResetDayHeaders();

            foreach (var classInfo in _classes)
            {
                if (int.TryParse(classInfo.SKXQ, out int weekday) &&
                    int.TryParse(classInfo.SKJC, out int startPeriod))
                {
                    // 获取持续节次
                    int duration = 1;
                    if (int.TryParse(classInfo.CXJC, out int parsedDuration))
                    {
                        duration = parsedDuration;
                    }

                    // 计算结束节次
                    int endPeriod = startPeriod + duration - 1;

                    // 确保在有效范围内
                    if (weekday >= 1 && weekday <= 7 &&
                        startPeriod >= 1 && startPeriod <= 11 &&
                        endPeriod <= 11)
                    {
                        AddClassToGrid(classInfo.KCMC, classInfo.JXDD, classInfo.JSXM, weekday, startPeriod, endPeriod);
                    }
                }
            }
        }

        /// <summary>
        /// 显示自定义查询课程表
        /// </summary>
        private void DisplayCustomSchedule()
        {
            ClearSchedule();

            foreach (var classInfo in _customClasses)
            {
                int weekday = classInfo.SKXQ;
                int startPeriod = classInfo.SKJC;
                int duration = classInfo.CXJC;
                int endPeriod = startPeriod + duration - 1;

                // 确保在有效范围内
                if (weekday >= 1 && weekday <= 7 &&
                    startPeriod >= 1 && startPeriod <= 11 &&
                    endPeriod <= 11)
                {
                    AddClassToGrid(classInfo.KCMC, classInfo.JXDD, classInfo.JSXM, weekday, startPeriod, endPeriod);
                }
            }
        }

        /// <summary>
        /// 添加课程到网格
        /// </summary>
        private void AddClassToGrid(string courseName, string classroom, string teacher, int weekday, int startPeriod, int endPeriod)
        {
            var border = new Border
            {
                Style = Resources["ClassCellStyle"] as Style,
                Margin = new Thickness(1),
                CornerRadius = new CornerRadius(4)
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(4)
            };

            // 课程名称
            var courseNameBlock = new TextBlock
            {
                Text = courseName,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(45, 90, 61)),
                Margin = new Thickness(0, 0, 0, 2)
            };

            // 教室
            var classroomBlock = new TextBlock
            {
                Text = classroom,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 2)
            };

            // 教师
            var teacherBlock = new TextBlock
            {
                Text = teacher,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(courseNameBlock);
            stackPanel.Children.Add(classroomBlock);
            stackPanel.Children.Add(teacherBlock);
            border.Child = stackPanel;

            // 设置网格位置
            Grid.SetRow(border, startPeriod);
            Grid.SetRowSpan(border, endPeriod - startPeriod + 1);
            Grid.SetColumn(border, weekday);

            ScheduleGrid.Children.Add(border);
        }

        /// <summary>
        /// 清空课程表
        /// </summary>
        private void ClearSchedule()
        {
            // 移除所有课程单元格（保留表头和节次标签）
            var toRemove = ScheduleGrid.Children
                .Cast<UIElement>()
                .Where(child => Grid.GetRow(child) > 0 && Grid.GetColumn(child) > 0)
                .ToList();

            foreach (var child in toRemove)
            {
                ScheduleGrid.Children.Remove(child);
            }
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
            RefreshButton.IsEnabled = !isLoading;
            QueryButton.IsEnabled = !isLoading;
            YearComboBox.IsEnabled = !isLoading;
            SemesterComboBox.IsEnabled = !isLoading;

            foreach (var button in _weekButtons)
            {
                button.IsEnabled = !isLoading;
            }
        }

        /// <summary>
        /// 更新周次按钮选中状态
        /// </summary>
        private void UpdateWeekButtonSelection()
        {
            for (int i = 0; i < _weekButtons.Count; i++)
            {
                if (i + 1 == _selectedWeek)
                {
                    _weekButtons[i].Style = Resources["WeekButtonSelectedStyle"] as Style;
                }
                else
                {
                    _weekButtons[i].Style = Resources["WeekButtonStyle"] as Style;
                }
            }
        }

        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        private async void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            if (_isCustomQueryMode)
            {
                // 如果在自定义查询模式，重新查询当前选择的周
                await QueryCustomScheduleAsync();
            }
            else
            {
                // 否则加载默认课程表
                await LoadClassScheduleAsync();
            }
        }

        /// <summary>
        /// 查询按钮点击事件
        /// </summary>
        private async void OnQueryClick(object sender, RoutedEventArgs e)
        {
            await QueryCustomScheduleAsync();
        }

        /// <summary>
        /// 周次按钮点击事件
        /// </summary>
        private async void OnWeekButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int week)
            {
                _selectedWeek = week;
                UpdateWeekButtonSelection();

                // 如果已经选择了学年和学期，自动查询
                var yearItem = YearComboBox.SelectedItem as ComboBoxItem;
                var semesterItem = SemesterComboBox.SelectedItem as ComboBoxItem;

                if (yearItem?.Tag?.ToString() != "" && semesterItem?.Tag?.ToString() != "")
                {
                    await QueryCustomScheduleAsync();
                }
            }
        }

        /// <summary>
        /// 查询参数改变事件
        /// </summary>
        private void OnQueryParamChanged(object sender, SelectionChangedEventArgs e)
        {
            // 可以在这里添加参数改变时的逻辑
        }
    }
}
