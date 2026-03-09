using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iisdtbu;
using iisdtbu.Models;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 一卡通信息视图
    /// </summary>
    public partial class CardInfoView : UserControl
    {
        private ZHSSService? _service;

        public CardInfoView()
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
        /// 加载一卡通信息
        /// </summary>
        public async Task LoadCardInfoAsync()
        {
            if (_service == null)
            {
                ShowStatus("服务未初始化");
                return;
            }

            try
            {
                SetLoadingState(true);
                ShowStatus("正在加载一卡通信息...");

                var cardInfo = await _service.GetCardInfoAsync();

                if (cardInfo != null)
                {
                    DisplayCardInfo(cardInfo);
                    ShowStatus("一卡通信息加载完成");
                }
                else
                {
                    ShowError("获取一卡通信息失败");
                }
            }
            catch (Exception ex)
            {
                ShowError($"加载一卡通信息时发生错误: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// 显示一卡通信息
        /// </summary>
        private void DisplayCardInfo(CardInfo cardInfo)
        {
            // 显示余额
            BalanceText.Text = cardInfo.余额;

            // 显示上次消费时间
            if (!string.IsNullOrEmpty(cardInfo.上次消费时间))
            {
                LastConsumeText.Text = cardInfo.上次消费时间;
            }
            else
            {
                LastConsumeText.Text = "暂无消费记录";
            }
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        private void ShowError(string message)
        {
            BalanceText.Text = "--";
            LastConsumeText.Text = "--";
            ShowStatus(message);
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
            RefreshButton.Content = isLoading ? "加载中..." : "刷新信息";
        }

        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        private async void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            await LoadCardInfoAsync();
        }
    }
}