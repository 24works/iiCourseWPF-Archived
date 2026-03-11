using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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

            if (e.OldValue is SpareClassroomViewModel oldVm)
            {
                oldVm.PropertyChanged -= view.ViewModel_PropertyChanged;
                oldVm.Buildings.CollectionChanged -= view.Buildings_CollectionChanged;
                oldVm.PeriodGroups.CollectionChanged -= view.PeriodGroups_CollectionChanged;
            }

            if (e.NewValue is SpareClassroomViewModel newVm)
            {
                newVm.PropertyChanged += view.ViewModel_PropertyChanged;
                newVm.Buildings.CollectionChanged += view.Buildings_CollectionChanged;
                newVm.PeriodGroups.CollectionChanged += view.PeriodGroups_CollectionChanged;
                view.UpdateUI();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                switch (e.PropertyName)
                {
                    case nameof(SpareClassroomViewModel.StatusMessage):
                        StatusText.Text = ViewModel?.StatusMessage ?? "";
                        break;
                    case nameof(SpareClassroomViewModel.ResultCountText):
                        ResultCountText.Text = ViewModel?.ResultCountText ?? "";
                        break;
                    case nameof(SpareClassroomViewModel.AvailablePeriods):
                        UpdatePeriodFilterButtons();
                        break;
                    case nameof(SpareClassroomViewModel.Buildings):
                        // 重新订阅新集合的 CollectionChanged 事件
                        if (ViewModel?.Buildings != null)
                        {
                            ViewModel.Buildings.CollectionChanged -= Buildings_CollectionChanged;
                            ViewModel.Buildings.CollectionChanged += Buildings_CollectionChanged;
                        }
                        UpdateBuildingButtons();
                        break;
                    case nameof(SpareClassroomViewModel.PeriodGroups):
                        // 重新订阅新集合的 CollectionChanged 事件
                        if (ViewModel?.PeriodGroups != null)
                        {
                            ViewModel.PeriodGroups.CollectionChanged -= PeriodGroups_CollectionChanged;
                            ViewModel.PeriodGroups.CollectionChanged += PeriodGroups_CollectionChanged;
                        }
                        UpdateClassroomList();
                        break;
                    case nameof(SpareClassroomViewModel.SelectedPeriod):
                        // 节次筛选改变时刷新按钮样式和列表
                        UpdatePeriodFilterButtons();
                        UpdateClassroomList();
                        break;
                    case nameof(SpareClassroomViewModel.SelectedBuildingId):
                        // 教学楼改变时刷新按钮样式
                        UpdateBuildingButtons();
                        break;
                    case nameof(SpareClassroomViewModel.SelectedCampusId):
                        // 校区改变时刷新按钮样式
                        UpdateCampusButtons();
                        break;
                }
            });
        }

        private void Buildings_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(UpdateBuildingButtons);
        }

        private void PeriodGroups_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(UpdateClassroomList);
        }

        private void UpdateUI()
        {
            UpdateCampusButtons();
            UpdateBuildingButtons();
            UpdatePeriodFilterButtons();
            UpdateClassroomList();
            StatusText.Text = ViewModel?.StatusMessage ?? "";
            ResultCountText.Text = ViewModel?.ResultCountText ?? "";
        }

        private void UpdateCampusButtons()
        {
            CampusButtonsPanel.Children.Clear();

            var filterButtonStyle = (Style)FindResource("FilterButtonStyle");
            var filterButtonSelectedStyle = (Style)FindResource("FilterButtonSelectedStyle");

            // 东校区按钮
            var eastCampusButton = new Button
            {
                Tag = "1",
                Style = ViewModel?.SelectedCampusId == "1" ? filterButtonSelectedStyle : filterButtonStyle,
                Cursor = System.Windows.Input.Cursors.Hand,
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new Path
                        {
                            Width = 14,
                            Height = 14,
                            Data = (Geometry)FindResource("SchoolIcon"),
                            Fill = ViewModel?.SelectedCampusId == "1" ? Brushes.White : (Brush)FindResource("PrimaryColor"),
                            Stretch = Stretch.Uniform,
                            Margin = new Thickness(0, 0, 8, 0),
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = "东校区",
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }
            };
            eastCampusButton.IsEnabled = true;
            eastCampusButton.Click += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("东校区按钮被点击");
                if (ViewModel?.SelectCampusCommand != null)
                {
                    ViewModel.SelectCampusCommand.Execute("1");
                }
            };
            CampusButtonsPanel.Children.Add(eastCampusButton);

            // 西校区按钮
            var westCampusButton = new Button
            {
                Tag = "2",
                Style = ViewModel?.SelectedCampusId == "2" ? filterButtonSelectedStyle : filterButtonStyle,
                Cursor = System.Windows.Input.Cursors.Hand,
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new Path
                        {
                            Width = 14,
                            Height = 14,
                            Data = (Geometry)FindResource("SchoolIcon"),
                            Fill = ViewModel?.SelectedCampusId == "2" ? Brushes.White : (Brush)FindResource("PrimaryColor"),
                            Stretch = Stretch.Uniform,
                            Margin = new Thickness(0, 0, 8, 0),
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = "西校区",
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }
            };
            westCampusButton.IsEnabled = true;
            westCampusButton.Click += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("西校区按钮被点击");
                if (ViewModel?.SelectCampusCommand != null)
                {
                    ViewModel.SelectCampusCommand.Execute("2");
                }
            };
            CampusButtonsPanel.Children.Add(westCampusButton);
        }

        private void UpdateBuildingButtons()
        {
            BuildingButtonsPanel.Children.Clear();

            if (ViewModel?.Buildings == null) return;

            var filterButtonStyle = (Style)FindResource("FilterButtonStyle");
            var filterButtonSelectedStyle = (Style)FindResource("FilterButtonSelectedStyle");

            foreach (var building in ViewModel.Buildings)
            {
                var buildingId = building.Id; // 捕获变量
                var isSelected = ViewModel.SelectedBuildingId == buildingId;

                var button = new Button
                {
                    Content = building.Name,
                    Tag = buildingId,
                    Style = isSelected ? filterButtonSelectedStyle : filterButtonStyle,
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                // 强制启用按钮
                button.IsEnabled = true;

                // 使用 Click 事件替代 Command
                button.Click += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"教学楼按钮被点击: {buildingId}");
                    if (ViewModel?.SelectBuildingCommand != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"执行命令，CanExecute: {ViewModel.SelectBuildingCommand.CanExecute(buildingId)}");
                        ViewModel.SelectBuildingCommand.Execute(buildingId);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("SelectBuildingCommand 为 null");
                    }
                };

                BuildingButtonsPanel.Children.Add(button);
            }

            if (ViewModel?.Buildings.Count > 0)
            {
                StatusText.Text = ViewModel.StatusMessage;
            }
        }

        private void UpdatePeriodFilterButtons()
        {
            PeriodFilterButtons.Children.Clear();

            if (ViewModel?.AvailablePeriods == null || ViewModel.AvailablePeriods.Count == 0)
            {
                PeriodFilterPanel.Visibility = Visibility.Collapsed;
                return;
            }

            PeriodFilterPanel.Visibility = Visibility.Visible;

            var compactFilterButtonStyle = (Style)FindResource("CompactFilterButtonStyle");
            var compactFilterButtonSelectedStyle = (Style)FindResource("CompactFilterButtonSelectedStyle");

            // "全部"按钮 - 当选中节次为空时显示选中状态
            var isAllSelected = string.IsNullOrEmpty(ViewModel?.SelectedPeriod);
            var allButton = new Button
            {
                Content = "全部",
                Style = isAllSelected ? compactFilterButtonSelectedStyle : compactFilterButtonStyle,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            allButton.IsEnabled = true;

            // 使用 Click 事件替代 Command
            allButton.Click += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("全部按钮被点击");
                if (ViewModel?.SelectPeriodCommand != null)
                {
                    ViewModel.SelectPeriodCommand.Execute(null);
                }
            };

            PeriodFilterButtons.Children.Add(allButton);

            foreach (var period in ViewModel?.AvailablePeriods ?? new System.Collections.ObjectModel.ObservableCollection<string>())
            {
                var periodValue = period; // 捕获变量
                var isSelected = ViewModel?.SelectedPeriod == periodValue;

                var button = new Button
                {
                    Content = $"第{period}节",
                    Style = isSelected ? compactFilterButtonSelectedStyle : compactFilterButtonStyle,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                button.IsEnabled = true;

                // 使用 Click 事件替代 Command
                button.Click += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"课时按钮被点击: {periodValue}");
                    if (ViewModel?.SelectPeriodCommand != null)
                    {
                        ViewModel.SelectPeriodCommand.Execute(periodValue);
                    }
                };

                PeriodFilterButtons.Children.Add(button);
            }
        }

        private void UpdateClassroomList()
        {
            ClassroomByPeriodPanel.Children.Clear();

            if (ViewModel?.PeriodGroups == null || ViewModel.PeriodGroups.Count == 0)
            {
                var emptyBorder = new Border
                {
                    Style = (Style)FindResource("PeriodRowStyle"),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F0")!)
                };

                var stackPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var path = new Path
                {
                    Width = 32,
                    Height = 32,
                    Data = (Geometry)FindResource("RoomIcon"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")!),
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(0, 0, 0, 10),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var text = new TextBlock
                {
                    Text = "请选择教学楼查询",
                    FontSize = 12,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")!),
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                stackPanel.Children.Add(path);
                stackPanel.Children.Add(text);
                emptyBorder.Child = stackPanel;
                ClassroomByPeriodPanel.Children.Add(emptyBorder);
                return;
            }

            var filteredGroups = ViewModel.PeriodGroups;

            if (!string.IsNullOrEmpty(ViewModel.SelectedPeriod))
            {
                filteredGroups = new System.Collections.ObjectModel.ObservableCollection<PeriodGroup>(
                    ViewModel.PeriodGroups.Where(g => g.Period == ViewModel.SelectedPeriod));
            }

            foreach (var group in filteredGroups)
            {
                var groupBorder = new Border
                {
                    Style = (Style)FindResource("PeriodRowStyle")
                };

                var groupStack = new StackPanel();

                var headerBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")!),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6, 3, 6, 3),
                    Margin = new Thickness(0, 0, 0, 10),
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                headerBorder.Child = new TextBlock
                {
                    Text = group.DisplayText,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")!)
                };

                groupStack.Children.Add(headerBorder);

                if (group.Classrooms.Count == 0)
                {
                    groupStack.Children.Add(new TextBlock
                    {
                        Text = "该时段无空教室",
                        FontSize = 11,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")!),
                        Margin = new Thickness(0, 5, 0, 0)
                    });
                }
                else
                {
                    var wrapPanel = new WrapPanel();

                    foreach (var classroom in group.Classrooms)
                    {
                        var classroomBorder = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFAFA")!),
                            CornerRadius = new CornerRadius(4),
                            Padding = new Thickness(8, 6, 8, 6),
                            Margin = new Thickness(0, 0, 8, 8),
                            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")!),
                            BorderThickness = new Thickness(1)
                        };

                        var classroomStack = new StackPanel
                        {
                            Orientation = Orientation.Horizontal
                        };

                        var icon = new Path
                        {
                            Width = 12,
                            Height = 12,
                            Data = (Geometry)FindResource("RoomIcon"),
                            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B35")!),
                            Stretch = Stretch.Uniform,
                            Margin = new Thickness(0, 0, 6, 0),
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        var nameText = new TextBlock
                        {
                            Text = classroom.ClassroomName,
                            FontSize = 11,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")!),
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        classroomStack.Children.Add(icon);
                        classroomStack.Children.Add(nameText);
                        classroomBorder.Child = classroomStack;
                        wrapPanel.Children.Add(classroomBorder);
                    }

                    groupStack.Children.Add(wrapPanel);
                }

                groupBorder.Child = groupStack;
                ClassroomByPeriodPanel.Children.Add(groupBorder);
            }

            ResultCountText.Text = ViewModel?.ResultCountText ?? "";
        }
    }
}
