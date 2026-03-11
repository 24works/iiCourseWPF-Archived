using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using iiCourse.Core.ViewModels;
using iiCourseWPF.Views;

namespace iiCourseWPF
{
    /// <summary>
    /// 主窗口 - 纯UI层，负责视图切换和动画
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private UserControl? _currentView;

        public MainWindow()
        {
            InitializeComponent();
            InitializeViewModel();
            InitializeEventHandlers();
        }

        /// <summary>
        /// 初始化ViewModel
        /// </summary>
        private void InitializeViewModel()
        {
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // 绑定ViewModel到各个视图
            LoginView.ViewModel = _viewModel.LoginViewModel;
            UserInfoView.ViewModel = _viewModel.UserInfoViewModel;
            ClassScheduleView.ViewModel = _viewModel.ClassScheduleViewModel;
            ScoreView.ViewModel = _viewModel.ScoreViewModel;
            SpareClassroomView.ViewModel = _viewModel.SpareClassroomViewModel;
            EvaluationView.ViewModel = _viewModel.EvaluationViewModel;

            // 初始显示登录视图
            _ = ShowLoginView();
        }

        /// <summary>
        /// 初始化事件处理器
        /// </summary>
        private void InitializeEventHandlers()
        {
            // 监听ViewModel的CurrentView属性变化
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainViewModel.CurrentView))
                    {
                        _ = OnCurrentViewChanged(_viewModel.CurrentView);
                    }
                    if (e.PropertyName == nameof(MainViewModel.IsLoggedIn) ||
                        e.PropertyName == nameof(MainViewModel.CurrentName) ||
                        e.PropertyName == nameof(MainViewModel.CurrentStudentId))
                    {
                        Sidebar.UpdateLoginStatus(
                            _viewModel.IsLoggedIn,
                            _viewModel.CurrentName ?? "",
                            _viewModel.CurrentStudentId ?? "");
                    }
                };
            }

            // 侧边栏菜单点击事件
            Sidebar.MenuClicked += (menuTag) =>
            {
                _viewModel?.NavigateCommand.Execute(menuTag);
            };

            // 登录完成事件
            if (_viewModel?.LoginViewModel != null)
            {
                _viewModel.LoginViewModel.LoginCompleted += (success, username) =>
                {
                    if (!success)
                    {
                        MessageBox.Show(
                            "登录失败，请检查用户名和密码。",
                            "登录错误",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                };
            }
        }

        /// <summary>
        /// 当前视图变更处理
        /// </summary>
        private async Task OnCurrentViewChanged(string viewName)
        {
            UserControl? targetView = viewName switch
            {
                "Login" => LoginView,
                "UserInfo" => UserInfoView,
                "ClassSchedule" => ClassScheduleView,
                "Score" => ScoreView,
                "SpareClassroom" => SpareClassroomView,
                "Evaluation" => EvaluationView,
                "Settings" => SettingsView,
                "Privacy" => PrivacyView,
                _ => null
            };

            if (targetView != null)
            {
                // 先立即更新侧边栏选中状态，提供即时反馈
                Sidebar.SetActiveMenu(viewName);
                await ShowViewAsync(targetView);
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
                await AnimateViewExitAsync(_currentView);
                _currentView.Visibility = Visibility.Collapsed;
            }

            viewToShow.Visibility = Visibility.Visible;
            _currentView = viewToShow;

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
        /// 窗口关闭事件
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _viewModel?.Dispose();
            base.OnClosed(e);
        }
    }
}
