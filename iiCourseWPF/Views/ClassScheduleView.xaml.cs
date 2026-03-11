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

        /// <summary>
        /// Initializes the ClassScheduleView control, sets up UI components, and creates the week selection buttons.
        /// </summary>
        public ClassScheduleView()
        {
            InitializeComponent();
            InitializeWeekButtons();
        }

        /// <summary>
        /// 设置服务实例
        /// <summary>
        /// Assigns the iiCoreService instance used by this view.
        /// </summary>
        /// <param name="service">The service implementation to use for data operations; may be null to clear the current service.</param>
        public void SetService(iiCoreService service)
        {
            _service = service;
        }

        /// <summary>
        /// 初始化周次按钮
        /// <summary>
        /// Sets up the week-selection UI by clearing existing buttons and creating 20 week buttons (第1周 through 第20周),
        /// applying the week button style, assigning each button its week index in the Tag, wiring the click handler, and adding them to the panel and internal list.
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
        /// <summary>
        /// Load the list of school years from the configured service and populate the YearComboBox selector.
        /// </summary>
        /// <returns>A Task that completes after the YearComboBox has been populated; if the service is null no action is taken.</returns>
        /// <remarks>On failure, a status message is displayed via ShowStatus.</remarks>
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
        /// <summary>
        /// Loads the default class schedule from the configured service and updates the view with the results.
        /// </summary>
        /// <remarks>
        /// Sets the view into a loading state while fetching data, resets custom-query mode, and displays the retrieved classes in the schedule grid.
        /// Updates status messages and the date-range header; clears the displayed schedule and shows an error/status message if no data is available or if loading fails (including holiday conditions indicated by an InvalidOperationException containing "假期").
        /// </remarks>
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
        /// <summary>
        /// Queries and displays the class schedule for the selected school year, semester, and week.
        /// </summary>
        /// <remarks>
        /// Validates year and semester selections and returns early with a status message if invalid or if the service is not set.
        /// While running it toggles the view's loading state, invokes the service to fetch custom schedule data, and on success:
        /// - stores results in internal state (_customClasses, _currentWeekDates) and sets _isCustomQueryMode to true,
        /// - updates the grid by calling DisplayCustomSchedule (after ensuring layout),
        /// - updates the visible date range and weekday headers when available,
        /// - shows status messages from the service.
        /// On empty results it clears the schedule and date range. Exceptions from the request are caught and reported via ShowStatus; loading state is always cleared at the end.
        /// Side effects: updates UI elements and several private fields; does not throw exceptions to callers.
        /// </remarks>
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
        /// <summary>
        /// Update the weekday header labels to show the corresponding dates for the currently loaded week.
        /// </summary>
        /// <remarks>
        /// If no current week dates are available, the method does nothing.
        /// </remarks>
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
        /// <summary>
        /// Restores the weekday header texts to their default Chinese labels ("周一" through "周日").
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
        /// <summary>
        /// Ensures the control's visual layout is fully updated before continuing.
        /// </summary>
        /// <remarks>
        /// Forces a synchronous layout update, yields to allow the UI thread to process pending work, and then waits for the dispatcher at Loaded priority so subsequent code sees the finalized layout.
        /// </remarks>
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
        /// <summary>
        /// Renders the default class schedule from the internal _classes list into the schedule grid.
        /// </summary>
        /// <remarks>
        /// Clears existing course cells and resets weekday headers before rendering. Entries with non-integer or out-of-range weekday, start period, or end period values are skipped.
        /// </remarks>
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
        /// <summary>
        /// Renders schedule cells for the current custom-query results into the schedule grid.
        /// </summary>
        /// <remarks>
        /// Clears the existing schedule and adds a grid cell for each class in the custom-query results whose weekday is Monday (1) through Sunday (7) and whose start and end periods fall within periods 1 through 11.
        /// </remarks>
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
        /// <summary>
        /// Adds a visual class cell to the schedule grid for the specified course and positions it by weekday and period range.
        /// </summary>
        /// <param name="courseName">The course title to display in the cell.</param>
        /// <param name="classroom">The classroom/location text to display beneath the course title.</param>
        /// <param name="teacher">The teacher name to display beneath the classroom.</param>
        /// <param name="weekday">The column index for the weekday, where 1 = Monday through 7 = Sunday.</param>
        /// <param name="startPeriod">The starting row index (period) for the class placement.</param>
        /// <param name="endPeriod">The ending row index (period) for the class placement (inclusive).</param>
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
        /// <summary>
        /// Enable or disable interactive controls in the view based on the loading state.
        /// </summary>
        /// <param name="isLoading">`true` to set the view into a loading state (disables interactive controls); `false` to enable them.</param>
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
        /// <summary>
        /// Updates the visual state of all week buttons so the button for the current selected week uses the selected style and all others use the default week style.
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
        /// <summary>
        /// Re-runs the current schedule load: if custom-query mode is active, re-executes the custom query for the selected week; otherwise reloads the default class schedule.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the routed event.</param>
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
        /// <summary>
        /// Triggers a custom schedule query when the Query button is clicked.
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Event data for the click.</param>
        private async void OnQueryClick(object sender, RoutedEventArgs e)
        {
            await QueryCustomScheduleAsync();
        }

        /// <summary>
        /// 周次按钮点击事件
        /// <summary>
        /// Handles clicks on week buttons: updates the selected week and its visual state, and if a school year and semester are selected, initiates a custom schedule query.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event data for the click.</param>
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
        /// <summary>
        /// Invoked when a query parameter selection (such as school year or semester) changes.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">Event data describing the selection change.</param>
        private void OnQueryParamChanged(object sender, SelectionChangedEventArgs e)
        {
            // 可以在这里添加参数改变时的逻辑
        }
    }
}
