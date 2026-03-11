using System.Windows;
using System.Windows.Controls;
using iiCourse.Core.ViewModels;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 空教室查询视图 - 纯UI层
    /// </summary>
    public partial class SpareClassroomView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(SpareClassroomViewModel),
                typeof(SpareClassroomView),
                new PropertyMetadata(null, OnViewModelChanged));

        public SpareClassroomViewModel? ViewModel
        {
            get => (SpareClassroomViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public SpareClassroomView()
        {
            InitializeComponent();
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (SpareClassroomView)d;
            view.DataContext = e.NewValue;
        }
    }
}
