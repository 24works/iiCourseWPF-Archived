using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using iiCourse.Core;
using iiCourse.Core.Models;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// Spare classroom query view - Display by period dimension
    /// </summary>
    public partial class SpareClassroomView : UserControl
    {
        private iiCoreService? _service;
        private List<SpareClassroom> _classrooms = new();
        private List<BuildingInfo> _buildings = new();
        private Button? _currentCampusButton;
        private Button? _currentBuildingButton;
        private string? _selectedPeriod;
        private string? _currentCampusId;
        private string? _currentBuildingName;

        public SpareClassroomView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set service instance
        /// </summary>
        public void SetService(iiCoreService service)
        {
            _service = service;
        }

        /// <summary>
        /// Campus button click event
        /// </summary>
        private async void OnCampusClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                UpdateCampusButtonStates(button);
                _currentCampusButton = button;
                _currentCampusId = tag;

                await LoadBuildingsAsync(tag);
            }
        }

        /// <summary>
        /// Load building list
        /// </summary>
        private async Task LoadBuildingsAsync(string campusId)
        {
            if (_service == null)
            {
                ShowStatus("Service not initialized");
                return;
            }

            try
            {
                SetLoadingState(true);
                ShowStatus("Loading building list...");

                BuildingButtonsPanel.Children.Clear();
                _currentBuildingButton = null;

                _buildings = await _service.GetBuildingsAsync(campusId);

                if (_buildings.Count > 0)
                {
                    foreach (var building in _buildings)
                    {
                        var btn = CreateBuildingButton(building);
                        BuildingButtonsPanel.Children.Add(btn);
                    }
                    ShowStatus($"Loaded {_buildings.Count} buildings");
                }
                else
                {
                    ShowStatus("No building data for this campus");
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to load buildings: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// Create building button
        /// </summary>
        private Button CreateBuildingButton(BuildingInfo building)
        {
            var button = new Button
            {
                Style = FindResource("SecondaryButtonStyle") as Style,
                Padding = new Thickness(20, 10, 20, 10),
                FontSize = 13,
                Tag = building.ID,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var content = new StackPanel { Orientation = Orientation.Horizontal };

            var icon = new Path
            {
                Width = 16,
                Height = 16,
                Data = FindResource("SchoolIcon") as Geometry,
                Fill = new SolidColorBrush(Color.FromRgb(255, 107, 53)),
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var text = new TextBlock
            {
                Text = building.Name,
                VerticalAlignment = VerticalAlignment.Center
            };

            content.Children.Add(icon);
            content.Children.Add(text);
            button.Content = content;

            button.Click += OnBuildingClick;

            return button;
        }

        /// <summary>
        /// Building button click event
        /// </summary>
        private async void OnBuildingClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                if (int.TryParse(tag, out int buildingId))
                {
                    _currentBuildingButton = button;
                    _selectedPeriod = null;
                    _currentBuildingName = ((button.Content as StackPanel)?.Children[1] as TextBlock)?.Text;
                    await LoadSpareClassroomsAsync(buildingId);
                    UpdateBuildingButtonStates(button);
                }
            }
        }

        /// <summary>
        /// Load spare classroom data
        /// </summary>
        private async Task LoadSpareClassroomsAsync(int buildingId)
        {
            if (_service == null)
            {
                ShowStatus("Service not initialized");
                return;
            }

            try
            {
                SetLoadingState(true);
                ShowStatus("Querying spare classrooms...");

                _classrooms = await _service.GetSpareClassroomAsync(buildingId);

                if (_classrooms.Count > 0)
                {
                    GeneratePeriodFilterButtons();
                    DisplayClassroomsByPeriod();
                    ShowStatus($"Found {_classrooms.Count} spare time slots");
                }
                else
                {
                    ShowEmptyState();
                    PeriodFilterPanel.Visibility = Visibility.Collapsed;
                    ShowStatus("No spare classrooms");
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Query failed: {ex.Message}");
                ShowEmptyState();
                PeriodFilterPanel.Visibility = Visibility.Collapsed;
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// Generate period filter buttons
        /// </summary>
        private void GeneratePeriodFilterButtons()
        {
            PeriodFilterButtons.Children.Clear();
            PeriodFilterPanel.Visibility = Visibility.Visible;

            var availablePeriods = _classrooms
                .Select(c => c.Period)
                .Distinct()
                .OrderBy(p => int.TryParse(p, out var n) ? n : 999)
                .ToList();

            var allButton = CreatePeriodFilterButton("All", null, true);
            PeriodFilterButtons.Children.Add(allButton);

            foreach (var period in availablePeriods)
            {
                var btn = CreatePeriodFilterButton(period, period, false);
                PeriodFilterButtons.Children.Add(btn);
            }
        }

        /// <summary>
        /// Create period filter button
        /// </summary>
        private Button CreatePeriodFilterButton(string displayText, string? periodValue, bool isActive)
        {
            var button = new Button
            {
                Style = FindResource("PeriodFilterButtonStyle") as Style,
                Tag = periodValue,
                Margin = new Thickness(3)
            };

            string buttonText = displayText;
            if (periodValue != null)
            {
                buttonText = $"Period {periodValue}";
            }

            button.Content = new TextBlock
            {
                Text = buttonText,
                FontSize = 12,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            if (isActive)
            {
                SetPeriodButtonActive(button);
            }

            button.Click += OnPeriodFilterClick;
            return button;
        }

        /// <summary>
        /// Period filter button click event
        /// </summary>
        private void OnPeriodFilterClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                _selectedPeriod = button.Tag as string;

                foreach (var child in PeriodFilterButtons.Children)
                {
                    if (child is Button btn)
                    {
                        if (btn == button)
                        {
                            SetPeriodButtonActive(btn);
                        }
                        else
                        {
                            SetPeriodButtonInactive(btn);
                        }
                    }
                }

                DisplayClassroomsByPeriod();
            }
        }

        /// <summary>
        /// Set period button active state
        /// </summary>
        private static void SetPeriodButtonActive(Button button)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(255, 107, 53));
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 107, 53));
            (button.Content as TextBlock)!.Foreground = Brushes.White;
        }

        /// <summary>
        /// Set period button inactive state
        /// </summary>
        private void SetPeriodButtonInactive(Button button)
        {
            button.Background = Brushes.White;
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224));
            (button.Content as TextBlock)!.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
        }

        /// <summary>
        /// Display classrooms by period
        /// </summary>
        private void DisplayClassroomsByPeriod()
        {
            ClassroomByPeriodPanel.Children.Clear();

            var filteredClassrooms = _selectedPeriod == null
                ? _classrooms
                : _classrooms.Where(c => c.Period == _selectedPeriod).ToList();

            var groupedByPeriod = filteredClassrooms
                .GroupBy(c => c.Period)
                .OrderBy(g => int.TryParse(g.Key, out var n) ? n : 999);

            int totalCount = 0;

            foreach (var periodGroup in groupedByPeriod)
            {
                var periodRow = CreatePeriodRow(periodGroup.Key, periodGroup.ToList());
                ClassroomByPeriodPanel.Children.Add(periodRow);
                totalCount += periodGroup.Count();
            }

            ResultCountText.Text = _selectedPeriod == null
                ? $"Total {totalCount} spare time slots"
                : $"Period {_selectedPeriod}: {totalCount} classrooms";
        }

        /// <summary>
        /// Create period row
        /// </summary>
        private Border CreatePeriodRow(string period, List<SpareClassroom> classrooms)
        {
            var border = new Border
            {
                Style = Resources["PeriodRowStyle"] as Style,
                Background = Brushes.White
            };

            var mainStack = new StackPanel();

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var periodBadge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 107, 53)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 12, 0)
            };

            string periodDisplay = GetPeriodDisplayText(period);
            var periodText = new TextBlock
            {
                Text = periodDisplay,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            periodBadge.Child = periodText;
            Grid.SetColumn(periodBadge, 0);

            var separator = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Height = 1,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(separator, 1);

            var countText = new TextBlock
            {
                Text = $"{classrooms.Count} spare classrooms",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(countText, 2);

            headerGrid.Children.Add(periodBadge);
            headerGrid.Children.Add(separator);
            headerGrid.Children.Add(countText);

            mainStack.Children.Add(headerGrid);

            var classroomGrid = new UniformGrid
            {
                Columns = 4,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var byBuilding = classrooms
                .GroupBy(c => c.BuildingName)
                .OrderBy(g => g.Key);

            foreach (var buildingGroup in byBuilding)
            {
                foreach (var classroom in buildingGroup.OrderBy(c => c.ClassroomName))
                {
                    var classroomTag = CreateClassroomTag(classroom);
                    classroomGrid.Children.Add(classroomTag);
                }
            }

            mainStack.Children.Add(classroomGrid);
            border.Child = mainStack;

            return border;
        }

        /// <summary>
        /// Create classroom tag
        /// </summary>
        private static Border CreateClassroomTag(SpareClassroom classroom)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 6, 8, 6),
                Margin = new Thickness(4),
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 230, 201)),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var tooltip = new ToolTip
            {
                Content = $"{classroom.BuildingName}\n{classroom.ClassroomName}"
            };
            border.ToolTip = tooltip;

            var text = new TextBlock
            {
                Text = classroom.ClassroomName,
                FontSize = 12,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(45, 90, 61)),
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.NoWrap
            };

            border.Child = text;
            return border;
        }

        /// <summary>
        /// Get period display text
        /// </summary>
        private string GetPeriodDisplayText(string period)
        {
            if (!int.TryParse(period, out int periodNumber))
                return $"Period {period}";

            if (!string.IsNullOrEmpty(_currentBuildingName))
            {
                return ClassTime.GetPeriodDisplayText(_currentBuildingName, periodNumber);
            }

            return ClassTime.GetPeriodDisplayText(periodNumber);
        }

        /// <summary>
        /// Show empty state
        /// </summary>
        private void ShowEmptyState()
        {
            ClassroomByPeriodPanel.Children.Clear();
            ResultCountText.Text = "";

            var emptyBorder = new Border
            {
                Style = Resources["PeriodRowStyle"] as Style,
                Background = new SolidColorBrush(Color.FromRgb(249, 249, 249))
            };

            var stackPanel = new StackPanel();

            var icon = new TextBlock
            {
                Text = "🔍",
                FontSize = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var text = new TextBlock
            {
                Text = "No spare classrooms",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)),
                TextAlignment = TextAlignment.Center
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(text);

            emptyBorder.Child = stackPanel;
            ClassroomByPeriodPanel.Children.Add(emptyBorder);
        }

        /// <summary>
        /// Update campus button states
        /// </summary>
        private void UpdateCampusButtonStates(Button activeButton)
        {
            ResetButtonStyle(EastCampusButton);
            ResetButtonStyle(WestCampusButton);
            SetActiveButtonStyle(activeButton);
        }

        /// <summary>
        /// Update building button states
        /// </summary>
        private void UpdateBuildingButtonStates(Button activeButton)
        {
            foreach (var child in BuildingButtonsPanel.Children)
            {
                if (child is Button button)
                {
                    ResetButtonStyle(button);
                }
            }
            SetActiveButtonStyle(activeButton);
        }

        /// <summary>
        /// Reset button style
        /// </summary>
        private static void ResetButtonStyle(Button button)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
            button.Foreground = new SolidColorBrush(Color.FromRgb(45, 90, 61));
        }

        /// <summary>
        /// Set active button style
        /// </summary>
        private static void SetActiveButtonStyle(Button button)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(45, 90, 61));
            button.Foreground = Brushes.White;
        }

        /// <summary>
        /// Show status message
        /// </summary>
        private void ShowStatus(string message)
        {
            StatusText.Text = message;
        }

        /// <summary>
        /// Set loading state
        /// </summary>
        private void SetLoadingState(bool isLoading)
        {
            EastCampusButton.IsEnabled = !isLoading;
            WestCampusButton.IsEnabled = !isLoading;

            foreach (var child in BuildingButtonsPanel.Children)
            {
                if (child is Button button)
                {
                    button.IsEnabled = !isLoading;
                }
            }
        }
    }
}
