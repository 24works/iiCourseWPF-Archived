# iiCourse.Core - 山东工商学院智慧山商 API 服务库

一个用于访问山东工商学院（SDTBU）智慧山商系统的 .NET 类库，提供了登录认证、成绩查询、课表获取、空教室查询、一卡通信息查询以及学生评教等功能。

## 项目信息

| 属性 | 值 |
|------|-----|
| 目标框架 | .NET 9.0 |
| 语言版本 | C# (启用 ImplicitUsings 和 Nullable) |
| 依赖包 | HtmlAgilityPack 1.11.71 |

## 项目结构

```
iiCourse.Core/
├── iiCourse.Core.csproj    # 项目配置文件
├── iiCoreService.cs        # 核心服务类
├── DesHelper.cs            # DES 加密辅助类
└── Models/
    ├── Models.cs           # 数据模型定义
    └── ClassTime.cs        # 课程时间配置
```

## 功能特性

### 1. 用户认证
- 支持学号密码登录智慧山商系统
- 自动处理 CAS 单点登录流程
- 支持 Cookie 会话管理
- 登录状态验证

### 2. 用户信息查询
- 获取当前登录用户的基本信息（学号、姓名、性别、学院）

### 3. 一卡通服务
- 查询一卡通余额
- 查询上次消费时间

### 4. 成绩查询
- 获取当前学期的考试成绩

### 5. 课程表查询
- 获取当前学期的课程安排信息
- 包含课程名称、上课时间、上课地点、任课教师等详细信息

### 6. 空教室查询
- 按教学楼查询空闲教室
- 支持多节次并行查询

### 7. 学生评教
- 获取评教任务列表
- 获取评教详情
- 自动完成评教（一键评教）

## 核心类说明

### iiCoreService

iiCourse 核心服务的主类，提供所有业务功能的实现。

#### 属性

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `IsLogin` | `bool` | 获取当前登录状态 |
| `LogCallback` | `Action<string>?` | 日志回调函数，用于输出操作日志 |

#### 方法

| 方法名 | 返回类型 | 说明 |
|--------|----------|------|
| `LoginAsync(string username, string password)` | `Task<(bool success, string message)>` | 异步登录智慧山商 |
| `GetUserInfoAsync(string username)` | `Task<UserInfo?>` | 获取用户信息 |
| `GetCardInfoAsync()` | `Task<CardInfo?>` | 获取一卡通信息 |
| `GetExamScoreAsync()` | `Task<JsonElement?>` | 获取考试成绩 |
| `GetClassInfoAsync()` | `Task<List<ClassInfo>>` | 获取课程信息 |
| `GetSpareClassroomAsync(int buildingId)` | `Task<List<SpareClassroom>>` | 获取空教室列表 |
| `GetStudentReviewsAsync()` | `Task<ApiResponse<List<StudentReview>>>` | 获取评教列表 |
| `GetStudentReviewsDetailAsync()` | `Task<ApiResponse<List<StudentReviewDetail>>>` | 获取评教详情 |
| `FinishStudentReviewsAsync()` | `Task<ApiResponse<List<string>>>` | 完成所有评教 |

### DesHelper

DES 加密辅助类，用于智慧山商登录时的密码加密。

#### 方法

| 方法名 | 说明 |
|--------|------|
| `StrEnc(string data, string firstKey, string secondKey, string thirdKey)` | 使用三重密钥进行 DES 加密 |
| `StrDec(string data, string firstKey, string secondKey, string thirdKey)` | 使用三重密钥进行 DES 解密 |

## 数据模型

### UserInfo - 用户信息

| 属性 | 类型 | 说明 |
|------|------|------|
| 学号 | `string` | 学生学号 |
| 姓名 | `string` | 学生姓名 |
| 性别 | `string` | 性别 |
| 学院 | `string` | 所属学院 |

### CardInfo - 一卡通信息

| 属性 | 类型 | 说明 |
|------|------|------|
| 上次消费时间 | `string` | 最近一次消费的时间 |
| 余额 | `string` | 一卡通余额 |

### ClassInfo - 课程信息

