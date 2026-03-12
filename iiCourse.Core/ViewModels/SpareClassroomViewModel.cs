using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using iiCourse.Core.Commands;
using iiCourse.Core.Models;

namespace iiCourse.Core.ViewModels
{
    /// <summary>
    /// 校区选项
    /// </summary>
    public class CampusOption : ViewModelBase
    {
        private string _id = string.Empty;
        private string _name = string.Empty;
        private bool _isSelected;
        private Brush _backgroundBrush = Brushes.Transparent;
        private Brush _foregroundBrush = Brushes.Black;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

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
    /// 教学楼选项
    /// </summary>
    public class BuildingOption : ViewModelBase
    {
        private string _id = string.Empty;
        private string _name = string.Empty;
        private bool _isSelected;
        private Brush _backgroundBrush = Brushes.Transparent;
        private Brush _foregroundBrush = Brushes.Black;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

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
    /// 节次筛选选项
    /// </summary>
    public class PeriodFilterOption : ViewModelBase
    {
        private string? _period; // null 表示"全部"
        private string _displayText = string.Empty;
        private bool _isSelected;
        private Brush _backgroundBrush = Brushes.Transparent;
        private Brush _foregroundBrush = Brushes.Black;

        public string? Period
        {
            get => _period;
            set => SetProperty(ref _period, value);
        }

        public string DisplayText
        {
            get => _displayText;
            set => SetProperty(ref _displayText, value);
        }

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
    /// 教学楼信息
    /// </summary>
    public class BuildingViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }

    /// <summary>
    /// 空教室信息
    /// </summary>
    public class SpareClassroomViewItem
    {
        public string ClassroomName { get; set; } = string.Empty;
        public string BuildingName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
    }

    /// <summary>
    /// 按节次分组的空教室
    /// </summary>
    public class PeriodGroup
    {
        public string Period { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
        public ObservableCollection<SpareClassroomViewItem> Classrooms { get; set; } = new();
        public int Count => Classrooms.Count;
    }

    /// <summary>
    /// 空教室查询ViewModel
    /// </summary>
    public class SpareClassroomViewModel : ViewModelBase
    {
        private readonly iiCoreService _coreService;

