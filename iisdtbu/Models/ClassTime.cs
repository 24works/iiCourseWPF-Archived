namespace iisdtbu.Models
{
    /// <summary>
    /// 上课时间表配置
    /// 支持不同教学楼的不同时间安排
    /// </summary>
    public static class ClassTime
    {
        /// <summary>
        /// 教学楼类型
        /// </summary>
        public enum BuildingType
        {
            /// <summary>
            /// A类教学楼：课间休息20分钟
            /// 东校：第二教学楼、第四教学楼、综合楼、行政楼、商学实验中心、室内外体育课
            /// 西校：第三教学楼、工学实验中心、室内外体育课
            /// </summary>
            TypeA,

            /// <summary>
            /// B类教学楼：课间休息30分钟
            /// 东校：第三教学楼、第五教学楼
            /// 西校：第一教学楼、第二教学楼
            /// </summary>
            TypeB
        }

        /// <summary>
        /// 节次类型（用于两节课一组）
        /// </summary>
        public enum PeriodType
        {
            Period1_2,
            Break,
            Period3_4,
            Period5_6,
            Period7_8,
            Period9_10
        }

        /// <summary>
        /// 单节课节次
        /// </summary>
        public enum SinglePeriod
        {
            Period1 = 1,
            Period2 = 2,
            Period3 = 3,
            Period4 = 4,
            Period5 = 5,
            Period6 = 6,
            Period7 = 7,
            Period8 = 8,
            Period9 = 9,
            Period10 = 10
        }

        /// <summary>
        /// 时间段信息
        /// </summary>
        public class TimeSlot
        {
            /// <summary>
            /// 开始时间 (例如: "8:00")
            /// </summary>
            public string StartTime { get; set; } = string.Empty;

            /// <summary>
            /// 结束时间 (例如: "9:30")
            /// </summary>
            public string EndTime { get; set; } = string.Empty;

            /// <summary>
            /// 时间段描述
            /// </summary>
            public string DisplayTime => $"{StartTime}-{EndTime}";
        }

        /// <summary>
        /// 获取指定教学楼类型和节次的时间安排（两节课一组）
        /// </summary>
        /// <param name="buildingType">教学楼类型</param>
        /// <param name="period">节次</param>
        /// <returns>时间段信息</returns>
        public static TimeSlot GetTimeSlot(BuildingType buildingType, PeriodType period)
        {
            return buildingType switch
            {
                BuildingType.TypeA => GetTypeATimeSlot(period),
                BuildingType.TypeB => GetTypeBTimeSlot(period),
                _ => new TimeSlot()
            };
        }

        /// <summary>
        /// 获取单节课的时间安排
        /// </summary>
        /// <param name="buildingType">教学楼类型</param>
        /// <param name="period">单节课节次 (1-10)</param>
        /// <returns>时间段信息</returns>
        public static TimeSlot GetSinglePeriodTimeSlot(BuildingType buildingType, SinglePeriod period)
        {
            return buildingType switch
            {
                BuildingType.TypeA => GetTypeASinglePeriod(period),
                BuildingType.TypeB => GetTypeBSinglePeriod(period),
                _ => new TimeSlot()
            };
        }

        /// <summary>
        /// 根据数字获取单节课时间 (1-10)
        /// </summary>
        /// <param name="buildingType">教学楼类型</param>
        /// <param name="periodNumber">节次数字 (1-10)</param>
        /// <returns>时间段信息</returns>
        public static TimeSlot GetSinglePeriodTimeSlot(BuildingType buildingType, int periodNumber)
        {
            if (periodNumber < 1 || periodNumber > 10)
                return new TimeSlot();

            return GetSinglePeriodTimeSlot(buildingType, (SinglePeriod)periodNumber);
        }

        /// <summary>
        /// 获取单节课的格式化显示文本
        /// 格式：第X节 (HH:MM-HH:MM)
        /// </summary>
        /// <param name="buildingType">教学楼类型</param>
        /// <param name="periodNumber">节次数字 (1-10)</param>
        /// <returns>格式化文本</returns>
        public static string GetPeriodDisplayText(BuildingType buildingType, int periodNumber)
        {
            var timeSlot = GetSinglePeriodTimeSlot(buildingType, periodNumber);
            if (string.IsNullOrEmpty(timeSlot.StartTime))
                return $"第{periodNumber}节";
            return $"第{periodNumber}节 ({timeSlot.DisplayTime})";
        }

        /// <summary>
        /// 获取单节课的格式化显示文本（使用默认教学楼类型）
        /// </summary>
        /// <param name="periodNumber">节次数字 (1-10)</param>
        /// <returns>格式化文本</returns>
        public static string GetPeriodDisplayText(int periodNumber)
        {
            return GetPeriodDisplayText(BuildingType.TypeA, periodNumber);
        }

        /// <summary>
        /// 根据教学楼名称获取格式化显示文本
        /// </summary>
        /// <param name="buildingName">教学楼名称</param>
        /// <param name="periodNumber">节次数字</param>
        /// <returns>格式化文本</returns>
        public static string GetPeriodDisplayText(string buildingName, int periodNumber)
        {
            var buildingType = GetBuildingType(buildingName);
            return GetPeriodDisplayText(buildingType, periodNumber);
        }

        /// <summary>
        /// 获取A类教学楼的时间安排（两节课一组）
        /// </summary>
        private static TimeSlot GetTypeATimeSlot(PeriodType period)
        {
            return period switch
            {
                PeriodType.Period1_2 => new TimeSlot { StartTime = "8:00", EndTime = "9:30" },
                PeriodType.Break => new TimeSlot { StartTime = "9:30", EndTime = "9:50" },
                PeriodType.Period3_4 => new TimeSlot { StartTime = "9:50", EndTime = "11:20" },
                PeriodType.Period5_6 => new TimeSlot { StartTime = "14:00", EndTime = "15:30" },
                PeriodType.Period7_8 => new TimeSlot { StartTime = "15:50", EndTime = "17:20" },
                PeriodType.Period9_10 => new TimeSlot { StartTime = "19:00", EndTime = "20:30" },
                _ => new TimeSlot()
            };
        }

        /// <summary>
        /// 获取B类教学楼的时间安排（两节课一组）
        /// </summary>
        private static TimeSlot GetTypeBTimeSlot(PeriodType period)
        {
            return period switch
            {
                PeriodType.Period1_2 => new TimeSlot { StartTime = "8:00", EndTime = "9:30" },
                PeriodType.Break => new TimeSlot { StartTime = "9:30", EndTime = "10:00" },
                PeriodType.Period3_4 => new TimeSlot { StartTime = "10:00", EndTime = "11:30" },
                PeriodType.Period5_6 => new TimeSlot { StartTime = "14:00", EndTime = "15:30" },
                PeriodType.Period7_8 => new TimeSlot { StartTime = "15:50", EndTime = "17:20" },
                PeriodType.Period9_10 => new TimeSlot { StartTime = "19:00", EndTime = "20:30" },
                _ => new TimeSlot()
            };
        }

        /// <summary>
        /// 获取A类教学楼单节课的时间安排
        /// 每节课45分钟
        /// </summary>
        private static TimeSlot GetTypeASinglePeriod(SinglePeriod period)
        {
            return period switch
            {
                // 上午
                SinglePeriod.Period1 => new TimeSlot { StartTime = "8:00", EndTime = "8:45" },
                SinglePeriod.Period2 => new TimeSlot { StartTime = "8:45", EndTime = "9:30" },
                SinglePeriod.Period3 => new TimeSlot { StartTime = "9:50", EndTime = "10:35" },
                SinglePeriod.Period4 => new TimeSlot { StartTime = "10:35", EndTime = "11:20" },
                // 下午
                SinglePeriod.Period5 => new TimeSlot { StartTime = "14:00", EndTime = "14:45" },
                SinglePeriod.Period6 => new TimeSlot { StartTime = "14:45", EndTime = "15:30" },
                SinglePeriod.Period7 => new TimeSlot { StartTime = "15:50", EndTime = "16:35" },
                SinglePeriod.Period8 => new TimeSlot { StartTime = "16:35", EndTime = "17:20" },
                // 晚上
                SinglePeriod.Period9 => new TimeSlot { StartTime = "19:00", EndTime = "19:45" },
                SinglePeriod.Period10 => new TimeSlot { StartTime = "19:45", EndTime = "20:30" },
                _ => new TimeSlot()
            };
        }

        /// <summary>
        /// 获取B类教学楼单节课的时间安排
        /// 每节课45分钟
        /// </summary>
        private static TimeSlot GetTypeBSinglePeriod(SinglePeriod period)
        {
            return period switch
            {
                // 上午
                SinglePeriod.Period1 => new TimeSlot { StartTime = "8:00", EndTime = "8:45" },
                SinglePeriod.Period2 => new TimeSlot { StartTime = "8:45", EndTime = "9:30" },
                SinglePeriod.Period3 => new TimeSlot { StartTime = "10:00", EndTime = "10:45" },
                SinglePeriod.Period4 => new TimeSlot { StartTime = "10:45", EndTime = "11:30" },
                // 下午
                SinglePeriod.Period5 => new TimeSlot { StartTime = "14:00", EndTime = "14:45" },
                SinglePeriod.Period6 => new TimeSlot { StartTime = "14:45", EndTime = "15:30" },
                SinglePeriod.Period7 => new TimeSlot { StartTime = "15:50", EndTime = "16:35" },
                SinglePeriod.Period8 => new TimeSlot { StartTime = "16:35", EndTime = "17:20" },
                // 晚上
                SinglePeriod.Period9 => new TimeSlot { StartTime = "19:00", EndTime = "19:45" },
                SinglePeriod.Period10 => new TimeSlot { StartTime = "19:45", EndTime = "20:30" },
                _ => new TimeSlot()
            };
        }

        /// <summary>
        /// 根据教学楼名称获取教学楼类型
        /// </summary>
        /// <param name="buildingName">教学楼名称</param>
        /// <returns>教学楼类型</returns>
        public static BuildingType GetBuildingType(string buildingName)
        {
            if (string.IsNullOrEmpty(buildingName))
                return BuildingType.TypeA;

            // A类教学楼
            string[] typeABuildings = new[]
            {
                "第二教学楼", "第四教学楼", "综合楼", "行政楼", "商学实验中心",
                "工学实验中心", "室内外体育课"
            };

            // B类教学楼
            string[] typeBBuildings = new[]
            {
                "第三教学楼", "第五教学楼", "第一教学楼"
            };

            // 东校第三教学楼是A类，西校第三教学楼是B类，需要特殊处理
            if (buildingName.Contains("第三教学楼"))
            {
                // 根据上下文判断，这里简化处理
                // 如果有校区信息可以进一步判断
                return BuildingType.TypeA; // 默认东校
            }

            if (typeABuildings.Any(b => buildingName.Contains(b)))
                return BuildingType.TypeA;

            if (typeBBuildings.Any(b => buildingName.Contains(b)))
                return BuildingType.TypeB;

            // 默认返回A类
            return BuildingType.TypeA;
        }

        /// <summary>
        /// 根据校区和教学楼名称获取教学楼类型
        /// </summary>
        /// <param name="campus">校区 (东校/西校)</param>
        /// <param name="buildingName">教学楼名称</param>
        /// <returns>教学楼类型</returns>
        public static BuildingType GetBuildingType(string campus, string buildingName)
        {
            if (string.IsNullOrEmpty(buildingName))
                return BuildingType.TypeA;

            bool isEastCampus = campus.Contains("东") || campus == "1";
            bool isWestCampus = campus.Contains("西") || campus == "2";

            // 第三教学楼在东校是A类，在西校是B类
            if (buildingName.Contains("第三教学楼"))
            {
                return isEastCampus ? BuildingType.TypeA : BuildingType.TypeB;
            }

            // A类教学楼
            string[] typeABuildings = new[]
            {
                "第二教学楼", "第四教学楼", "综合楼", "行政楼", "商学实验中心",
                "工学实验中心", "室内外体育课"
            };

            // B类教学楼
            string[] typeBBuildings = new[]
            {
                "第五教学楼", "第一教学楼", "第二教学楼"
            };

            if (typeABuildings.Any(b => buildingName.Contains(b)))
                return BuildingType.TypeA;

            if (typeBBuildings.Any(b => buildingName.Contains(b)))
                return BuildingType.TypeB;

            return BuildingType.TypeA;
        }

        /// <summary>
        /// 获取某教学楼类型的所有时间安排（两节课一组）
        /// </summary>
        /// <param name="buildingType">教学楼类型</param>
        /// <returns>所有节次的时间安排</returns>
        public static Dictionary<PeriodType, TimeSlot> GetAllTimeSlots(BuildingType buildingType)
        {
            var result = new Dictionary<PeriodType, TimeSlot>();
            foreach (PeriodType period in Enum.GetValues<PeriodType>())
            {
                result[period] = GetTimeSlot(buildingType, period);
            }
            return result;
        }

        /// <summary>
        /// 获取某教学楼类型的所有单节课时间安排
        /// </summary>
        /// <param name="buildingType">教学楼类型</param>
        /// <returns>所有单节课的时间安排</returns>
        public static Dictionary<SinglePeriod, TimeSlot> GetAllSinglePeriodTimeSlots(BuildingType buildingType)
        {
            var result = new Dictionary<SinglePeriod, TimeSlot>();
            foreach (SinglePeriod period in Enum.GetValues<SinglePeriod>())
            {
                result[period] = GetSinglePeriodTimeSlot(buildingType, period);
            }
            return result;
        }

        /// <summary>
        /// 解析教务系统返回的节次字符串 (如 "1,2" 或 "3" 或 "5,6,7,8")
        /// </summary>
        /// <param name="periodString">节次字符串</param>
        /// <returns>节次数字列表</returns>
        public static List<int> ParsePeriodString(string periodString)
        {
            var periods = new List<int>();
            if (string.IsNullOrWhiteSpace(periodString))
                return periods;

            var parts = periodString.Split(',');
            foreach (var part in parts)
            {
                if (int.TryParse(part.Trim(), out int period))
                {
                    if (period >= 1 && period <= 10)
                        periods.Add(period);
                }
            }
            return periods;
        }

        /// <summary>
        /// 根据教务系统的节次字符串获取完整时间范围
        /// </summary>
        /// <param name="buildingType">教学楼类型</param>
        /// <param name="periodString">节次字符串 (如 "1,2" 或 "3,4,5")</param>
        /// <returns>合并后的时间段</returns>
        public static TimeSlot? GetTimeRangeFromPeriodString(BuildingType buildingType, string periodString)
        {
            var periods = ParsePeriodString(periodString);
            if (periods.Count == 0)
                return null;

            var firstPeriod = GetSinglePeriodTimeSlot(buildingType, periods.First());
            var lastPeriod = GetSinglePeriodTimeSlot(buildingType, periods.Last());

            if (string.IsNullOrEmpty(firstPeriod.StartTime) || string.IsNullOrEmpty(lastPeriod.EndTime))
                return null;

            return new TimeSlot
            {
                StartTime = firstPeriod.StartTime,
                EndTime = lastPeriod.EndTime
            };
        }

        /// <summary>
        /// 根据教务系统的节次字符串获取显示文本
        /// </summary>
        /// <param name="buildingType">教学楼类型</param>
        /// <param name="periodString">节次字符串 (如 "1,2" 或 "3")</param>
        /// <returns>格式化文本</returns>
        public static string GetDisplayTextFromPeriodString(BuildingType buildingType, string periodString)
        {
            var periods = ParsePeriodString(periodString);
            if (periods.Count == 0)
                return periodString;

            if (periods.Count == 1)
            {
                return GetPeriodDisplayText(buildingType, periods.First());
            }

            var timeRange = GetTimeRangeFromPeriodString(buildingType, periodString);
            if (timeRange == null)
                return $"第{periodString}节";

            return $"第{periodString}节 ({timeRange.DisplayTime})";
        }

        /// <summary>
        /// 根据教务系统的节次字符串获取显示文本（使用默认类型）
        /// </summary>
        /// <param name="periodString">节次字符串</param>
        /// <returns>格式化文本</returns>
        public static string GetDisplayTextFromPeriodString(string periodString)
        {
            return GetDisplayTextFromPeriodString(BuildingType.TypeA, periodString);
        }
    }
}
