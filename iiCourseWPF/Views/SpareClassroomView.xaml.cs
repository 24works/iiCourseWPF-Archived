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
    /// 空教室查询视图
    /// </summary>
    public partial class SpareClassroomView : UserControl
    {
        private ZHSSService? _service;
        private List<SpareClassroom> _classrooms = new();

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
        /// 教学楼按钮点击事件
        /// </summary>
        private async void OnBuildingClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                if (int.TryParse(tag, out int buildingId))
                {
                    await LoadSpareClassroomsAsync(buildingId);
                    UpdateButtonStates(button);
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
                ShowStatus($"正在查询第{buildingId}教学楼的空教室...");

                _classrooms = await _service.GetSpareClassroomAsync(buildingId);

                if (_classrooms.Any())
                {
                    DisplayClassrooms();
                    ShowStatus($"共找到 {_classrooms.Count} 个空教室");
                }
                else
                {
                    ShowEmptyState();
                    ShowStatus("暂无空教室");
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"查询失败: {ex.Message}");
                ShowEmptyState();
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// 显示空教室列表
        /// </summary>
        private void DisplayClassrooms()
        {
            ClassroomPanel.Children.Clear();

            // 按教室名称分组
            var groupedClassrooms = _classrooms
                .GroupBy(c => c.教室名称)
                .OrderBy(g => g.Key);

            foreach (var group in groupedClassrooms)
            {
                var card = CreateClassroomCard(group.Key, group.ToList());
                ClassroomPanel.Children.Add(card);
            }
        }

        /// <summary>
        /// 创建教室卡片
        /// </summary>
        private Border CreateClassroomCard(string classroomName, List<SpareClassroom> classrooms)
        {
            var border = new Border
            {
                Style = Resources["CardStyle"] as Style,
                Width = 200
            };

            var stackPanel = new StackPanel();

            // 教室图标
            var icon = new TextBlock
            {
                Text = "🏫",
                FontSize = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // 教室名称
            var nameText = new TextBlock
            {
                Text = classroomName,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };

            // 教学楼
            var buildingText = new TextBlock
            {
                Text = classrooms.First().教学楼,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };

            // 可用节次
            var periods = classrooms
                .Select(c => c.节次)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            var periodsText = new TextBlock
            {
                Text = $"可用节次: {string.Join(", ", periods)}",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(buildingText);
            stackPanel.Children.Add(periodsText);

            border.Child = stackPanel;
            return border;
        }

        /// <summary>
        /// 显示空状态
        /// </summary>
        private void ShowEmptyState()
        {
            ClassroomPanel.Children.Clear();

            var emptyCard = new Border
            {
                Style = Resources["CardStyle"] as Style,
                Background = new SolidColorBrush(Color.FromRgb(249, 249, 249)),
                Width = 200
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
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(text);

            emptyCard.Child = stackPanel;
            ClassroomPanel.Children.Add(emptyCard);
        }

        /// <summary>
        /// 更新按钮状态
        /// </summary>
        private void UpdateButtonStates(Button activeButton)
        {
            // 重置所有按钮
            ResetButtonStyle(Building1Button);
            ResetButtonStyle(Building2Button);
            ResetButtonStyle(Building3Button);
            ResetButtonStyle(Building4Button);

            // 设置激活按钮
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
            Building1Button.IsEnabled = !isLoading;
            Building2Button.IsEnabled = !isLoading;
            Building3Button.IsEnabled = !isLoading;
            Building4Button.IsEnabled = !isLoading;
        }
    }
}