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
        private ObservableCollection<SchoolYearOption> _schoolYears = new();
        private string _selectedSchoolYear = string.Empty;
        private string _selectedSemester = string.Empty;
        private bool _isCustomQueryMode;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _hasData;

        public ScoreViewModel(iiCoreService coreService)
        {
            _coreService = coreService;
            RefreshCommand = new RelayCommand(async _ => await RefreshScoresAsync(), _ => !IsLoading);
            QueryCommand = new RelayCommand(async _ => await QueryCustomScoresAsync(), _ => CanQuery);
        }

        #region 属性

        public ObservableCollection<ScoreItem> Scores
        {
            get => _scores;
            set => SetProperty(ref _scores, value);
        }

        public ObservableCollection<SchoolYearOption> SchoolYears
        {
            get => _schoolYears;
            set => SetProperty(ref _schoolYears, value);
        }

        public string SelectedSchoolYear
        {
            get => _selectedSchoolYear;
            set
            {
                if (SetProperty(ref _selectedSchoolYear, value))
                {
                    (QueryCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string SelectedSemester
        {
            get => _selectedSemester;
            set
            {
                if (SetProperty(ref _selectedSemester, value))
                {
                    (QueryCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCustomQueryMode
        {
            get => _isCustomQueryMode;
            set => SetProperty(ref _isCustomQueryMode, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (QueryCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

        public bool CanQuery => !IsLoading &&
                                !string.IsNullOrEmpty(SelectedSchoolYear) &&
                                !string.IsNullOrEmpty(SelectedSemester);

        #endregion

        #region 命令

        public ICommand RefreshCommand { get; }
        public ICommand QueryCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 加载学年列表（从服务层缓存或API获取）
        /// </summary>
        public async Task LoadSchoolYearsAsync()
        {
            try
            {
                // 先尝试从缓存获取
                var cachedYears = _coreService.GetCachedSchoolYears();
                if (cachedYears.Count > 0)
                {
                    // 有缓存，直接使用
                    var options = new ObservableCollection<SchoolYearOption>
                    {
                        new() { DisplayName = "请选择", Value = "" }
                    };
                    foreach (var year in cachedYears)
                    {
                        options.Add(new SchoolYearOption
                        {
                            DisplayName = year.SCHOOL_YEAR,
                            Value = year.SCHOOL_YEAR
                        });
                    }
                    SchoolYears = options;
                    return;
                }

                // 没有缓存，从API加载
                var years = await _coreService.GetSchoolYearsAsync();
                var newOptions = new ObservableCollection<SchoolYearOption>
                {
                    new() { DisplayName = "请选择", Value = "" }
                };
                foreach (var year in years)
                {
                    newOptions.Add(new SchoolYearOption
                    {
                        DisplayName = year.SCHOOL_YEAR,
                        Value = year.SCHOOL_YEAR
                    });
                }
                SchoolYears = newOptions;
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载学年列表失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 刷新成绩（根据当前模式决定是加载默认还是自定义查询）
        /// </summary>
        private async Task RefreshScoresAsync()
        {
            if (IsCustomQueryMode)
            {
                await QueryCustomScoresAsync();
            }
            else
            {
                await LoadScoresAsync();
            }
        }

        /// <summary>
        /// 加载成绩数据（默认当前学期）
        /// </summary>
        public async Task LoadScoresAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                IsCustomQueryMode = false;
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
        /// 自定义查询成绩
        /// </summary>
        private async Task QueryCustomScoresAsync()
        {
            if (IsLoading || !CanQuery) return;

            try
            {
                IsLoading = true;
                IsCustomQueryMode = true;
                StatusMessage = $"正在查询 {SelectedSchoolYear} 学年第 {SelectedSemester} 学期成绩...";
                HasData = false;

                var scoreData = await _coreService.GetExamScoreByParamsAsync(SelectedSchoolYear, SelectedSemester);

                if (!string.IsNullOrEmpty(scoreData))
                {
                    ParseAndDisplayScores(scoreData);
                    StatusMessage = $"{SelectedSchoolYear} 学年第 {SelectedSemester} 学期：共 {Scores.Count} 门课程成绩";
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
                StatusMessage = $"查询成绩失败: {ex.Message}";
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
