using System.Collections.ObjectModel;
using System.Windows.Input;
using iiCourse.Core.Commands;
using iiCourse.Core.Models;

namespace iiCourse.Core.ViewModels
{
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

        private ObservableCollection<BuildingViewModel> _buildings = new();
        private ObservableCollection<PeriodGroup> _periodGroups = new();
        private ObservableCollection<string> _availablePeriods = new();
        private string? _selectedCampusId;
        private string? _selectedBuildingId;
        private string? _selectedPeriod;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private string _resultCountText = string.Empty;

        public SpareClassroomViewModel(iiCoreService coreService)
        {
            _coreService = coreService;
            SelectCampusCommand = new RelayCommand<string>(async campusId => await SelectCampusAsync(campusId), _ => !IsLoading);
            SelectBuildingCommand = new RelayCommand<string>(async buildingId => await SelectBuildingAsync(buildingId), _ => !IsLoading);
            SelectPeriodCommand = new RelayCommand<string>(period => SelectPeriod(period), _ => !IsLoading);
        }

        #region 属性

        public ObservableCollection<BuildingViewModel> Buildings
        {
            get => _buildings;
            set => SetProperty(ref _buildings, value);
        }

        public ObservableCollection<PeriodGroup> PeriodGroups
        {
            get => _periodGroups;
            set => SetProperty(ref _periodGroups, value);
        }

        public ObservableCollection<string> AvailablePeriods
        {
            get => _availablePeriods;
            set => SetProperty(ref _availablePeriods, value);
        }

        public string? SelectedCampusId
        {
            get => _selectedCampusId;
            set => SetProperty(ref _selectedCampusId, value);
        }

        public string? SelectedBuildingId
        {
            get => _selectedBuildingId;
            set => SetProperty(ref _selectedBuildingId, value);
        }

        public string? SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (SetProperty(ref _selectedPeriod, value))
                {
                    FilterByPeriod();
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

        #endregion

        #region 命令

        public ICommand SelectCampusCommand { get; }
        public ICommand SelectBuildingCommand { get; }
        public ICommand SelectPeriodCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 选择校区
        /// </summary>
        private async Task SelectCampusAsync(string? campusId)
        {
            if (string.IsNullOrEmpty(campusId) || IsLoading) return;

            SelectedCampusId = campusId;
            SelectedBuildingId = null;
            PeriodGroups.Clear();
            AvailablePeriods.Clear();

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
                var viewModels = new ObservableCollection<BuildingViewModel>();

                foreach (var b in buildings)
                {
                    viewModels.Add(new BuildingViewModel
                    {
                        Name = b.Name,
                        Id = b.ID
                    });
                }

                Buildings = viewModels;
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

                AvailablePeriods = new ObservableCollection<string>(periods);

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
                UpdateResultCount();
                StatusMessage = $"共 {classrooms.Count} 个空闲时段";
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
        /// 按节次筛选
        /// </summary>
        private void FilterByPeriod()
        {
            // 筛选逻辑在UI层处理，这里只是触发属性变更
            UpdateResultCount();
        }

        /// <summary>
        /// 更新结果计数
        /// </summary>
        private void UpdateResultCount()
        {
            if (string.IsNullOrEmpty(SelectedPeriod))
            {
                var total = PeriodGroups.Sum(g => g.Count);
                ResultCountText = $"共 {total} 个空闲时段";
            }
            else
            {
                var group = PeriodGroups.FirstOrDefault(g => g.Period == SelectedPeriod);
                ResultCountText = $"第{SelectedPeriod}节: {group?.Count ?? 0} 个教室";
            }
        }

        #endregion
    }
}