| 属性 | 类型 | 说明 |
|------|------|------|
| JSXM | `string` | 教师姓名 |
| JXBMC | `string` | 教学班名称 |
| ZZZ | `string` | 周次 |
| XH | `string` | 学号 |
| KCMC | `string` | 课程名称 |
| JXDD | `string` | 教学地点 |
| KKXND | `string` | 开课学年 |
| JXBH | `string` | 教学班号 |
| KKXQM | `string` | 开课学期 |
| JSGH | `string` | 教师工号 |
| CXJC | `string` | 持续节次 |
| QSZ | `string` | 起始周 |
| ZCSM | `string` | 周次说明 |
| SKXQ | `string` | 上课星期 |
| SKJC | `string` | 上节课次 |
| KCH | `string` | 课程号 |

### SpareClassroom - 空教室信息

| 属性 | 类型 | 说明 |
|------|------|------|
| 教室名称 | `string` | 教室名称 |
| 教学楼 | `string` | 所在教学楼 |
| 节次 | `string` | 空闲节次 |

### StudentReview - 评教信息

| 属性 | 类型 | 说明 |
|------|------|------|
| 学年学期 | `string` | 学年学期 |
| 评价分类 | `string` | 评价分类 |
| 评价批次 | `string` | 评价批次 |
| 评价课程类别 | `string` | 评价课程类别 |
| 开始时间 | `string` | 评教开始时间 |
| 结束时间 | `string` | 评教结束时间 |
| Url | `string` | 评教链接 |

### StudentReviewDetail - 评教详情

| 属性 | 类型 | 说明 |
|------|------|------|
| 教师编号 | `string` | 教师编号 |
| 教师姓名 | `string` | 教师姓名 |
| 所属院系 | `string` | 所属院系 |
| 评教类别 | `string` | 评教类别 |
| 总评分 | `string` | 总评分 |
| 已评 | `string` | 是否已评 |
| 是否提交 | `string` | 是否已提交 |
| Url | `string` | 评教链接 |

### ApiResponse\<T\> - API 响应模型

| 属性 | 类型 | 说明 |
|------|------|------|
| Code | `int` | 响应状态码 |
| Message | `string` | 响应消息 |
| Data | `T?` | 响应数据 |

## 使用示例

### 基本使用

```csharp
using iiCourse.Core;

// 创建服务实例
using var service = new iiCoreService
{
    LogCallback = message => Console.WriteLine(message)
};

// 登录
var (success, message) = await service.LoginAsync("学号", "密码");
if (!success)
{
    Console.WriteLine($"登录失败: {message}");
    return;
}

// 获取用户信息
var userInfo = await service.GetUserInfoAsync("学号");
Console.WriteLine($"姓名: {userInfo?.姓名}, 学院: {userInfo?.学院}");

// 获取一卡通信息
var cardInfo = await service.GetCardInfoAsync();
Console.WriteLine($"余额: {cardInfo?.余额}");

// 获取课程表
var classes = await service.GetClassInfoAsync();
foreach (var c in classes)
{
    Console.WriteLine($"{c.KCMC} - {c.JXDD} - {c.JSXM}");
}

// 获取空教室
var classrooms = await service.GetSpareClassroomAsync(1);
foreach (var room in classrooms)
{
    Console.WriteLine($"{room.教室名称} - {room.教学楼}");
}

// 完成评教
var reviewResult = await service.FinishStudentReviewsAsync();
if (reviewResult.Code == 200)
{
    Console.WriteLine($"已完成评教: {string.Join(", ", reviewResult.Data ?? new List<string>())}");
}
```

## 技术实现说明

### 登录流程

1. 访问 CAS 登录页面获取 LT 令牌
2. 使用 DES 加密算法对用户名、密码和 LT 进行加密
3. 提交加密后的登录表单
4. 访问智慧山商系统完成会话建立
5. 验证登录状态

### DES 加密

登录时使用的加密方式为自定义的 DES 变体，支持三重密钥加密：
- firstKey: "1"
- secondKey: "2"  
- thirdKey: "3"

用户信息查询时使用的加密密钥：
- firstKey: "tp"
- secondKey: "des"
- thirdKey: "param"

## 注意事项

1. 本库仅供学习和研究使用，请勿用于非法用途
2. 使用时请遵守学校相关规章制度
3. 自动评教功能请谨慎使用，建议手动完成评教以保证评价质量
4. 网络请求可能因学校系统更新而失效，请及时关注接口变化

## 依赖说明

- **HtmlAgilityPack**: 用于解析 HTML 页面，提取登录所需的 LT 令牌等数据

## 许可证

本项目仅供学习交流使用。
