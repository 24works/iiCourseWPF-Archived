using System.Windows.Input;
using iiCourse.Core.Commands;
using iiCourse.Core.Models;

namespace iiCourse.Core.ViewModels
{
    /// <summary>
    /// 一卡通信息ViewModel
    /// </summary>
    public class CardInfoViewModel : ViewModelBase
    {
        private readonly iiCoreService _coreService;

        private string _balance = string.Empty;
        private string _lastConsumeTime = string.Empty;
        private bool _isLoading;
        private string _statusMessage = string.Empty;

        public CardInfoViewModel(iiCoreService coreService)
        {
            _coreService = coreService;
            RefreshCommand = new RelayCommand(async _ => await LoadCardInfoAsync(), _ => !IsLoading);
        }

        #region 属性

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

        public bool HasData => !string.IsNullOrEmpty(Balance);

        #endregion

        #region 命令

        public ICommand RefreshCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 加载一卡通信息
        /// </summary>
        public async Task LoadCardInfoAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                StatusMessage = "正在加载一卡通信息...";

                var cardInfo = await _coreService.GetCardInfoAsync();

                if (cardInfo != null)
                {
                    DisplayCardInfo(cardInfo);
                    StatusMessage = "一卡通信息加载成功";
                }
                else
                {
                    ShowError("获取一卡通信息失败");
                }
            }
            catch (Exception ex)
            {
                ShowError($"加载一卡通信息失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
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
        /// 显示错误
        /// </summary>
        private void ShowError(string message)
        {
            Balance = string.Empty;
            LastConsumeTime = string.Empty;
            StatusMessage = message;
        }

        #endregion
    }
}
