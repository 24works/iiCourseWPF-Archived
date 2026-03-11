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

        public UserInfoView()
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
        /// Set username
        /// </summary>
        public void SetUsername(string username)
        {
            _username = username;
        }

        /// <summary>
        /// Load user info
        /// </summary>
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
        /// </summary>
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
        /// </summary>
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
            await LoadUserInfoAsync();
        }
    }
}
