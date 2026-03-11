using System.Windows.Input;
using iiCourse.Core.Commands;
using iiCourse.Core.Services;

namespace iiCourse.Core.ViewModels
{
    /// <summary>
    /// 登录ViewModel
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private readonly iiCoreService _coreService;
        private readonly ICredentialService _credentialService;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _rememberPassword;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _isStatusError;

        public event Action<bool, string>? LoginCompleted;

        public LoginViewModel(iiCoreService coreService, ICredentialService credentialService)
        {
            _coreService = coreService;
            _credentialService = credentialService;

            // 初始化命令
            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin());

            // 加载保存的凭据
            LoadSavedCredentials();
        }

        #region 属性

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool RememberPassword
        {
            get => _rememberPassword;
            set => SetProperty(ref _rememberPassword, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsStatusError
        {
            get => _isStatusError;
            set => SetProperty(ref _isStatusError, value);
        }

        public bool IsStatusVisible => !string.IsNullOrEmpty(StatusMessage);

        #endregion

        #region 命令

        public ICommand LoginCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 检查是否可以登录
        /// </summary>
        private bool CanLogin()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        /// <summary>
        /// 执行登录
        /// </summary>
        private async Task LoginAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                StatusMessage = string.Empty;

                var (success, message) = await _coreService.LoginAsync(Username.Trim(), Password);

                if (success)
                {
                    // 保存凭据
                    _credentialService.SaveCredentials(Username.Trim(), Password, RememberPassword);

                    StatusMessage = "登录成功!";
                    IsStatusError = false;

                    // 延迟后触发登录完成事件
                    await Task.Delay(500);
                    LoginCompleted?.Invoke(true, Username.Trim());
                }
                else
                {
                    StatusMessage = $"登录失败: {message}";
                    IsStatusError = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"登录异常: {ex.Message}";
                IsStatusError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 加载保存的凭据
        /// </summary>
        private void LoadSavedCredentials()
        {
            var credentials = _credentialService.LoadCredentials();
            if (credentials != null)
            {
                Username = credentials.Username;
                RememberPassword = credentials.RememberPassword;

                if (credentials.RememberPassword && !string.IsNullOrEmpty(credentials.Password))
                {
                    Password = credentials.Password;
                }
            }
        }

        /// <summary>
        /// 清空输入
        /// </summary>
        public void ClearInputs()
        {
            Username = string.Empty;
            Password = string.Empty;
            StatusMessage = string.Empty;
            RememberPassword = false;
        }

        #endregion
    }
}
