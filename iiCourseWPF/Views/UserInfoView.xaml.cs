using System.Windows;
using System.Windows.Controls;
using iiCourse.Core.ViewModels;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 用户信息视图 - 纯UI层，只负责数据绑定
    /// </summary>
    public partial class UserInfoView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(UserInfoViewModel),
                typeof(UserInfoView),
                new PropertyMetadata(null, OnViewModelChanged));

        public UserInfoViewModel? ViewModel
        {
            get => (UserInfoViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public UserInfoView()
        {
            InitializeComponent();
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (UserInfoView)d;
            view.DataContext = e.NewValue;
        }
    }
}
