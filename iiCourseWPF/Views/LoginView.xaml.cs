using System.Windows;
using System.Windows.Controls;
using iiCourse.Core.ViewModels;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 登录视图 - 纯UI层，只负责数据绑定
    /// </summary>
    public partial class LoginView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(LoginViewModel),
                typeof(LoginView),
                new PropertyMetadata(null, OnViewModelChanged));

        public LoginViewModel? ViewModel
        {
            get => (LoginViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public LoginView()
        {
            InitializeComponent();
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (LoginView)d;
            view.DataContext = e.NewValue;

            // 如果ViewModel已加载且存在已保存的密码，在密码框中显示占位符
            if (e.NewValue is LoginViewModel viewModel && !string.IsNullOrEmpty(viewModel.Password))
            {
                view.Dispatcher.BeginInvoke(() =>
                {
                    // 显示6个占位符字符，提示用户密码已保存
                    // 使用特殊标记避免同步回ViewModel
                    view.PasswordBox.Tag = "Placeholder";
                    view.PasswordBox.Password = "●●●●●●";
                    view.PasswordBox.Tag = null;
                }, System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        /// <summary>
        /// 密码框密码变更时同步到ViewModel
        /// </summary>
        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null && sender is PasswordBox passwordBox)
            {
                // 如果是初始化占位符，不要同步回ViewModel
                if (passwordBox.Tag?.ToString() == "Placeholder")
                    return;

                ViewModel.Password = passwordBox.Password;
            }
        }
    }
}