        // 颜色常量 - 优化配色方案
        private static readonly SolidColorBrush PrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 107, 53));
        private static readonly SolidColorBrush PrimaryDarkBrush = new SolidColorBrush(Color.FromRgb(230, 90, 40));
        private static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush LightGrayBrush = new SolidColorBrush(Color.FromRgb(245, 245, 245));
        private static readonly SolidColorBrush MediumGrayBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));
        private static readonly SolidColorBrush DarkGrayBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));
        private static readonly SolidColorBrush TextGrayBrush = new SolidColorBrush(Color.FromRgb(127, 140, 141));

        private ObservableCollection<CampusOption> _campusOptions = new();
        private ObservableCollection<BuildingOption> _buildingOptions = new();
        private ObservableCollection<PeriodFilterOption> _periodFilterOptions = new();
        private ObservableCollection<PeriodGroup> _periodGroups = new();
        private string? _selectedCampusId;
        private string? _selectedBuildingId;
        private string? _selectedPeriod;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private string _resultCountText = string.Empty;
        private bool _showPeriodFilter;

        public SpareClassroomViewModel(iiCoreService coreService)
        {
            _coreService = coreService;
            SelectCampusCommand = new RelayCommand<string>(async campusId => await SelectCampusAsync(campusId), _ => !IsLoading);
            SelectBuildingCommand = new RelayCommand<string>(async buildingId => await SelectBuildingAsync(buildingId), _ => !IsLoading);
            SelectPeriodCommand = new RelayCommand<string>(period => SelectPeriod(period), _ => !IsLoading);
            
            // 初始化校区选项
            InitCampusOptions();
        }

        #region 属性

        /// <summary>
        /// 校区选项集合（用于XAML绑定）
        /// </summary>
        public ObservableCollection<CampusOption> CampusOptions
        {
            get => _campusOptions;
            set => SetProperty(ref _campusOptions, value);
        }

        /// <summary>
        /// 教学楼选项集合（用于XAML绑定）
        /// </summary>
        public ObservableCollection<BuildingOption> BuildingOptions
        {
            get => _buildingOptions;
            set => SetProperty(ref _buildingOptions, value);
        }

        /// <summary>
        /// 节次筛选选项集合（用于XAML绑定）
        /// </summary>
        public ObservableCollection<PeriodFilterOption> PeriodFilterOptions
        {
            get => _periodFilterOptions;
            set => SetProperty(ref _periodFilterOptions, value);
        }

        /// <summary>
        /// 筛选后的节次分组（用于XAML绑定）
        /// </summary>
        public ObservableCollection<PeriodGroup> FilteredPeriodGroups
        {
            get
            {
                if (string.IsNullOrEmpty(_selectedPeriod))
                    return _periodGroups;
                return new ObservableCollection<PeriodGroup>(
                    _periodGroups.Where(g => g.Period == _selectedPeriod));
            }
        }

        public ObservableCollection<PeriodGroup> PeriodGroups
        {
            get => _periodGroups;
            set
            {
                if (SetProperty(ref _periodGroups, value))
                {
                    OnPropertyChanged(nameof(FilteredPeriodGroups));
                    OnPropertyChanged(nameof(HasData));
                }
            }
        }

        public string? SelectedCampusId
        {
            get => _selectedCampusId;
            set
            {
                if (SetProperty(ref _selectedCampusId, value))
                {
                    UpdateCampusOptionStyles();
                }
            }
        }

        public string? SelectedBuildingId
        {
            get => _selectedBuildingId;
            set
            {
                if (SetProperty(ref _selectedBuildingId, value))
                {
                    UpdateBuildingOptionStyles();
                }
            }
        }

        public string? SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (SetProperty(ref _selectedPeriod, value))
                {
                    UpdatePeriodFilterOptionStyles();
                    OnPropertyChanged(nameof(FilteredPeriodGroups));
                    UpdateResultCount();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    (SelectCampusCommand as RelayCommand<string>)?.RaiseCanExecuteChanged();
                    (SelectBuildingCommand as RelayCommand<string>)?.RaiseCanExecuteChanged();
                    (SelectPeriodCommand as RelayCommand<string>)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string ResultCountText
        {
            get => _resultCountText;
            set => SetProperty(ref _resultCountText, value);
        }

        public bool HasData => PeriodGroups.Count > 0;

        /// <summary>
        /// 是否显示节次筛选区
        /// </summary>
        public bool ShowPeriodFilter
        {
            get => _showPeriodFilter;
            set => SetProperty(ref _showPeriodFilter, value);
        }

        #endregion

        #region 命令

        public ICommand SelectCampusCommand { get; }
        public ICommand SelectBuildingCommand { get; }
        public ICommand SelectPeriodCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化校区选项
        /// </summary>
        private void InitCampusOptions()
        {
            CampusOptions = new ObservableCollection<CampusOption>
            {
                new CampusOption { Id = "1", Name = "东校区", IsSelected = false },
                new CampusOption { Id = "2", Name = "西校区", IsSelected = false }
            };
            UpdateCampusOptionStyles();
        }

        /// <summary>
        /// 更新校区选项样式 - 优化配色
        /// </summary>
        private void UpdateCampusOptionStyles()
        {
            foreach (var option in CampusOptions)
            {
                option.IsSelected = option.Id == _selectedCampusId;
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
            OnPropertyChanged(nameof(CampusOptions));
        }

        /// <summary>
        /// 更新教学楼选项样式 - 优化配色
        /// </summary>
        private void UpdateBuildingOptionStyles()
        {
            foreach (var option in BuildingOptions)
            {
                option.IsSelected = option.Id == _selectedBuildingId;
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
            OnPropertyChanged(nameof(BuildingOptions));
        }

        /// <summary>
        /// 更新节次筛选选项样式 - 优化配色
        /// </summary>
        private void UpdatePeriodFilterOptionStyles()
        {
            foreach (var option in PeriodFilterOptions)
            {
                option.IsSelected = option.Period == _selectedPeriod;
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
            OnPropertyChanged(nameof(PeriodFilterOptions));
        }

        /// <summary>
        /// 选择校区
        /// </summary>
        private async Task SelectCampusAsync(string? campusId)
        {
            if (string.IsNullOrEmpty(campusId) || IsLoading) return;

            SelectedCampusId = campusId;
            SelectedBuildingId = null;
            SelectedPeriod = null;
            PeriodGroups.Clear();
            BuildingOptions.Clear();
            PeriodFilterOptions.Clear();
            ShowPeriodFilter = false;

            await LoadBuildingsAsync(campusId);
        }

        /// <summary>
        /// 加载教学楼列表
        /// </summary>
        private async Task LoadBuildingsAsync(string campusId)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "正在加载教学楼列表...";

                var buildings = await _coreService.GetBuildingsAsync(campusId);
                var options = new ObservableCollection<BuildingOption>();

                foreach (var b in buildings)
                {
                    options.Add(new BuildingOption
                    {
                        Name = b.Name,
                        Id = b.ID,
                        IsSelected = false,
                        BackgroundBrush = LightGrayBrush,
                        ForegroundBrush = DarkGrayBrush
                    });
                }

                BuildingOptions = options;
                StatusMessage = $"共 {buildings.Count} 栋教学楼";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 选择教学楼
        /// </summary>
        private async Task SelectBuildingAsync(string? buildingId)
        {
            if (string.IsNullOrEmpty(buildingId) || IsLoading) return;

            SelectedBuildingId = buildingId;
            SelectedPeriod = null;

            if (int.TryParse(buildingId, out int id))
            {
                await LoadSpareClassroomsAsync(id);
            }
        }

        /// <summary>
        /// 加载空教室数据
        /// </summary>
        private async Task LoadSpareClassroomsAsync(int buildingId)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "正在查询空教室...";

                var classrooms = await _coreService.GetSpareClassroomAsync(buildingId);

                // 提取所有节次
                var periods = classrooms
                    .Select(c => c.Period)
                    .Distinct()
                    .OrderBy(p => int.TryParse(p, out var n) ? n : 999)
                    .ToList();

                // 创建节次筛选选项
                var filterOptions = new ObservableCollection<PeriodFilterOption>
                {
                    new PeriodFilterOption
                    {
                        Period = null,
                        DisplayText = "全部",
                        IsSelected = true,
                        BackgroundBrush = PrimaryBrush,
                        ForegroundBrush = WhiteBrush
                    }
                };

                foreach (var period in periods)
                {
                    filterOptions.Add(new PeriodFilterOption
                    {
                        Period = period,
                        DisplayText = $"第{period}节",
                        IsSelected = false,
                        BackgroundBrush = LightGrayBrush,
                        ForegroundBrush = DarkGrayBrush
                    });
                }

                PeriodFilterOptions = filterOptions;
                ShowPeriodFilter = periods.Count > 0;

                // 按节次分组
                var groups = new ObservableCollection<PeriodGroup>();
                foreach (var period in periods)
                {
                    var periodClassrooms = classrooms
                        .Where(c => c.Period == period)
                        .Select(c => new SpareClassroomViewItem
                        {
                            ClassroomName = c.ClassroomName,
                            BuildingName = c.BuildingName,
                            Period = c.Period
                        })
                        .ToList();

                    groups.Add(new PeriodGroup
                    {
                        Period = period,
                        DisplayText = $"第{period}节",
                        Classrooms = new ObservableCollection<SpareClassroomViewItem>(periodClassrooms)
                    });
                }

                PeriodGroups = groups;
                OnPropertyChanged(nameof(FilteredPeriodGroups));
                UpdateResultCount();
                var uniqueClassrooms = classrooms.Select(c => c.ClassroomName).Distinct().Count();
                StatusMessage = $"找到 {uniqueClassrooms} 间教室的 {classrooms.Count} 个空闲时段";
            }
            catch (Exception ex)
            {
                PeriodGroups.Clear();
                StatusMessage = $"查询失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 选择节次筛选
        /// </summary>
        private void SelectPeriod(string? period)
        {
            SelectedPeriod = period;
        }

        /// <summary>
        /// 更新结果计数
        /// </summary>
        private void UpdateResultCount()
        {
            if (string.IsNullOrEmpty(SelectedPeriod))
            {
                var total = PeriodGroups.Sum(g => g.Count);
                var classroomCount = PeriodGroups.SelectMany(g => g.Classrooms).Select(c => c.ClassroomName).Distinct().Count();
                ResultCountText = $"{classroomCount} 间教室 · {total} 个空闲时段";
            }
            else
            {
                var group = PeriodGroups.FirstOrDefault(g => g.Period == SelectedPeriod);
                ResultCountText = $"第{SelectedPeriod}节: {group?.Count ?? 0} 间教室";
            }
        }

        #endregion
    }
}
