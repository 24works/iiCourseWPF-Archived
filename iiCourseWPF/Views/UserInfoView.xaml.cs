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
    /// User info view
    /// </summary>
    public partial class UserInfoView : UserControl
    {
        private iiCoreService? _service;
        private string? _username;

        /// <summary>
        /// Initializes a new instance of the UserInfoView control and initializes its UI components.
        /// </summary>
        public UserInfoView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set service instance
        /// <summary>
        /// Assigns the service instance used to retrieve user information for this view.
        /// </summary>
        /// <param name="service">The iiCoreService implementation used to fetch user data.</param>
        public void SetService(iiCoreService service)
        {
            _service = service;
        }

        /// <summary>
        /// Set username
        /// <summary>
        /// Sets the username to be used when loading user information.
        /// </summary>
        /// <param name="username">The username whose information will be loaded.</param>
        public void SetUsername(string username)
        {
            _username = username;
        }

        /// <summary>
        /// Load user info
        /// <summary>
        /// Loads user information for the configured username and updates the view's UI with the result.
        /// Shows an error message if the service or username is not set or if loading fails.
        /// </summary>
        /// <returns>A task that completes when the load operation and UI update have finished.</returns>
        public async Task LoadUserInfoAsync()
        {
            if (_service == null || string.IsNullOrEmpty(_username))
            {
                ShowError("Service not initialized or username not set");
                return;
            }

            try
            {
                SetLoadingState(true);

                var userInfo = await _service.GetUserInfoAsync(_username);

                if (userInfo != null)
                {
                    DisplayUserInfo(userInfo);
                }
                else
                {
                    ShowError("Failed to get user info");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading user info: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// Display user info
        /// <summary>
        /// Populates the view's UI fields with the provided user's profile data.
        /// </summary>
        /// <param name="userInfo">The user's profile data used to populate name, college, student ID, gender, and login status fields. Must not be null.</param>
        private void DisplayUserInfo(UserInfo userInfo)
        {
            NameText.Text = userInfo.Name;
            CollegeText.Text = userInfo.College;
            StudentIdText.Text = userInfo.StudentId;
            NameDetailText.Text = userInfo.Name;
            GenderText.Text = userInfo.Gender;
            CollegeDetailText.Text = userInfo.College;
            LoginStatusText.Text = "Logged In";
            LoginStatusText.Foreground = new SolidColorBrush(Color.FromRgb(78, 205, 196));
        }

        /// <summary>
        /// Show error message
        /// <summary>
        /// Resets the user display fields to placeholder values and marks the login status as failed.
        /// </summary>
        /// <remarks>
        /// Sets all user-related text fields to "--", updates LoginStatusText to "Load Failed", and changes its foreground color to red.
        /// </remarks>
        private void ShowError(string _)
        {
            NameText.Text = "--";
            CollegeText.Text = "--";
            StudentIdText.Text = "--";
            NameDetailText.Text = "--";
            GenderText.Text = "--";
            CollegeDetailText.Text = "--";
            LoginStatusText.Text = "Load Failed";
            LoginStatusText.Foreground = System.Windows.Media.Brushes.Red;
        }

        /// <summary>
        /// Set loading state
        /// <summary>
        /// Updates the refresh button's enabled state and label to reflect whether a refresh operation is in progress.
        /// </summary>
        /// <param name="isLoading">`true` if a refresh is in progress; `false` otherwise.</param>
        private void SetLoadingState(bool isLoading)
        {
            RefreshButton.IsEnabled = !isLoading;
            RefreshButton.Content = isLoading ? "Loading..." : "Refresh";
        }

        /// <summary>
        /// Refresh button click event
        /// <summary>
        /// Handles the Refresh button click and triggers reloading of the displayed user information.
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">The event arguments associated with the click.</param>
        private async void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            await LoadUserInfoAsync();
        }
    }
}
