using System;
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
    /// 评教视图
    /// </summary>
    public partial class EvaluationView : UserControl
    {
        private ZHSSService? _service;
        private ApiResponse<List<StudentReview>>? _reviews;

        public EvaluationView()
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
        /// 加载评教列表按钮点击事件
        /// </summary>
        private async void OnLoadReviewsClick(object sender, RoutedEventArgs e)
        {
            await LoadReviewsAsync();
        }

        /// <summary>
        /// 一键完成评教按钮点击事件
        /// </summary>
        private async void OnFinishAllClick(object sender, RoutedEventArgs e)
        {
            await FinishAllReviewsAsync();
        }

        /// <summary>
        /// 加载评教列表
        /// </summary>
        private async Task LoadReviewsAsync()
        {
            if (_service == null)
            {
                ShowStatus("服务未初始化");
                return;
            }

            try
            {
                SetLoadingState(true);
                ShowStatus("正在加载评教列表...");

                _reviews = await _service.GetStudentReviewsAsync();

                if (_reviews != null && _reviews.Code == 200 && _reviews.Data != null && _reviews.Data.Any())
                {
                    DisplayReviews(_reviews.Data);
                    ShowStatus($"共找到 {_reviews.Data.Count} 个评教任务");
                }
                else
                {
                    ShowEmptyState();
                    ShowStatus("暂无评教任务");
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"加载评教列表失败: {ex.Message}");
                ShowEmptyState();
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// 显示评教列表
        /// </summary>
        private void DisplayReviews(List<StudentReview> reviews)
        {
            ReviewPanel.Children.Clear();

            // 表头
            var header = new Border
            {
                Style = Resources["HeaderStyle"] as Style
            };
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            headerGrid.Children.Add(CreateHeaderText("学年学期", 0));
            headerGrid.Children.Add(CreateHeaderText("评价分类", 1));
            headerGrid.Children.Add(CreateHeaderText("开始时间", 2));
            headerGrid.Children.Add(CreateHeaderText("结束时间", 3));

            header.Child = headerGrid;
            ReviewPanel.Children.Add(header);

            // 评教数据行
            int index = 0;
            foreach (var review in reviews)
            {
                var row = CreateReviewRow(review, index);
                ReviewPanel.Children.Add(row);
                index++;
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
        /// 创建评教行
        /// </summary>
        private Border CreateReviewRow(StudentReview review, int index)
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

            grid.Children.Add(CreateCellText(review.学年学期, 0));
            grid.Children.Add(CreateCellText(review.评价分类, 1));
            grid.Children.Add(CreateCellText(review.开始时间, 2));
            grid.Children.Add(CreateCellText(review.结束时间, 3));

            border.Child = grid;
            return border;
        }

        /// <summary>
        /// 创建单元格文本
        /// </summary>
        private TextBlock CreateCellText(string text, int column)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(textBlock, column);
            return textBlock;
        }

        /// <summary>
        /// 一键完成评教
        /// </summary>
        private async Task FinishAllReviewsAsync()
        {
            if (_service == null)
            {
                ShowStatus("服务未初始化");
                return;
            }

            var result = MessageBox.Show(
                "确定要一键完成所有评教吗？\n\n此操作将自动填写并提交所有评教任务，完成后无法修改。",
                "确认操作",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                SetLoadingState(true);
                ShowStatus("正在完成评教...");

                var finishResult = await _service.FinishStudentReviewsAsync();

                if (finishResult != null && finishResult.Code == 200)
                {
                    var completedCount = finishResult.Data?.Count ?? 0;
                    ShowStatus($"评教完成！共完成 {completedCount} 个评教任务");
                    MessageBox.Show(
                        $"评教完成！\n\n共完成 {completedCount} 个评教任务。",
                        "操作成功",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // 重新加载评教列表
                    await LoadReviewsAsync();
                }
                else
                {
                    ShowStatus($"评教失败: {finishResult?.Message ?? "未知错误"}");
                    MessageBox.Show(
                        $"评教失败: {finishResult?.Message ?? "未知错误"}",
                        "操作失败",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"评教异常: {ex.Message}");
                MessageBox.Show(
                    $"评教异常: {ex.Message}",
                    "操作失败",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// 显示空状态
        /// </summary>
        private void ShowEmptyState()
        {
            ReviewPanel.Children.Clear();

            var emptyRow = new Border
            {
                Style = Resources["RowStyle"] as Style
            };
            var emptyText = new TextBlock
            {
                Text = "暂无评教任务",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)),
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(20)
            };
            emptyRow.Child = emptyText;
            ReviewPanel.Children.Add(emptyRow);
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
            LoadReviewsButton.IsEnabled = !isLoading;
            FinishAllButton.IsEnabled = !isLoading;
            LoadReviewsButton.Content = isLoading ? "加载中..." : "加载评教列表";
            FinishAllButton.Content = isLoading ? "处理中..." : "一键完成评教";
        }
    }
}