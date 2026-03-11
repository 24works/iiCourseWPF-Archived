using System.Collections.ObjectModel;
using System.Windows.Input;
using iiCourse.Core.Commands;
using Newtonsoft.Json.Linq;

namespace iiCourse.Core.ViewModels
{
    /// <summary>
    /// 成绩项模型
    /// </summary>
    public class ScoreItem
    {
        public string CourseName { get; set; } = string.Empty;
        public string Credit { get; set; } = string.Empty;
        public string Score { get; set; } = string.Empty;
        public string GPA { get; set; } = string.Empty;
        public string Nature { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public double ScoreValue { get; set; }
    }

    /// <summary>
    /// 成绩查询ViewModel
    /// </summary>
    public class ScoreViewModel : ViewModelBase
    {
        private readonly iiCoreService _coreService;

        private ObservableCollection<ScoreItem> _scores = new();
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _hasData;

        public ScoreViewModel(iiCoreService coreService)
        {
            _coreService = coreService;
            RefreshCommand = new RelayCommand(async _ => await LoadScoresAsync(), _ => !IsLoading);
        }

        #region 属性

        public ObservableCollection<ScoreItem> Scores
        {
            get => _scores;
            set => SetProperty(ref _scores, value);
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

        public bool HasData
        {
            get => _hasData;
            set => SetProperty(ref _hasData, value);
        }

        #endregion

        #region 命令

        public ICommand RefreshCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 加载成绩数据
        /// </summary>
        public async Task LoadScoresAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                StatusMessage = "正在加载成绩...";
                HasData = false;

                var scoreData = await _coreService.GetExamScoreAsync();

                if (!string.IsNullOrEmpty(scoreData))
                {
                    ParseAndDisplayScores(scoreData);
                    StatusMessage = $"共加载 {Scores.Count} 门课程成绩";
                    HasData = Scores.Count > 0;
                }
                else
                {
                    Scores.Clear();
                    StatusMessage = "暂无成绩数据";
                }
            }
            catch (Exception ex)
            {
                Scores.Clear();
                StatusMessage = $"加载成绩失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 解析并显示成绩数据
        /// </summary>
        private void ParseAndDisplayScores(string scoreData)
        {
            var scores = new ObservableCollection<ScoreItem>();

            try
            {
                var jsonArray = JArray.Parse(scoreData);

                foreach (var item in jsonArray)
                {
                    var scoreStr = item["SCORE_NUMERIC"]?.ToString() ?? "--";
                    double scoreValue = 0;
                    double.TryParse(scoreStr, out scoreValue);

                    scores.Add(new ScoreItem
                    {
                        CourseName = item["COURSENAME"]?.ToString() ?? "--",
                        Credit = item["CREDIT"]?.ToString() ?? "--",
                        Score = scoreStr,
                        GPA = "--",
                        Nature = item["EXAMPROPERTY"]?.ToString() ?? "--",
                        Semester = $"{item["XN"]?.ToString() ?? "--"}-{item["XQ"]?.ToString() ?? "--"}",
                        ScoreValue = scoreValue
                    });
                }
            }
            catch
            {
                // 解析失败时返回空集合
            }

            Scores = scores;
        }

        #endregion
    }
}
