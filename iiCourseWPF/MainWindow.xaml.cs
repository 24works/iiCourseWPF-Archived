using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using iiCourse.Core;
using iiCourseWPF.Views;

namespace iiCourseWPF
{
    /// <summary>
    /// 主窗口
    /// </summary>
    public partial class MainWindow : Window
    {
        private iiCoreService? _service;
        private string? _username;
        private bool _isLoggedIn;
        private UserControl? _currentView;

        public MainWindow()
        {
            InitializeComponent();
            InitializeService();
            InitializeEventHandlers();
            _ = ShowLoginView();
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        private void InitializeService()
        {
            _service = new iiCoreService
            {
                LogCallback = message => Console.WriteLine(message)
            };

            // 为所有视图设置服务实例
            LoginView.SetService(_service);
            UserInfoView.SetService(_service);
            ClassScheduleView.SetService(_service);
            ScoreView.SetService(_service);
            SpareClassroomView.SetService(_service);
            CardInfoView.SetService(_service);
            EvaluationView.SetService(_service);
        }

        /// <summary>
        /// 初始化事件处理器
        /// </summary>
        private void InitializeEventHandlers()
        {
            // 登录完成事件
            LoginView.LoginCompleted += async (success, username) =>
            {
                if (success)
                {
                    _username = username;
                    _isLoggedIn = true;

                    // 更新侧边栏状态
                    Sidebar.UpdateLoginStatus(true, username);

                    // 显示用户信息视图
                    await ShowView("UserInfo");

                    // 加载用户信息
                    UserInfoView.SetUsername(username);
                    await UserInfoView.LoadUserInfoAsync();
                }
                else
                {
                    MessageBox.Show(
                        "登录失败，请检查用户名和密码。",
                        "登录错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            };

            // 侧边栏菜单点击事件
            Sidebar.MenuClicked += async (menuTag) =>
            {
                await HandleMenuClick(menuTag);
            };
        }

        /// <summary>
        /// 处理菜单点击
        /// </summary>
        private async Task HandleMenuClick(string menuTag)
        {
            // 隐私政策页面不需要登录即可访问
            if (!_isLoggedIn && menuTag != "Settings" && menuTag != "Privacy")
            {
                MessageBox.Show(
                    "请先登录后再使用此功能。",
                    "未登录",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                await ShowLoginView();
                return;
            }

            await ShowView(menuTag);

            // 根据视图类型加载数据
            switch (menuTag)
            {
                case "UserInfo":
                    if (!string.IsNullOrEmpty(_username))
                    {
                        UserInfoView.SetUsername(_username);
                        await UserInfoView.LoadUserInfoAsync();
                    }
                    break;

                case "ClassSchedule":
                    await ClassScheduleView.LoadSchoolYearsAsync();
                    await ClassScheduleView.LoadClassScheduleAsync();
                    break;

                case "Score":
                    await ScoreView.LoadScoresAsync();
                    break;

                case "CardInfo":
                    await CardInfoView.LoadCardInfoAsync();
                    break;

                case "Evaluation":
                    // 评教视图不需要自动加载，用户手动点击按钮
                    break;
            }
        }

        /// <summary>
        /// 显示登录视图
        /// </summary>
        private async Task ShowLoginView()
        {
            await ShowViewAsync(LoginView);
            Sidebar.SetActiveMenu("UserInfo");
        }

        /// <summary>
        /// 显示指定视图（带动画）
        /// </summary>
        private async Task ShowViewAsync(UserControl viewToShow)
        {
            if (_currentView != null && _currentView != viewToShow)
            {
                // 播放当前页面退出动画
                await AnimateViewExitAsync(_currentView);
                _currentView.Visibility = Visibility.Collapsed;
            }
            
            // 显示新页面
            viewToShow.Visibility = Visibility.Visible;
            _currentView = viewToShow;
            
            // 播放进入动画
            await AnimateViewEnterAsync(viewToShow);
        }

        /// <summary>
        /// 播放视图进入动画
        /// </summary>
        private Task AnimateViewEnterAsync(UserControl view)
        {
            var tcs = new TaskCompletionSource<object>();
            
            var storyboard = (Storyboard)FindResource("PageEnterAnimation");
            storyboard = storyboard.Clone();
            storyboard.Completed += (s, e) => tcs.SetResult(null!);
            storyboard.Begin(view);
            
            return tcs.Task;
        }

        /// <summary>
        /// 播放视图退出动画
        /// </summary>
        private Task AnimateViewExitAsync(UserControl view)
        {
            var tcs = new TaskCompletionSource<object>();
            
            var storyboard = (Storyboard)FindResource("PageExitAnimation");
            storyboard = storyboard.Clone();
            storyboard.Completed += (s, e) => tcs.SetResult(null!);
            storyboard.Begin(view);
            
            return tcs.Task;
        }

        /// <summary>
        /// 显示指定视图
        /// </summary>
        private async Task ShowView(string viewTag)
        {
            UserControl? targetView = viewTag switch
            {
                "UserInfo" => UserInfoView,
                "ClassSchedule" => ClassScheduleView,
                "Score" => ScoreView,
                "SpareClassroom" => SpareClassroomView,
                "CardInfo" => CardInfoView,
                "Evaluation" => EvaluationView,
                "Settings" => SettingsView,
                "Privacy" => PrivacyView,
                _ => null
            };

            if (targetView != null)
            {
                await ShowViewAsync(targetView);
            }
        }

        /// <summary>
        /// 隐藏所有视图
        /// </summary>
        private void HideAllViews()
        {
            LoginView.Visibility = Visibility.Collapsed;
            UserInfoView.Visibility = Visibility.Collapsed;
            ClassScheduleView.Visibility = Visibility.Collapsed;
            ScoreView.Visibility = Visibility.Collapsed;
            SpareClassroomView.Visibility = Visibility.Collapsed;
            CardInfoView.Visibility = Visibility.Collapsed;
            EvaluationView.Visibility = Visibility.Collapsed;
            SettingsView.Visibility = Visibility.Collapsed;
            PrivacyView.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _service?.Dispose();
            base.OnClosed(e);
        }
    }
}