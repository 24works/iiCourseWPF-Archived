using System.Windows;
using System.Windows.Controls;
using iiCourse.Core.ViewModels;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 评教视图 - 纯UI层
    /// </summary>
    public partial class EvaluationView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(EvaluationViewModel),
                typeof(EvaluationView),
                new PropertyMetadata(null, OnViewModelChanged));

        public EvaluationViewModel? ViewModel
        {
            get => (EvaluationViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public EvaluationView()
        {
            InitializeComponent();
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (EvaluationView)d;
            view.DataContext = e.NewValue;
        }
    }
}
