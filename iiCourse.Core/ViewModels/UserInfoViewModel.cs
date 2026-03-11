using System.Windows.Input;
using iiCourse.Core.Commands;
using iiCourse.Core.Models;

namespace iiCourse.Core.ViewModels
{
    /// <summary>
    /// 用户信息ViewModel（包含一卡通信息）
    /// </summary>
    public class UserInfoViewModel : ViewModelBase
    {
        private readonly iiCoreService _coreService;

        // 用户基本信息
        private string _username = string.Empty;
        private string _name = string.Empty;
        private string _studentId = string.Empty;
        private string _gender = string.Empty;
        private string _college = string.Empty;

        // 一卡通信息
        private string _balance = string.Empty;
        private string _lastConsumeTime = string.Empty;

        // 状态
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _isError;

        public UserInfoViewModel(iiCoreService coreService)
        {
            _coreService = coreService;
            RefreshCommand = new RelayCommand(async _ => await LoadAllInfoAsync(), _ => !IsLoading);
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

        // 一卡通信息属性
        public string Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public string LastConsumeTime
        {
            get => _lastConsumeTime;
            set => SetProperty(ref _lastConsumeTime, value);
        }

        public bool HasCardData => !string.IsNullOrEmpty(Balance);

        #endregion

        #region 命令

        public ICommand RefreshCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 加载所有信息（用户+一卡通）
        /// </summary>
        public async Task LoadAllInfoAsync()
        {
            if (IsLoading || string.IsNullOrEmpty(Username)) return;

            try
            {
                IsLoading = true;
                IsError = false;
                StatusMessage = "正在加载信息...";

                // 同时加载用户信息和一卡通信息
                var userInfoTask = _coreService.GetUserInfoAsync(Username);
                var cardInfoTask = _coreService.GetCardInfoAsync();

                await Task.WhenAll(userInfoTask, cardInfoTask);

                var userInfo = await userInfoTask;
                var cardInfo = await cardInfoTask;

                if (userInfo != null)
                {
                    DisplayUserInfo(userInfo);
                }
                else
                {
                    ShowUserError("获取用户信息失败");
                }

                if (cardInfo != null)
                {
                    DisplayCardInfo(cardInfo);
                }
                else
                {
                    ShowCardError("获取一卡通信息失败");
                }

                if (userInfo != null && cardInfo != null)
                {
                    StatusMessage = "信息加载成功";
                }
            }
            catch (Exception ex)
            {
                ShowUserError($"加载信息失败: {ex.Message}");
                ShowCardError($"加载信息失败: {ex.Message}");
                IsError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 加载用户信息（兼容旧代码）
        /// </summary>
        public async Task LoadUserInfoAsync()
        {
            await LoadAllInfoAsync();
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
        /// 显示一卡通信息
        /// </summary>
        private void DisplayCardInfo(CardInfo cardInfo)
        {
            Balance = cardInfo.Balance;
            LastConsumeTime = string.IsNullOrEmpty(cardInfo.LastConsumeTime)
                ? "暂无消费记录"
                : cardInfo.LastConsumeTime;
        }

        /// <summary>
        /// 显示用户错误
        /// </summary>
        private void ShowUserError(string message)
        {
            Name = string.Empty;
            StudentId = string.Empty;
            Gender = string.Empty;
            College = string.Empty;
            StatusMessage = message;
            IsError = true;
        }

        /// <summary>
        /// 显示一卡通错误
        /// </summary>
        private void ShowCardError(string message)
        {
            Balance = string.Empty;
            LastConsumeTime = string.Empty;
        }

        #endregion
    }
}
