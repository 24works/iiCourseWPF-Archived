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

        /// <summary>
        /// Initializes a new instance of the <see cref="SpareClassroomView"/> class and constructs its UI components.
        /// </summary>
        public SpareClassroomView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set service instance
        /// <summary>
        /// Stores the provided iiCoreService instance for use by the view's data-loading operations.
        /// </summary>
        /// <param name="service">An iiCoreService implementation used to fetch buildings and spare-classroom data.</param>
        public void SetService(iiCoreService service)
        {
            _service = service;
        }

        /// <summary>
        /// Campus button click event
        /// <summary>
        /// Handles campus button clicks and initiates loading of buildings for the selected campus.
        /// </summary>
        /// <param name="sender">The clicked Button; its Tag is expected to contain the campus ID as a string.</param>
        /// <param name="e">Event data for the click event.</param>
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
        /// <summary>
        /// Load and display building buttons for the specified campus.
        /// </summary>
        /// <param name="campusId">The identifier of the campus whose buildings should be loaded.</param>
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
        /// <summary>
        /// Create a selectable Button representing the specified building for the UI.
        /// </summary>
        /// <param name="building">The building information used to populate the button's tag and displayed label.</param>
        /// <returns>A Button configured for the building (icon and name content), with its Tag set to the building ID and the click handler attached.</returns>
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
        /// <summary>
        /// Handles building button clicks and loads spare classrooms for the selected building.
        /// </summary>
        /// <param name="sender">The clicked Button; its Tag must be a string containing the building ID.</param>
        /// <param name="e">Event data for the click.</param>
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
        /// <summary>
        /// Loads spare classroom data for the specified building and updates the view (period filters, classroom list, and status) based on the query result.
        /// </summary>
        /// <param name="buildingId">Identifier of the building to query for spare classrooms.</param>
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
        /// <summary>
        /// Builds and shows the period filter UI based on the current spare-classroom data.
        /// </summary>
        /// <remarks>
        /// Clears existing period filter buttons, makes the period filter panel visible, adds an active "All" button,
        /// then adds one button for each distinct period found in _classrooms ordered by numeric value when possible.
        /// </remarks>
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
        /// <summary>
        /// Creates a period filter button used to select or display a specific period (or "All") in the UI.
        /// </summary>
        /// <param name="displayText">Base text to show on the button; ignored when <paramref name="periodValue"/> is non-null, in which case the label becomes "Period X".</param>
        /// <param name="periodValue">The period identifier stored in the button's Tag; pass null to represent the "All" filter.</param>
        /// <param name="isActive">If true, applies the active visual style to the created button.</param>
        /// <returns>The configured Button representing the period filter.</returns>
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
        /// <summary>
        /// Sets the selected period from the clicked period-filter button, updates filter button visual states, and refreshes the displayed classrooms.
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
        /// <summary>
        /// Apply the active visual style to a period filter button.
        /// </summary>
        /// <param name="button">The button to mark as active; its background, border, and text color will be updated.</param>
        private static void SetPeriodButtonActive(Button button)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(255, 107, 53));
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 107, 53));
            (button.Content as TextBlock)!.Foreground = Brushes.White;
        }

        /// <summary>
        /// Set period button inactive state
        /// <summary>
        /// Apply the inactive visual style to a period filter button.
        /// </summary>
        /// <param name="button">The period filter Button to style as inactive (resets background, border, and text color).</param>
        private void SetPeriodButtonInactive(Button button)
        {
            button.Background = Brushes.White;
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224));
            (button.Content as TextBlock)!.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
        }

        /// <summary>
        /// Display classrooms by period
        /// <summary>
        /// Populate the UI with spare classrooms grouped by period and update the result count.
        /// </summary>
        /// <remarks>
        /// Filters the internal classroom list by <c>_selectedPeriod</c> (uses all when null), groups the filtered items by their <c>Period</c> value (ordered numerically when possible),
        /// creates a period row for each group and adds it to <c>ClassroomByPeriodPanel</c>, and sets <c>ResultCountText</c> to show either the total count or the count for the selected period.
        /// </remarks>
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
        /// <summary>
        /// Creates a UI row for a specific period that displays a period badge, the number of spare classrooms, and a grid of classroom tags.
        /// </summary>
        /// <param name="period">The period identifier used to generate the period display text.</param>
        /// <param name="classrooms">The list of spare classrooms belonging to this period.</param>
        /// <returns>A Border containing the assembled period row UI with header and classroom tag grid.</returns>
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
        /// <summary>
        /// Creates a compact visual tag representing a spare classroom.
        /// </summary>
        /// <param name="classroom">The spare classroom data whose BuildingName and ClassroomName are used for the tag text and tooltip.</param>
        /// <returns>A Border containing a centered TextBlock with the classroom name and a tooltip showing the building and classroom details.</returns>
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
        /// <summary>
        /// Produces a user-friendly label for the given period value.
        /// </summary>
        /// <param name="period">The period identifier as a string; typically a number represented as text.</param>
        /// <returns>
        /// A human-readable period label: if <paramref name="period"/> parses to an integer, returns a formatted period string (building-specific when a building name is set); otherwise returns "Period {period}".
        /// </returns>
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
        /// <summary>
        /// Clears the current classroom results and displays a centered empty-state message indicating no spare classrooms are available.
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
        /// <summary>
        /// Updates campus buttons' visual states so only the specified button appears active.
        /// </summary>
        /// <param name="activeButton">The campus button to mark as active; other campus buttons will be set to inactive.</param>
        private void UpdateCampusButtonStates(Button activeButton)
        {
            ResetButtonStyle(EastCampusButton);
            ResetButtonStyle(WestCampusButton);
            SetActiveButtonStyle(activeButton);
        }

        /// <summary>
        /// Update building button states
        /// <summary>
        /// Update the visual state of all building buttons, resetting every button to the inactive style and applying the active style to the specified button.
        /// </summary>
        /// <param name="activeButton">The button that should be rendered as active. All other building buttons will be reset to the inactive style.</param>
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
        /// <summary>
        /// Apply the default (inactive) visual style to the specified button.
        /// </summary>
        /// <param name="button">The Button to reset to the default inactive styling.</param>
        private static void ResetButtonStyle(Button button)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
            button.Foreground = new SolidColorBrush(Color.FromRgb(45, 90, 61));
        }

        /// <summary>
        /// Set active button style
        /// <summary>
        /// Applies the active visual styling to the provided button.
        /// </summary>
        /// <param name="button">The button to style as active (sets background and foreground colors).</param>
        private static void SetActiveButtonStyle(Button button)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(45, 90, 61));
            button.Foreground = Brushes.White;
        }

        /// <summary>
        /// Show status message
        /// <summary>
        /// Updates the status message displayed in the view's status text area.
        /// </summary>
        /// <param name="message">Text to display in the status area.</param>
        private void ShowStatus(string message)
        {
            StatusText.Text = message;
        }

        /// <summary>
        /// Set loading state
        /// <summary>
        /// Enable or disable campus and building buttons according to the loading state.
        /// </summary>
        /// <param name="isLoading">True to disable campus and building buttons while loading; false to enable them.</param>
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
