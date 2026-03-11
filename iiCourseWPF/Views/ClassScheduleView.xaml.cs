using System.Windows;
using System.Windows.Controls;
using iiCourse.Core.ViewModels;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 课程表视图 - 纯UI层，使用数据绑定
    /// </summary>
    public partial class ClassScheduleView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ClassScheduleViewModel),
                typeof(ClassScheduleView),
                new PropertyMetadata(null, OnViewModelChanged));

        public ClassScheduleViewModel? ViewModel
        {
            get => (ClassScheduleViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public ClassScheduleView()
        {
            InitializeComponent();
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ClassScheduleView)d;
            view.DataContext = e.NewValue;
        }
    }
}
