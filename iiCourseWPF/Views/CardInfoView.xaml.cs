using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iiCourse.Core;
using iiCourse.Core.Models;

namespace iiCourseWPF.Views
{
    /// <summary>
    /// Card info view
    /// </summary>
    public partial class CardInfoView : UserControl
    {
        private iiCoreService? _service;

        /// <summary>
        /// Initializes a new instance of the CardInfoView class and initializes its UI components.
        /// </summary>
        public CardInfoView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set service instance
        /// <summary>
        /// Sets the iiCoreService instance used by this view to load and display card information.
        /// </summary>
        /// <param name="service">The service implementation used to retrieve card data.</param>
        public void SetService(iiCoreService service)
        {
            _service = service;
        }

        /// <summary>
        /// Load card info
        /// <summary>
        /// Loads card information from the configured service and updates the view with status, loading state, and content or error messages.
        /// </summary>
        public async Task LoadCardInfoAsync()
        {
            if (_service == null)
            {
                ShowStatus("Service not initialized");
                return;
            }

            try
            {
                SetLoadingState(true);
                ShowStatus("Loading card info...");

                var cardInfo = await _service.GetCardInfoAsync();

                if (cardInfo != null)
                {
                    DisplayCardInfo(cardInfo);
                    ShowStatus("Card info loaded");
                }
                else
                {
                    ShowError("Failed to get card info");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading card info: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// Display card info
        /// <summary>
        /// Populate the UI fields with the provided card's balance and last consume time.
        /// </summary>
        /// <param name="cardInfo">Card information whose Balance and LastConsumeTime are displayed; if LastConsumeTime is null or empty, displays "No consume record".</param>
        private void DisplayCardInfo(CardInfo cardInfo)
        {
            BalanceText.Text = cardInfo.Balance;

            if (!string.IsNullOrEmpty(cardInfo.LastConsumeTime))
            {
                LastConsumeText.Text = cardInfo.LastConsumeTime;
            }
            else
            {
                LastConsumeText.Text = "No consume record";
            }
        }

        /// <summary>
        /// Show error message
        /// <summary>
        /// Display an error message and reset visible card fields to placeholders.
        /// </summary>
        /// <param name="message">The error text to show in the status area.</param>
        private void ShowError(string message)
        {
            BalanceText.Text = "--";
            LastConsumeText.Text = "--";
            ShowStatus(message);
        }

        /// <summary>
        /// Show status message
        /// <summary>
        /// Updates the status text displayed in the view.
        /// </summary>
        /// <param name="message">The status message to display.</param>
        private void ShowStatus(string message)
        {
            StatusText.Text = message;
        }

        /// <summary>
        /// Set loading state
        /// <summary>
        /// Updates the UI to reflect the current loading state.
        /// </summary>
        /// <param name="isLoading">When true, disables the refresh button and sets its content to "Loading..."; when false, enables the button and sets its content to "Refresh".</param>
        private void SetLoadingState(bool isLoading)
        {
            RefreshButton.IsEnabled = !isLoading;
            RefreshButton.Content = isLoading ? "Loading..." : "Refresh";
        }

        /// <summary>
        /// Refresh button click event
        /// <summary>
        /// Handles the Refresh button click and triggers reloading of card information.
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Event data for the routed event.</param>
        private async void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            await LoadCardInfoAsync();
        }
    }
}
