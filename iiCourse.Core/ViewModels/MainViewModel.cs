using System.Windows.Input;
using iiCourse.Core.Commands;
using iiCourse.Core.Models;
using iiCourse.Core.Services;

namespace iiCourse.Core.ViewModels
{
    /// <summary>
    /// 主窗口ViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly iiCoreService _coreService;
        private readonly ICredentialService _credentialService;

        private bool _isLoggedIn;
        private string? _currentUsername;
        private string? _currentName;
        private string? _currentStudentId;
        private string _currentView = "Login";
        private string _statusMessage = string.Empty;
        private bool _isLoading;

        // 子ViewModel
        public LoginViewModel LoginViewModel { get; }
        public UserInfoViewModel UserInfoViewModel { get; }
        public ClassScheduleViewModel ClassScheduleViewModel { get; }
        public ScoreViewModel ScoreViewModel { get; }
        public SpareClassroomViewModel SpareClassroomViewModel { get; }
        public EvaluationViewModel EvaluationViewModel { get; }

        public MainViewModel()
        {
            _coreService = new iiCoreService();
            _credentialService = new CredentialService();

            // 初始化子ViewModel
            LoginViewModel = new LoginViewModel(_coreService, _credentialService);
            UserInfoViewModel = new UserInfoViewModel(_coreService);
            ClassScheduleViewModel = new ClassScheduleViewModel(_coreService);
            ScoreViewModel = new ScoreViewModel(_coreService);
            SpareClassroomViewModel = new SpareClassroomViewModel(_coreService);
            EvaluationViewModel = new EvaluationViewModel(_coreService);

            // 订阅登录完成事件
            LoginViewModel.LoginCompleted += OnLoginCompleted;

            // 初始化命令
            NavigateCommand = new RelayCommand<string>(OnNavigate);

            // 设置日志回调
            _coreService.LogCallback = message => StatusMessage = message;
        }

        #region 属性

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            private set => SetProperty(ref _isLoggedIn, value);
        }

        public string? CurrentUsername
        {
            get => _currentUsername;
            private set => SetProperty(ref _currentUsername, value);
        }

        public string? CurrentName
        {
            get => _currentName;
            private set => SetProperty(ref _currentName, value);
        }

        public string? CurrentStudentId
        {
            get => _currentStudentId;
            private set => SetProperty(ref _currentStudentId, value);
        }

        public string CurrentView
        {
            get => _currentView;
            private set => SetProperty(ref _currentView, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public iiCoreService CoreService => _coreService;

        #endregion

        #region 命令

        public ICommand NavigateCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 导航到指定视图
        /// </summary>
        private void OnNavigate(string? viewName)
        {
            if (string.IsNullOrEmpty(viewName)) return;

            // 隐私政策页面不需要登录即可访问
            if (!IsLoggedIn && viewName != "Settings" && viewName != "Privacy")
            {
                StatusMessage = "请先登录后再使用此功能";
                CurrentView = "Login";
                return;
            }

            CurrentView = viewName;

            // 根据视图加载数据
            _ = LoadViewDataAsync(viewName);
        }

        /// <summary>
        /// 加载视图数据
        /// </summary>
        private async Task LoadViewDataAsync(string viewName)
        {
            try
            {
                IsLoading = true;

                switch (viewName)
                {
                    case "UserInfo":
                        if (!string.IsNullOrEmpty(CurrentUsername))
                        {
                            UserInfoViewModel.Username = CurrentUsername;
                            await UserInfoViewModel.LoadUserInfoAsync();
                        }
                        break;

                    case "ClassSchedule":
                        await ClassScheduleViewModel.LoadSchoolYearsAsync();
                        await ClassScheduleViewModel.LoadClassScheduleAsync();
                        break;

                    case "Score":
                        await ScoreViewModel.LoadSchoolYearsAsync();
                        await ScoreViewModel.LoadScoresAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载数据失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 登录完成处理
        /// </summary>
        private void OnLoginCompleted(bool success, string username)
        {
            if (success)
            {
                IsLoggedIn = true;
                CurrentUsername = username;
                CurrentView = "UserInfo";

                // 加载用户信息
                UserInfoViewModel.Username = username;
                _ = LoadUserInfoAfterLoginAsync();
            }
        }

        /// <summary>
        /// 登录后加载用户信息并更新姓名和学号
        /// </summary>
        private async Task LoadUserInfoAfterLoginAsync()
        {
            await UserInfoViewModel.LoadUserInfoAsync();
            CurrentName = UserInfoViewModel.Name;
            CurrentStudentId = UserInfoViewModel.StudentId;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _coreService?.Dispose();
        }

        #endregion
    }
}
