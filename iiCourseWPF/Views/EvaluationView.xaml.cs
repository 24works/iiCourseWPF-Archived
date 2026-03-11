using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using iiCourse.Core;
using iiCourse.Core.Models;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// Student evaluation view
    /// </summary>
    public partial class EvaluationView : UserControl
    {
        private iiCoreService? _service;

        /// <summary>
        /// Initializes a new instance of the EvaluationView and sets up its user interface components.
        /// </summary>
        public EvaluationView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set service instance
        /// <summary>
        /// Stores the iiCoreService instance that the view will use to perform review-related operations.
        /// </summary>
        /// <param name="service">The service implementation used to load and update student reviews.</param>
        public void SetService(iiCoreService service)
        {
            _service = service;
        }

        /// <summary>
        /// Load review list
        /// <summary>
        /// Loads student reviews from the configured service and updates the view UI with the results.
        /// </summary>
        /// <remarks>
        /// If the service has not been set, the status is updated and the operation is aborted. While loading, the view's loading state and status text are updated; on success the reviews are displayed, and on failure an empty state and an error/status message are shown. Exceptions are caught and reported via the status text.
        /// </remarks>
        public async Task LoadReviewsAsync()
        {
            if (_service == null)
            {
                ShowStatus("Service not initialized");
                return;
            }

            try
            {
                SetLoadingState(true);
                ShowStatus("Loading review list...");

                var reviews = await _service.GetStudentReviewsAsync();

                if (reviews.Code == 200 && reviews.Data != null)
                {
                    DisplayReviews(reviews.Data);
                    ShowStatus($"Loaded {reviews.Data.Count} reviews");
                }
                else
                {
                    ShowEmptyState();
                    ShowStatus(reviews.Message ?? "Failed to get review list");
                }
            }
            catch (Exception ex)
            {
                ShowEmptyState();
                ShowStatus($"Error: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// Display review list
        /// <summary>
        /// Render the provided student reviews into the view by populating the ReviewPanel with review cards.
        /// </summary>
        /// <param name="reviews">The collection of StudentReview objects to display; the panel is cleared before adding these items.</param>
        private void DisplayReviews(List<StudentReview> reviews)
        {
            ReviewPanel.Children.Clear();

            foreach (var review in reviews)
            {
                var reviewCard = CreateReviewCard(review);
                ReviewPanel.Children.Add(reviewCard);
            }
        }

        /// <summary>
        /// Create review card
        /// <summary>
        /// Create a styled UI card that represents a single student review.
        /// </summary>
        /// <param name="review">The StudentReview whose properties (year/semester, category, batch, course type, start and end time) are displayed on the card.</param>
        /// <returns>A Border containing the composed review card UI for display in the review list.</returns>
        private Border CreateReviewCard(StudentReview review)
        {
            var border = new Border
            {
                Style = FindResource("RowStyle") as Style,
                Background = new SolidColorBrush(Color.FromRgb(255, 253, 245)),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var statusBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 107, 53)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var statusText = new TextBlock
            {
                Text = "Pending",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            statusBorder.Child = statusText;
            Grid.SetColumn(statusBorder, 0);

            var infoStack = new StackPanel { Margin = new Thickness(0, 0, 16, 0) };

            var titleText = new TextBlock
            {
                Text = $"{review.YearSemester} - {review.Category}",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                Margin = new Thickness(0, 0, 0, 6)
            };

            var batchText = new TextBlock
            {
                Text = $"Batch: {review.Batch} | Type: {review.CourseType}",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                Margin = new Thickness(0, 0, 0, 4)
            };

            var timeText = new TextBlock
            {
                Text = $"Time: {review.StartTime} ~ {review.EndTime}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(149, 165, 166))
            };

            infoStack.Children.Add(titleText);
            infoStack.Children.Add(batchText);
            infoStack.Children.Add(timeText);
            Grid.SetColumn(infoStack, 1);

            var arrow = new Path
            {
                Width = 20,
                Height = 20,
                Data = FindResource("ChevronRightIcon") as Geometry,
                Fill = new SolidColorBrush(Color.FromRgb(189, 195, 199)),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(arrow, 2);

            grid.Children.Add(statusBorder);
            grid.Children.Add(infoStack);
            grid.Children.Add(arrow);

            border.Child = grid;
            return border;
        }

        /// <summary>
        /// Show empty state
        /// <summary>
        /// Displays a placeholder UI when there are no student reviews to show.
        /// </summary>
        private void ShowEmptyState()
        {
            ReviewPanel.Children.Clear();

            var emptyBorder = new Border
            {
                Style = FindResource("RowStyle") as Style,
                Background = new SolidColorBrush(Color.FromRgb(249, 249, 249))
            };

            var stackPanel = new StackPanel();

            var icon = new TextBlock
            {
                Text = "📋",
                FontSize = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var text = new TextBlock
            {
                Text = "No reviews",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)),
                TextAlignment = TextAlignment.Center
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(text);

            emptyBorder.Child = stackPanel;
            ReviewPanel.Children.Add(emptyBorder);
        }

        /// <summary>
        /// Load review details
        /// <summary>
        /// Handles the click event to initiate loading of student reviews.
        /// </summary>
        private async void OnLoadReviewsClick(object sender, RoutedEventArgs e)
        {
            await LoadReviewsAsync();
        }

        /// <summary>
        /// One-click complete reviews
        /// <summary>
        /// Handles the click event to finish all pending student reviews: prompts for confirmation, invokes the service to complete reviews, displays the outcome, updates status/loading UI, and refreshes the review list on success.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">Event data for the routed event.</param>
        private async void OnFinishAllClick(object sender, RoutedEventArgs e)
        {
            if (_service == null)
            {
                ShowStatus("Service not initialized");
                return;
            }

            var result = MessageBox.Show(
                "This will auto-complete all pending reviews. Continue?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                SetLoadingState(true);
                ShowStatus("Processing reviews...");

                var finishResult = await _service.FinishStudentReviewsAsync();

                if (finishResult.Code == 200 && finishResult.Data != null)
                {
                    var message = finishResult.Data.Count > 0
                        ? $"Completed {finishResult.Data.Count} reviews: {string.Join(", ", finishResult.Data)}"
                        : "No reviews to complete";

                    MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ShowStatus("Reviews completed");

                    await LoadReviewsAsync();
                }
                else
                {
                    MessageBox.Show(finishResult.Message ?? "Operation failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ShowStatus(finishResult.Message ?? "Operation failed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowStatus($"Error: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// Show status message
        /// <summary>
        /// Update the status text displayed in the view.
        /// </summary>
        /// <param name="message">The status message to display.</param>
        private void ShowStatus(string message)
        {
            StatusText.Text = message;
        }

        /// <summary>
        /// Set loading state
        /// <summary>
        /// Toggles the view's loading UI: enables/disables action buttons and updates the load button label.
        /// </summary>
        /// <param name="isLoading">`true` to set the view into loading state (disables actions and shows "Loading..."); `false` to restore normal state (enables actions and shows "Load Reviews").</param>
        private void SetLoadingState(bool isLoading)
        {
            LoadReviewsButton.IsEnabled = !isLoading;
            FinishAllButton.IsEnabled = !isLoading;

            LoadReviewsButton.Content = isLoading ? "Loading..." : "Load Reviews";
        }
    }
}
