using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iiCourse.Core;
using Newtonsoft.Json.Linq;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 成绩查询视图
    /// </summary>
    public partial class ScoreView : UserControl
    {
        private iiCoreService? _service;

        public ScoreView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 设置服务实例
        /// </summary>
        public void SetService(iiCoreService service)
        {
            _service = service;
        }

        /// <summary>
        /// 加载成绩数据
        /// </summary>
        public async Task LoadScoresAsync()
        {
            if (_service == null)
            {
                ShowStatus("服务未初始化");
                return;
            }

            try
            {
                SetLoadingState(true);
                ShowStatus("正在加载成绩...");

                var scoreData = await _service.GetExamScoreAsync();

                if (!string.IsNullOrEmpty(scoreData))
                {
                    DisplayScores(scoreData);
                    ShowStatus("成绩加载完成");
                }
                else
                {
                    ShowStatus("暂无成绩数据");
                    ShowEmptyState();
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"加载成绩失败: {ex.Message}");
                ShowEmptyState();
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// 显示成绩数据
        /// </summary>
        private void DisplayScores(string scoreData)
        {
            ScorePanel.Children.Clear();

            // 保留表头
            var header = new Border
            {
                Style = Resources["HeaderStyle"] as Style
            };
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            headerGrid.Children.Add(CreateHeaderText("课程名称", 0));
            headerGrid.Children.Add(CreateHeaderText("学分", 1));
            headerGrid.Children.Add(CreateHeaderText("成绩", 2));
            headerGrid.Children.Add(CreateHeaderText("绩点", 3));
            headerGrid.Children.Add(CreateHeaderText("性质", 4));
            headerGrid.Children.Add(CreateHeaderText("学期", 5));

            header.Child = headerGrid;
            ScorePanel.Children.Add(header);

            try
            {
                var jsonArray = JArray.Parse(scoreData);
                int index = 0;

                foreach (var item in jsonArray)
                {
                    var row = CreateScoreRow(item, index);
                    ScorePanel.Children.Add(row);
                    index++;
                }

                if (index == 0)
                {
                    ShowEmptyState();
                }
            }
            catch
            {
                ShowEmptyState();
            }
        }

        /// <summary>
        /// 创建表头文本
        /// </summary>
        private TextBlock CreateHeaderText(string text, int column)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(textBlock, column);
            return textBlock;
        }

        /// <summary>
        /// 创建成绩行
        /// </summary>
        private Border CreateScoreRow(JToken item, int index)
        {
            var border = new Border
            {
                Style = index % 2 == 0 
                    ? Resources["RowStyle"] as Style 
                    : Resources["AlternateRowStyle"] as Style
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var courseName = item["COURSENAME"]?.ToString() ?? "--";
            var credit = item["CREDIT"]?.ToString() ?? "--";
            var score = item["SCORE_NUMERIC"]?.ToString() ?? "--";
            var gpa = "--";
            var nature = item["EXAMPROPERTY"]?.ToString() ?? "--";
            var year = item["XN"]?.ToString() ?? "--";
            var semester = item["XQ"]?.ToString() ?? "--";
            var semesterText = $"{year}-{semester}";

            grid.Children.Add(CreateCellText(courseName, 0));
            grid.Children.Add(CreateCellText(credit, 1, true));
            grid.Children.Add(CreateScoreCell(score, 2, true));
            grid.Children.Add(CreateCellText(gpa, 3, true));
            grid.Children.Add(CreateCellText(nature, 4));
            grid.Children.Add(CreateCellText(semesterText, 5));

            border.Child = grid;
            return border;
        }

        /// <summary>
        /// 创建单元格文本
        /// </summary>
        private TextBlock CreateCellText(string text, int column, bool isCenter = false)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                TextWrapping = TextWrapping.Wrap
            };

            if (isCenter)
            {
                textBlock.TextAlignment = TextAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                textBlock.VerticalAlignment = VerticalAlignment.Center;
            }

            Grid.SetColumn(textBlock, column);
            return textBlock;
        }

        /// <summary>
        /// 创建成绩单元格（带颜色）
        /// </summary>
        private TextBlock CreateScoreCell(string score, int column, bool isCenter = false)
        {
            var textBlock = CreateCellText(score, column, isCenter);
            textBlock.FontWeight = FontWeights.SemiBold;

            // 根据成绩设置颜色
            if (double.TryParse(score, out double scoreValue))
            {
                if (scoreValue >= 90)
                {
                    textBlock.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // 绿色
                }
                else if (scoreValue >= 80)
                {
                    textBlock.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // 蓝色
                }
                else if (scoreValue >= 60)
                {
                    textBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // 橙色
                }
                else
                {
                    textBlock.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // 红色
                }
            }

            return textBlock;
        }

        /// <summary>
        /// 显示空状态
        /// </summary>
        private void ShowEmptyState()
        {
            ScorePanel.Children.Clear();

            var header = new Border
            {
                Style = Resources["HeaderStyle"] as Style
            };
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            headerGrid.Children.Add(CreateHeaderText("课程名称", 0));
            headerGrid.Children.Add(CreateHeaderText("学分", 1));
            headerGrid.Children.Add(CreateHeaderText("成绩", 2));
            headerGrid.Children.Add(CreateHeaderText("绩点", 3));
            headerGrid.Children.Add(CreateHeaderText("性质", 4));
            headerGrid.Children.Add(CreateHeaderText("学期", 5));

            header.Child = headerGrid;
            ScorePanel.Children.Add(header);

            var emptyRow = new Border
            {
                Style = Resources["RowStyle"] as Style
            };
            var emptyText = new TextBlock
            {
                Text = "暂无成绩数据",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)),
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(20)
            };
            emptyRow.Child = emptyText;
            ScorePanel.Children.Add(emptyRow);
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
            RefreshButton.Content = isLoading ? "加载中..." : "刷新成绩";
        }

        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        private async void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            await LoadScoresAsync();
        }
    }
}