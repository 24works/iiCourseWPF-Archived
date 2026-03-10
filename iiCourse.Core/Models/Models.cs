namespace iiCourse.Core.Models
{
    /// <summary>
    /// 用户信息模型
    /// </summary>
    public class UserInfo
    {
        public string 学号 { get; set; } = string.Empty;
        public string 姓名 { get; set; } = string.Empty;
        public string 性别 { get; set; } = string.Empty;
        public string 学院 { get; set; } = string.Empty;
    }

    /// <summary>
    /// 一卡通信息模型
    /// </summary>
    public class CardInfo
    {
        public string 上次消费时间 { get; set; } = string.Empty;
        public string 余额 { get; set; } = string.Empty;
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
        public string 名称 { get; set; } = string.Empty;
        public string ID { get; set; } = string.Empty;
    }

    /// <summary>
    /// 空教室信息模型
    /// </summary>
    public class SpareClassroom
    {
        public string 教室名称 { get; set; } = string.Empty;
        public string 教学楼 { get; set; } = string.Empty;
        public string 节次 { get; set; } = string.Empty;
    }

    /// <summary>
    /// 学生评教信息模型
    /// </summary>
    public class StudentReview
    {
        public string 学年学期 { get; set; } = string.Empty;
        public string 评价分类 { get; set; } = string.Empty;
        public string 评价批次 { get; set; } = string.Empty;
        public string 评价课程类别 { get; set; } = string.Empty;
        public string 开始时间 { get; set; } = string.Empty;
        public string 结束时间 { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// 学生评教详情模型
    /// </summary>
    public class StudentReviewDetail
    {
        public string 教师编号 { get; set; } = string.Empty;
        public string 教师姓名 { get; set; } = string.Empty;
        public string 所属院系 { get; set; } = string.Empty;
        public string 评教类别 { get; set; } = string.Empty;
        public string 总评分 { get; set; } = string.Empty;
        public string 已评 { get; set; } = string.Empty;
        public string 是否提交 { get; set; } = string.Empty;
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
        public string date1 { get; set; } = string.Empty;
        public string date2 { get; set; } = string.Empty;
        public string date3 { get; set; } = string.Empty;
        public string date4 { get; set; } = string.Empty;
        public string date5 { get; set; } = string.Empty;
        public string date6 { get; set; } = string.Empty;
        public string date7 { get; set; } = string.Empty;
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
        public string colorNum { get; set; } = string.Empty;
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
