using System.Windows.Input;
using iiCourse.Core.Commands;
using iiCourse.Core.Models;

namespace iiCourse.Core.ViewModels
{
    /// <summary>
    /// 用户信息ViewModel
    /// </summary>
    public class UserInfoViewModel : ViewModelBase
    {
        private readonly iiCoreService _coreService;

        private string _username = string.Empty;
        private string _name = string.Empty;
        private string _studentId = string.Empty;
        private string _gender = string.Empty;
        private string _college = string.Empty;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _isError;

        public UserInfoViewModel(iiCoreService coreService)
        {
            _coreService = coreService;
            RefreshCommand = new RelayCommand(async _ => await LoadUserInfoAsync(), _ => !IsLoading);
        }

        #region 属性

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    // 通知 IsLoggedIn 属性已变更
                    OnPropertyChanged(nameof(IsLoggedIn));
                }
            }
        }

        public string StudentId
        {
            get => _studentId;
            set => SetProperty(ref _studentId, value);
        }

        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        public string College
        {
            get => _college;
            set => SetProperty(ref _college, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        public bool IsLoggedIn => !string.IsNullOrEmpty(Name);

        #endregion

        #region 命令

        public ICommand RefreshCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 加载用户信息
        /// </summary>
        public async Task LoadUserInfoAsync()
        {
            if (IsLoading || string.IsNullOrEmpty(Username)) return;

            try
            {
                IsLoading = true;
                IsError = false;
                StatusMessage = "正在加载用户信息...";

                var userInfo = await _coreService.GetUserInfoAsync(Username);

                if (userInfo != null)
                {
                    DisplayUserInfo(userInfo);
                    StatusMessage = "用户信息加载成功";
                }
                else
                {
                    ShowError("获取用户信息失败");
                }
            }
            catch (Exception ex)
            {
                ShowError($"加载用户信息失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 显示用户信息
        /// </summary>
        private void DisplayUserInfo(UserInfo userInfo)
        {
            Name = userInfo.Name;
            StudentId = userInfo.StudentId;
            Gender = userInfo.Gender;
            College = userInfo.College;
        }

        /// <summary>
        /// 显示错误
        /// </summary>
        private void ShowError(string message)
        {
            Name = string.Empty;
            StudentId = string.Empty;
            Gender = string.Empty;
            College = string.Empty;
            StatusMessage = message;
            IsError = true;
        }

        #endregion
    }
}
