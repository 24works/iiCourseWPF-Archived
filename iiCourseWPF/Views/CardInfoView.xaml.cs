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

        public CardInfoView()
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
        /// Load card info
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
        /// </summary>
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
        /// </summary>
        private void ShowError(string message)
        {
            BalanceText.Text = "--";
            LastConsumeText.Text = "--";
            ShowStatus(message);
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
            RefreshButton.IsEnabled = !isLoading;
            RefreshButton.Content = isLoading ? "Loading..." : "Refresh";
        }

        /// <summary>
        /// Refresh button click event
        /// </summary>
        private async void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            await LoadCardInfoAsync();
        }
    }
}
