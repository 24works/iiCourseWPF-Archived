namespace iiCourse.Core.Models
{
    /// <summary>
    /// 用户信息模型
    /// </summary>
    public class UserInfo
    {
        public string StudentId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string College { get; set; } = string.Empty;
    }

    /// <summary>
    /// 一卡通信息模型
    /// </summary>
    public class CardInfo
    {
        public string LastConsumeTime { get; set; } = string.Empty;
        public string Balance { get; set; } = string.Empty;
    }

    /// <summary>
    /// 课程信息模型
    /// </summary>
    public class ClassInfo
    {
        public string JSXM { get; set; } = string.Empty;
        public string JXBMC { get; set; } = string.Empty;
        public string ZZZ { get; set; } = string.Empty;
        public string XH { get; set; } = string.Empty;
        public string KCMC { get; set; } = string.Empty;
        public string JXDD { get; set; } = string.Empty;
        public string KKXND { get; set; } = string.Empty;
        public string JXBH { get; set; } = string.Empty;
        public string KKXQM { get; set; } = string.Empty;
        public string JSGH { get; set; } = string.Empty;
        public string CXJC { get; set; } = string.Empty;
        public string QSZ { get; set; } = string.Empty;
        public string ZCSM { get; set; } = string.Empty;
        public string SKXQ { get; set; } = string.Empty;
        public string SKJC { get; set; } = string.Empty;
        public string KCH { get; set; } = string.Empty;
    }

    /// <summary>
    /// 教学楼信息模型
    /// </summary>
    public class BuildingInfo
    {
        public string Name { get; set; } = string.Empty;
        public string ID { get; set; } = string.Empty;
    }

    /// <summary>
    /// 空教室信息模型
    /// </summary>
    public class SpareClassroom
    {
        public string ClassroomName { get; set; } = string.Empty;
        public string BuildingName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
    }

    /// <summary>
    /// 学生评教信息模型
    /// </summary>
    public class StudentReview
    {
        public string YearSemester { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Batch { get; set; } = string.Empty;
        public string CourseType { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// 学生评教详情模型
    /// </summary>
    public class StudentReviewDetail
    {
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string ReviewType { get; set; } = string.Empty;
        public string TotalScore { get; set; } = string.Empty;
        public string IsReviewed { get; set; } = string.Empty;
        public string IsSubmitted { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// API响应模型
    /// </summary>
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    /// <summary>
    /// 学年信息模型
    /// </summary>
    public class SchoolYearInfo
    {
        public string SCHOOL_YEAR { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用户课程信息模型（自定义查询用）
    /// </summary>
    public class UserClassInfo
    {
        public string SKZC { get; set; } = string.Empty;
        public string JSGH { get; set; } = string.Empty;
        public string KKXND { get; set; } = string.Empty;
        public string JXDD { get; set; } = string.Empty;
        public string KCMC { get; set; } = string.Empty;
        public string XH { get; set; } = string.Empty;
        public string KKXQM { get; set; } = string.Empty;
        public string KCH { get; set; } = string.Empty;
        public int CXJC { get; set; }
        public string JXBMC { get; set; } = string.Empty;
        public string JXBH { get; set; } = string.Empty;
        public int SKXQ { get; set; }
        public int SKJC { get; set; }
        public string QSZ { get; set; } = string.Empty;
        public string ZZZ { get; set; } = string.Empty;
        public string JSXM { get; set; } = string.Empty;
    }

    /// <summary>
    /// 周日期信息模型
    /// </summary>
    public class WeekDateInfo
    {
        public string Date1 { get; set; } = string.Empty;
        public string Date2 { get; set; } = string.Empty;
        public string Date3 { get; set; } = string.Empty;
        public string Date4 { get; set; } = string.Empty;
        public string Date5 { get; set; } = string.Empty;
        public string Date6 { get; set; } = string.Empty;
        public string Date7 { get; set; } = string.Empty;
    }

    /// <summary>
    /// 选定时间课程信息模型
    /// </summary>
    public class SelectedTimeClassInfo
    {
        public string SKZC { get; set; } = string.Empty;
        public string JSGH { get; set; } = string.Empty;
        public string KKXND { get; set; } = string.Empty;
        public string JXDD { get; set; } = string.Empty;
        public string KCMC { get; set; } = string.Empty;
        public string XH { get; set; } = string.Empty;
        public string KKXQM { get; set; } = string.Empty;
        public string KCH { get; set; } = string.Empty;
        public int CXJC { get; set; }
        public string JXBMC { get; set; } = string.Empty;
        public string JXBH { get; set; } = string.Empty;
        public int SKXQ { get; set; }
        public int SKJC { get; set; }
        public string QSZ { get; set; } = string.Empty;
        public string ZZZ { get; set; } = string.Empty;
        public string JSXM { get; set; } = string.Empty;
        public string SKZ { get; set; } = string.Empty;
        public string ColorNum { get; set; } = string.Empty;
    }

    /// <summary>
    /// 自定义查询参数模型
    /// </summary>
    public class CustomQueryParams
    {
        public string SchoolYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string LearnWeek { get; set; } = string.Empty;
    }
}
