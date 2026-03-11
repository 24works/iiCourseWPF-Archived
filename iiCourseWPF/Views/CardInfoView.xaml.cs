using System.Windows;
using System.Windows.Controls;
using iiCourse.Core.ViewModels;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// 一卡通信息视图 - 纯UI层
    /// </summary>
    public partial class CardInfoView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(CardInfoViewModel),
                typeof(CardInfoView),
                new PropertyMetadata(null, OnViewModelChanged));

        public CardInfoViewModel? ViewModel
        {
            get => (CardInfoViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public CardInfoView()
        {
            InitializeComponent();
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (CardInfoView)d;
            view.DataContext = e.NewValue;
        }
    }
}
