# 📚 iiCourseWPF

> 山东工商学院智慧山商桌面客户端 —— 让校园生活更轻松！

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?logo=windows)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## ✨ 功能亮点

| 功能 | 描述 |
|------|------|
| 🔐 一键登录 | 自动处理 CAS 认证，告别繁琐登录流程 |
| 📊 成绩查询 | 期末成绩实时推送，第一时间知道结果 |
| 📅 课表管理 | 周课表一目了然，上课再也不迷路 |
| 🏫 空教室查询 | 找自习室？秒查空闲教室 |
| 💳 一卡通 | 余额、消费记录随时查看 |
| ⭐ 一键评教 | 评教季救星，解放你的双手 |

---

## 🚀 快速开始

### 环境要求
- Windows 10/11
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)

### 安装运行

```bash
# 克隆仓库
git clone https://github.com/iicemeta/iiCourseWPF.git

# 进入项目目录
cd iiCourseWPF

# 运行项目
dotnet run --project iiCourseWPF
```

### 下载 release

直接下载最新版本：[Releases](../../releases) 🎉

---

## 📸 界面预览

> 简洁优雅的界面设计，让操作变得愉悦

| 登录页 | 主界面 | 课表页 |
|--------|--------|--------|
| 即将添加 | 即将添加 | 即将添加 |

---

## 🏗️ 项目架构

```
iiCourseWPF/
├── 📁 iiCourseWPF/          # WPF 客户端
│   ├── Views/               # 页面视图
│   ├── Controls/            # 自定义控件
│   ├── Services/            # 本地服务
│   └── Resources/           # 资源文件
│
├── 📁 iiCourse.Core/        # 核心 API 库
│   ├── iiCoreService.cs     # iiCourse 核心服务
│   ├── DesHelper.cs         # 加密工具
│   └── Models/              # 数据模型
│
└── 📁 docs/                 # 详细文档
```

---

## 📖 文档导航

| 文档 | 说明 |
|------|------|
| [📘 使用指南](docs/usage-guide.md) | 详细的使用教程和技巧 |
| [🔧 开发文档](docs/development.md) | 项目结构、API 说明、贡献指南 |
| [🐛 常见问题](docs/faq.md) | 遇到问题？先看这里 |
| [📋 更新日志](docs/changelog.md) | 版本更新记录 |

---

## 🤝 参与贡献

欢迎提交 Issue 和 PR！

1. 🍴 Fork 本仓库
2. 🌿 创建分支 (`git checkout -b feature/amazing`)
3. 💾 提交更改 (`git commit -m 'Add amazing feature'`)
4. 📤 推送分支 (`git push origin feature/amazing`)
5. 🔀 创建 Pull Request

---

## ⚠️ 免责声明

本项目仅供学习交流使用，使用请遵守学校相关规定。

- 自动评教功能请谨慎使用
- 请妥善保管个人账号信息
- 因使用本工具产生的任何问题，开发者不承担责任

---

## 💖 致谢


感谢每一位贡献者和用户！

---

<div align="center">

Made with 💻 by AiScReam

[🌟 Star 一下](https://github.com/iicemeta/iiCourseWPF) | [🐛 提交问题](../../issues) | [💡 功能建议](../../discussions)

</div>
