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
        }

        /// <summary>
        /// 密码框密码变更时同步到ViewModel
        /// </summary>
        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null && sender is PasswordBox passwordBox)
            {
                ViewModel.Password = passwordBox.Password;
            }
        }
    }
}
