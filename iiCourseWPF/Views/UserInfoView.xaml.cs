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
    /// 用户信息视图
    /// </summary>
    public partial class UserInfoView : UserControl
    {
        private ZHSSService? _service;
        private string? _username;

        public UserInfoView()
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
        /// 设置用户名
        /// </summary>
        public void SetUsername(string username)
        {
            _username = username;
        }

        /// <summary>
        /// 加载用户信息
        /// </summary>
        public async Task LoadUserInfoAsync()
        {
            if (_service == null || string.IsNullOrEmpty(_username))
            {
                ShowError("服务未初始化或用户名未设置");
                return;
            }

            try
            {
                SetLoadingState(true);

                var userInfo = await _service.GetUserInfoAsync(_username);

                if (userInfo != null)
                {
                    DisplayUserInfo(userInfo);
                }
                else
                {
                    ShowError("获取用户信息失败");
                }
            }
            catch (Exception ex)
            {
                ShowError($"加载用户信息时发生错误: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// 显示用户信息
        /// </summary>
        private void DisplayUserInfo(UserInfo userInfo)
        {
            NameText.Text = userInfo.姓名;
            CollegeText.Text = userInfo.学院;
            StudentIdText.Text = userInfo.学号;
            NameDetailText.Text = userInfo.姓名;
            GenderText.Text = userInfo.性别;
            CollegeDetailText.Text = userInfo.学院;
            LoginStatusText.Text = "已登录";
            LoginStatusText.Foreground = new SolidColorBrush(Color.FromRgb(78, 205, 196));
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        private void ShowError(string message)
        {
            NameText.Text = "--";
            CollegeText.Text = "--";
            StudentIdText.Text = "--";
            NameDetailText.Text = "--";
            GenderText.Text = "--";
            CollegeDetailText.Text = "--";
            LoginStatusText.Text = "加载失败";
            LoginStatusText.Foreground = System.Windows.Media.Brushes.Red;
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
            await LoadUserInfoAsync();
        }
    }
}