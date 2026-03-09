using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iisdtbu;
using iisdtbu.Models;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 课程表视图
    /// </summary>
    public partial class ClassScheduleView : UserControl
    {
        private ZHSSService? _service;
        private List<ClassInfo> _classes = new();

        public ClassScheduleView()
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
        /// 加载课程表
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

                if (_classes.Any())
                {
                    // 确保 UI 布局完成后再显示课程
                    await EnsureLayoutUpdatedAsync();
                    DisplaySchedule();
                    ShowStatus($"共加载 {_classes.Count} 门课程");
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
        /// 显示课程表
        /// </summary>
        private void DisplaySchedule()
        {
            ClearSchedule();

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
                        AddClassToGrid(classInfo, weekday, startPeriod, endPeriod);
                    }
                }
            }
        }

        /// <summary>
        /// 添加课程到网格
        /// </summary>
        private void AddClassToGrid(ClassInfo classInfo, int weekday, int startPeriod, int endPeriod)
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
            var courseName = new TextBlock
            {
                Text = classInfo.KCMC,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(45, 90, 61)),
                Margin = new Thickness(0, 0, 0, 2)
            };

            // 教室
            var classroom = new TextBlock
            {
                Text = classInfo.JXDD,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 2)
            };

            // 教师
            var teacher = new TextBlock
            {
                Text = classInfo.JSXM,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(courseName);
            stackPanel.Children.Add(classroom);
            stackPanel.Children.Add(teacher);
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
            RefreshButton.Content = isLoading ? "加载中..." : "刷新课程表";
        }

        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        private async void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            await LoadClassScheduleAsync();
        }
    }
}