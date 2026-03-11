using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using iiCourse.Core;
using iiCourseWPF.Services;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 登录视图
    /// </summary>
    public partial class LoginView : UserControl
    {
        public event Action<bool, string>? LoginCompleted;

        private iiCoreService? _service;
        private readonly CredentialService _credentialService;

        public LoginView()
        {
            InitializeComponent();
            _credentialService = new CredentialService();
            
            // 加载保存的凭据
            LoadSavedCredentials();
            
            UsernameTextBox.Focus();
        }

        /// <summary>
        /// 加载保存的凭据
        /// </summary>
        private void LoadSavedCredentials()
        {
            var credentials = _credentialService.LoadCredentials();
            if (credentials != null)
            {
                // 填充用户名
                UsernameTextBox.Text = credentials.Username;
                
                // 如果之前勾选了记住密码，填充密码并勾选复选框
                if (credentials.RememberPassword && !string.IsNullOrEmpty(credentials.Password))
                {
                    PasswordBox.Password = credentials.Password;
                    RememberPasswordCheckBox.IsChecked = true;
                }
            }
        }

        /// <summary>
        /// 设置服务实例
        /// </summary>
        public void SetService(iiCoreService service)
        {
            _service = service;
            _service.LogCallback = OnLogMessage;
        }

        /// <summary>
        /// 登录按钮点击事件
        /// </summary>
        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;
            var rememberPassword = RememberPasswordCheckBox.IsChecked ?? false;

            if (string.IsNullOrEmpty(username))
            {
                ShowStatus("请输入学号", false);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowStatus("请输入密码", false);
                return;
            }

            await PerformLoginAsync(username, password, rememberPassword);
        }

        /// <summary>
        /// 执行登录操作
        /// </summary>
        private async Task PerformLoginAsync(string username, string password, bool rememberPassword)
        {
            try
            {
                SetLoadingState(true);
                HideStatus();

                if (_service == null)
                {
                    ShowStatus("服务未初始化", false);
                    SetLoadingState(false);
                    return;
                }

                var (success, message) = await _service.LoginAsync(username, password);

                SetLoadingState(false);

                if (success)
                {
                    // 登录成功，保存凭据
                    _credentialService.SaveCredentials(username, password, rememberPassword);
                    
                    ShowStatus("登录成功!", true);
                    await Task.Delay(500);
                    LoginCompleted?.Invoke(true, username);
                }
                else
                {
                    ShowStatus($"登录失败: {message}", false);
                }
            }
            catch (Exception ex)
            {
                SetLoadingState(false);
                ShowStatus($"登录异常: {ex.Message}", false);
            }
        }

        /// <summary>
        /// 设置加载状态
        /// </summary>
        private void SetLoadingState(bool isLoading)
        {
            LoginButton.IsEnabled = !isLoading;
            UsernameTextBox.IsEnabled = !isLoading;
            PasswordBox.IsEnabled = !isLoading;
            RememberPasswordCheckBox.IsEnabled = !isLoading;
            LoadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 显示状态信息
        /// </summary>
        private void ShowStatus(string message, bool isSuccess)
        {
            StatusText.Text = message;
            StatusText.Foreground = isSuccess 
                ? System.Windows.Media.Brushes.Green 
                : System.Windows.Media.Brushes.Red;
            StatusText.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 隐藏状态信息
        /// </summary>
        private void HideStatus()
        {
            StatusText.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 日志消息回调
        /// </summary>
        private void OnLogMessage(string message)
        {
            // 可以在这里显示日志信息
            Console.WriteLine(message);
        }

        /// <summary>
        /// 清空输入框
        /// </summary>
        public void ClearInputs()
        {
            UsernameTextBox.Text = "";
            PasswordBox.Password = "";
            HideStatus();
        }
    }
}
