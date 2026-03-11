using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using iiCourse.Core.Commands;
using iiCourse.Core.Models;

namespace iiCourse.Core.ViewModels
{
    /// <summary>
    /// 课程项模型（用于课程表显示）
    /// </summary>
    public class ScheduleClassItem
    {
        public string CourseName { get; set; } = string.Empty;
        public string Classroom { get; set; } = string.Empty;
        public string Teacher { get; set; } = string.Empty;
        public int Weekday { get; set; }
        public int StartPeriod { get; set; }
        public int Duration { get; set; }
        public int EndPeriod => StartPeriod + Duration - 1;

        // 颜色属性（由ViewModel设置）
        public Brush BackgroundBrush { get; set; } = Brushes.White;
        public Brush BorderBrush { get; set; } = Brushes.Transparent;
        public Brush ForegroundBrush { get; set; } = Brushes.Black;
    }

    /// <summary>
    /// 学年选项
    /// </summary>
    public class SchoolYearOption
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// 周次选项
    /// </summary>
    public class WeekOption : ViewModelBase
    {
        private int _week;
        private bool _isSelected;
        private Brush _backgroundBrush = new SolidColorBrush(Color.FromRgb(245, 245, 245));
        private Brush _foregroundBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));

        public int Week
        {
            get => _week;
            set => SetProperty(ref _week, value);
        }

        public string DisplayText => $"第{Week}周";

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public Brush BackgroundBrush
        {
            get => _backgroundBrush;
            set => SetProperty(ref _backgroundBrush, value);
        }

        public Brush ForegroundBrush
        {
            get => _foregroundBrush;
            set => SetProperty(ref _foregroundBrush, value);
        }
    }

    /// <summary>
    /// 课程表ViewModel
    /// </summary>
    public class ClassScheduleViewModel : ViewModelBase
    {
        private readonly iiCoreService _coreService;

        private ObservableCollection<ScheduleClassItem> _classes = new();
        private ObservableCollection<SchoolYearOption> _schoolYears = new();
        private ObservableCollection<WeekOption> _weekOptions = new();
        private string _selectedSchoolYear = string.Empty;
        private string _selectedSemester = string.Empty;
        private int _selectedWeek = 1;
        private bool _isCustomQueryMode;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private string _dateRangeText = string.Empty;
        private WeekDateInfo? _currentWeekDates;

        // 颜色调色板
        private static readonly Color[] ColorPalette = new[]
        {
            Color.FromRgb(255, 235, 210), // 浅橙
            Color.FromRgb(255, 240, 220), // 浅黄
            Color.FromRgb(255, 230, 225), // 浅粉
            Color.FromRgb(235, 245, 255), // 浅蓝
            Color.FromRgb(230, 255, 235), // 浅绿
            Color.FromRgb(245, 235, 255), // 浅紫
            Color.FromRgb(255, 245, 230), // 浅杏
            Color.FromRgb(240, 255, 250), // 浅青
        };

        public ClassScheduleViewModel(iiCoreService coreService)
        {
            _coreService = coreService;
            RefreshCommand = new RelayCommand(async _ => await RefreshScheduleAsync(), _ => !IsLoading);
            QueryCommand = new RelayCommand(async _ => await QueryCustomScheduleAsync(), _ => !IsLoading);
            SelectWeekCommand = new RelayCommand<int>(async week => await SelectWeekAsync(week), _ => !IsLoading);

            // 初始化周次选项（1-20周）
            for (int i = 1; i <= 20; i++)
            {
                _weekOptions.Add(new WeekOption { Week = i, IsSelected = i == 1 });
            }
        }

        #region 属性

        public ObservableCollection<ScheduleClassItem> Classes
        {
            get => _classes;
            set => SetProperty(ref _classes, value);
        }

        public ObservableCollection<SchoolYearOption> SchoolYears
        {
            get => _schoolYears;
            set => SetProperty(ref _schoolYears, value);
        }

        public ObservableCollection<WeekOption> WeekOptions
        {
            get => _weekOptions;
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

        public int SelectedWeek
        {
            get => _selectedWeek;
            set => SetProperty(ref _selectedWeek, value);
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
                    (SelectWeekCommand as RelayCommand<int>)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string DateRangeText
        {
            get => _dateRangeText;
            set => SetProperty(ref _dateRangeText, value);
        }

        public WeekDateInfo? CurrentWeekDates
        {
            get => _currentWeekDates;
            set => SetProperty(ref _currentWeekDates, value);
        }

        public bool CanQuery => !IsLoading &&
                                !string.IsNullOrEmpty(SelectedSchoolYear) &&
                                !string.IsNullOrEmpty(SelectedSemester);

        #endregion

        #region 命令

        public ICommand RefreshCommand { get; }
        public ICommand QueryCommand { get; }
        public ICommand SelectWeekCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 根据课程名获取一致的颜色
        /// </summary>
        private Brush GetClassColor(string courseName)
        {
            int hash = 0;
            foreach (char c in courseName)
            {
                hash = ((hash << 5) - hash) + c;
                hash = hash & hash;
            }

            var color = ColorPalette[Math.Abs(hash) % ColorPalette.Length];
            return new SolidColorBrush(color);
        }

        /// <summary>
        /// 创建课程项并设置颜色
        /// </summary>
        private ScheduleClassItem CreateClassItem(string courseName, string classroom, string teacher, int weekday, int startPeriod, int duration)
        {
            return new ScheduleClassItem
            {
                CourseName = courseName,
                Classroom = classroom,
                Teacher = teacher,
                Weekday = weekday,
                StartPeriod = startPeriod,
                Duration = duration,
                BackgroundBrush = GetClassColor(courseName),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 200, 150)),
                ForegroundBrush = new SolidColorBrush(Color.FromRgb(150, 80, 40))
            };
        }

        // 颜色常量
        private static readonly SolidColorBrush PrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 107, 53));
        private static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush LightGrayBrush = new SolidColorBrush(Color.FromRgb(245, 245, 245));
        private static readonly SolidColorBrush DarkGrayBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));

        /// <summary>
        /// 更新周次选中状态
        /// </summary>
        private void UpdateWeekSelection(int selectedWeek)
        {
            foreach (var option in WeekOptions)
            {
                option.IsSelected = option.Week == selectedWeek;
                if (option.IsSelected)
                {
                    option.BackgroundBrush = PrimaryBrush;
                    option.ForegroundBrush = WhiteBrush;
                }
                else
                {
                    option.BackgroundBrush = LightGrayBrush;
                    option.ForegroundBrush = DarkGrayBrush;
                }
            }
        }

        /// <summary>
        /// 加载学年列表
        /// </summary>
        public async Task LoadSchoolYearsAsync()
        {
            try
            {
                var years = await _coreService.GetSchoolYearsAsync();
                var options = new ObservableCollection<SchoolYearOption>
                {
                    new() { DisplayName = "请选择", Value = "" }
                };

                foreach (var year in years)
                {
                    options.Add(new SchoolYearOption
                    {
                        DisplayName = year.SCHOOL_YEAR,
                        Value = year.SCHOOL_YEAR
                    });
                }

                SchoolYears = options;
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载学年列表失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 加载课程表（默认当前周）
        /// </summary>
        public async Task LoadClassScheduleAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                IsCustomQueryMode = false;
                StatusMessage = "正在加载课程表...";
                DateRangeText = string.Empty;

                var classes = await _coreService.GetClassInfoAsync();

                var items = new ObservableCollection<ScheduleClassItem>();
                foreach (var c in classes)
                {
                    if (int.TryParse(c.SKXQ, out int weekday) &&
                        int.TryParse(c.SKJC, out int startPeriod))
                    {
                        int duration = 1;
                        if (int.TryParse(c.CXJC, out int parsedDuration))
                        {
                            duration = parsedDuration;
                        }

                        if (weekday >= 1 && weekday <= 7 &&
                            startPeriod >= 1 && startPeriod <= 11)
                        {
                            items.Add(CreateClassItem(c.KCMC, c.JXDD, c.JSXM, weekday, startPeriod, duration));
                        }
                    }
                }

                Classes = items;
                StatusMessage = $"共加载 {items.Count} 门课程";
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("假期"))
            {
                Classes.Clear();
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Classes.Clear();
                StatusMessage = $"加载课程表失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 刷新课程表
        /// </summary>
        private async Task RefreshScheduleAsync()
        {
            if (IsCustomQueryMode)
            {
                await QueryCustomScheduleAsync();
            }
            else
            {
                await LoadClassScheduleAsync();
            }
        }

        /// <summary>
        /// 自定义查询课程表
        /// </summary>
        private async Task QueryCustomScheduleAsync()
        {
            if (IsLoading || !CanQuery) return;

            try
            {
                IsLoading = true;
                IsCustomQueryMode = true;
                StatusMessage = $"正在查询第{SelectedWeek}周课程表...";

                var parameters = new CustomQueryParams
                {
                    SchoolYear = SelectedSchoolYear,
                    Semester = SelectedSemester,
                    LearnWeek = SelectedWeek.ToString()
                };

                var result = await _coreService.QueryCustomScheduleAsync(parameters);

                var items = new ObservableCollection<ScheduleClassItem>();
                foreach (var c in result.classes)
                {
                    int endPeriod = c.SKJC + c.CXJC - 1;

                    if (c.SKXQ >= 1 && c.SKXQ <= 7 &&
                        c.SKJC >= 1 && c.SKJC <= 11 &&
                        endPeriod <= 11)
                    {
                        items.Add(CreateClassItem(c.KCMC, c.JXDD, c.JSXM, c.SKXQ, c.SKJC, c.CXJC));
                    }
                }

                Classes = items;
                CurrentWeekDates = result.dates;
                StatusMessage = result.message;

                if (result.dates != null)
                {
                    DateRangeText = $"({result.dates.Date1} - {result.dates.Date7})";
                }
            }
            catch (Exception ex)
            {
                Classes.Clear();
                StatusMessage = $"查询失败: {ex.Message}";
                DateRangeText = string.Empty;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 选择周次
        /// </summary>
        private async Task SelectWeekAsync(int week)
        {
            SelectedWeek = week;
            UpdateWeekSelection(week);

            // 如果已经选择了学年和学期，自动查询
            if (!string.IsNullOrEmpty(SelectedSchoolYear) && !string.IsNullOrEmpty(SelectedSemester))
            {
                await QueryCustomScheduleAsync();
            }
        }

        #endregion
    }
}
