using System.Collections.ObjectModel;
using System.Windows.Input;
using iiCourse.Core.Commands;
using iiCourse.Core.Models;

namespace iiCourse.Core.ViewModels
{
    /// <summary>
    /// 评教项视图模型
    /// </summary>
    public class ReviewItemViewModel
    {
        public string YearSemester { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Batch { get; set; } = string.Empty;
        public string CourseType { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
    }

    /// <summary>
    /// 评教ViewModel
    /// </summary>
    public class EvaluationViewModel : ViewModelBase
    {
        private readonly iiCoreService _coreService;

        private ObservableCollection<ReviewItemViewModel> _reviews = new();
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _hasData;

        public EvaluationViewModel(iiCoreService coreService)
        {
            _coreService = coreService;
            LoadReviewsCommand = new RelayCommand(async _ => await LoadReviewsAsync(), _ => !IsLoading);
            FinishAllCommand = new RelayCommand(async _ => await FinishAllReviewsAsync(), _ => !IsLoading);
        }

        #region 属性

        public ObservableCollection<ReviewItemViewModel> Reviews
        {
            get => _reviews;
            set => SetProperty(ref _reviews, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    (LoadReviewsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (FinishAllCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool HasData
        {
            get => _hasData;
            set => SetProperty(ref _hasData, value);
        }

        #endregion

        #region 命令

        public ICommand LoadReviewsCommand { get; }
        public ICommand FinishAllCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 加载评教列表
        /// </summary>
        public async Task LoadReviewsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                StatusMessage = "正在加载评教列表...";

                var result = await _coreService.GetStudentReviewsAsync();

                if (result.Code == 200 && result.Data != null)
                {
                    var reviews = new ObservableCollection<ReviewItemViewModel>();
                    foreach (var r in result.Data)
                    {
                        reviews.Add(new ReviewItemViewModel
                        {
                            YearSemester = r.YearSemester,
                            Category = r.Category,
                            Batch = r.Batch,
                            CourseType = r.CourseType,
                            StartTime = r.StartTime,
                            EndTime = r.EndTime,
                            Status = "Pending"
                        });
                    }

                    Reviews = reviews;
                    HasData = reviews.Count > 0;
                    StatusMessage = $"共 {reviews.Count} 个评教任务";
                }
                else
                {
                    Reviews.Clear();
                    HasData = false;
                    StatusMessage = result.Message ?? "获取评教列表失败";
                }
            }
            catch (Exception ex)
            {
                Reviews.Clear();
                HasData = false;
                StatusMessage = $"加载失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 一键完成评教
        /// </summary>
        private async Task FinishAllReviewsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                StatusMessage = "正在处理评教...";

                var result = await _coreService.FinishStudentReviewsAsync();

                if (result.Code == 200 && result.Data != null)
                {
                    var message = result.Data.Count > 0
                        ? $"已完成 {result.Data.Count} 个评教"
                        : "没有需要完成的评教";

                    StatusMessage = message;

                    // 重新加载列表
                    await LoadReviewsAsync();
                }
                else
                {
                    StatusMessage = result.Message ?? "操作失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"操作失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}
