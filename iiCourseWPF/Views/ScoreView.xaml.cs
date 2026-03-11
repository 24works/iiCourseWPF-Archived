using System.Windows;
using System.Windows.Controls;
using iiCourse.Core.ViewModels;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 成绩查询视图 - 纯UI层
    /// </summary>
    public partial class ScoreView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ScoreViewModel),
                typeof(ScoreView),
                new PropertyMetadata(null, OnViewModelChanged));

        public ScoreViewModel? ViewModel
        {
            get => (ScoreViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public ScoreView()
        {
            InitializeComponent();
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ScoreView)d;
            view.DataContext = e.NewValue;
        }
    }
}
