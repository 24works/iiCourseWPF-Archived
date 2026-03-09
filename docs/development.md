# 🔧 开发文档

> 深入了解 iiCourseWPF 的技术实现

---

## 🏗️ 项目结构

```
iiCourseWPF/
├── iiCourseWPF/                 # WPF 客户端项目
│   ├── App.xaml                 # 应用入口
│   ├── MainWindow.xaml          # 主窗口
│   ├── Views/                   # 视图页面
│   │   ├── LoginView.xaml       # 登录页
│   │   ├── UserInfoView.xaml    # 用户信息
│   │   ├── ScoreView.xaml       # 成绩查询
│   │   ├── ClassScheduleView.xaml # 课表
│   │   ├── SpareClassroomView.xaml # 空教室
│   │   ├── CardInfoView.xaml    # 一卡通
│   │   ├── EvaluationView.xaml  # 评教
│   │   ├── SettingsView.xaml    # 设置
│   │   └── PrivacyView.xaml     # 隐私政策
│   ├── Controls/                # 自定义控件
│   │   └── Sidebar.xaml         # 侧边栏
│   ├── Services/                # 本地服务
│   │   └── CredentialService.cs # 凭据管理
│   └── Resources/               # 资源文件
│       ├── iiCourse.ico         # 应用图标
│       ├── iiCourse.png         # Logo
│       └── Animations.xaml      # 动画资源
│
└── iiCourse.Core/               # 核心 API 库
    ├── ZHSSService.cs           # 智慧山商服务
    ├── DesHelper.cs             # DES 加密
    └── Models/                  # 数据模型
        ├── Models.cs            # 主要模型
        └── ClassTime.cs         # 课程时间配置
```

---

## 🔌 核心 API

### ZHSSService

智慧山商服务的主类，封装了所有业务功能。

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `IsLogin` | `bool` | 登录状态 |
| `LogCallback` | `Action<string>?` | 日志回调 |

#### 方法

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `LoginAsync(username, password)` | `Task<(bool, string)>` | 异步登录 |
| `GetUserInfoAsync(username)` | `Task<UserInfo?>` | 获取用户信息 |
| `GetCardInfoAsync()` | `Task<CardInfo?>` | 获取一卡通信息 |
| `GetExamScoreAsync()` | `Task<JsonElement?>` | 获取成绩 |
| `GetClassInfoAsync()` | `Task<List<ClassInfo>>` | 获取课表 |
| `GetSpareClassroomAsync(buildingId)` | `Task<List<SpareClassroom>>` | 获取空教室 |
| `GetStudentReviewsAsync()` | `Task<ApiResponse<List<StudentReview>>>` | 获取评教列表 |
| `FinishStudentReviewsAsync()` | `Task<ApiResponse<List<string>>>` | 完成评教 |

---

## 🔐 登录流程

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  访问 CAS   │────▶│  获取 LT    │────▶│ DES 加密    │
│  登录页     │     │   令牌      │     │ 密码+LT     │
└─────────────┘     └─────────────┘     └──────┬──────┘
                                               │
┌─────────────┐     ┌─────────────┐     ┌──────▼──────┐
│  验证登录   │◀────│  访问智慧   │◀────│  提交登录   │
│   状态      │     │   山商      │     │   表单      │
└─────────────┘     └─────────────┘     └─────────────┘
```

### DES 加密密钥

- **登录加密**: `1`, `2`, `3`
- **用户信息加密**: `tp`, `des`, `param`

---

## 📦 依赖包

| 包名 | 版本 | 用途 |
|------|------|------|
| HtmlAgilityPack | 1.11.71 | HTML 解析 |
| Newtonsoft.Json | 13.0.4 | JSON 处理 |

---

## 🚀 本地开发

### 环境要求

- Windows 10/11
- Visual Studio 2022 或 VS Code
- .NET 9.0 SDK

### 构建项目

```bash
# 还原依赖
dotnet restore

# 构建解决方案
dotnet build

# 运行项目
dotnet run --project iiCourseWPF

# 发布 Release
dotnet publish iiCourseWPF -c Release -o ./publish
```

---

## 🤝 贡献指南

### 提交 PR 流程

1. Fork 本仓库
2. 创建功能分支：`git checkout -b feature/your-feature`
3. 提交更改：`git commit -m 'Add some feature'`
4. 推送分支：`git push origin feature/your-feature`
5. 创建 Pull Request

### 代码规范

- 使用 C# 10+ 特性
- 启用 Nullable 引用类型
- 异步方法使用 Async 后缀
- 添加适当的 XML 文档注释

---

## 📄 许可证

本项目仅供学习交流使用。
