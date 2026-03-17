// 下载页面配置文件
// 修改此文件即可更新下载页面内容，无需修改页面源码

export interface SystemRequirement {
  icon: string;
  title: string;
  desc: string;
}

export interface InstallStep {
  step: number;
  title: string;
  desc: string;
}

export interface FAQItem {
  question: string;
  answer: string;
}

export interface DownloadVersion {
  id: string;
  name: string;
  arch: string;
  size: string;
  description: string;
  recommended?: boolean;
  requiresDotNet?: boolean;
  href: string;
  mirrorHref: string; // 备用下载链接
}

export interface DownloadCategory {
  title: string;
  description: string;
  versions: DownloadVersion[];
}

export interface DownloadConfig {
  // 页面标题
  pageTitle: string;
  pageSubtitle: string;

  // 版本信息
  currentVersion: string;
  releaseDate: string;

  // GitHub 仓库信息
  githubRepo: string;

  // 下载分类
  downloadCategories: DownloadCategory[];

  // 下载按钮
  downloadButton: {
    text: string;
    icon: string;
    href: string;
    mirrorHref: string;
  };
  changelogButton: {
    text: string;
    href: string;
  };

  // 下载说明
  downloadNote: string;

  // 系统要求
  systemRequirementsTitle: string;
  systemRequirements: SystemRequirement[];

  // 安装步骤
  installStepsTitle: string;
  installSteps: InstallStep[];

  // 常见问题
  faqTitle: string;
  faqItems: FAQItem[];
}

// ============================================
// 版本更新指南：
// 1. 修改下面的 VERSION 常量
// 2. 修改 releaseDate 发布日期
// 3. 所有下载链接会自动更新
// ============================================

// 生成 GitHub Release 下载链接（已禁用下载）
const getReleaseUrl = (version: string, filename: string) => {
  // 返回空字符串禁用下载
  return "#";
};

// 生成备用下载链接（已禁用）
const getMirrorUrl = (version: string, filename: string) => {
  // 返回空字符串禁用下载
  return "#";
};

// 在这里修改版本号
const VERSION = "1.7.200";
const REPO = "24works/iiCourseWPF-Archived";

export const downloadConfig: DownloadConfig = {
  pageTitle: "下载应用",
  pageSubtitle: "免费下载 iiCourse，让教务管理更简单",

  currentVersion: VERSION,
  releaseDate: "2026-03-12", // 在这里修改发布日期

  githubRepo: REPO,

  // 多版本下载配置，与 release.yml 保持一致
  downloadCategories: [
    {
      title: "自包含版本（推荐）",
      description: "无需安装 .NET 9 运行时，下载即用",
      versions: [
        {
          id: "x64-self-contained",
          name: "64位",
          arch: "x64",
          size: "~50MB",
          description: "大多数用户推荐，64位 Windows",
          recommended: true,
          requiresDotNet: false,
          href: getReleaseUrl(VERSION, `iiCourseWPF-v${VERSION}-win-x64-self-contained.zip`),
          mirrorHref: getMirrorUrl(VERSION, `iiCourseWPF-v${VERSION}-win-x64-self-contained.zip`)
        },
        {
          id: "x86-self-contained",
          name: "32位",
          arch: "x86",
          size: "~45MB",
          description: "32位 Windows 系统",
          recommended: false,
          requiresDotNet: false,
          href: getReleaseUrl(VERSION, `iiCourseWPF-v${VERSION}-win-x86-self-contained.zip`),
          mirrorHref: getMirrorUrl(VERSION, `iiCourseWPF-v${VERSION}-win-x86-self-contained.zip`)
        },
        {
          id: "arm64-self-contained",
          name: "ARM64",
          arch: "arm64",
          size: "~45MB",
          description: "Surface Pro X、Copilot+ PC 等 ARM 设备",
          recommended: false,
          requiresDotNet: false,
          href: getReleaseUrl(VERSION, `iiCourseWPF-v${VERSION}-win-arm64-self-contained.zip`),
          mirrorHref: getMirrorUrl(VERSION, `iiCourseWPF-v${VERSION}-win-arm64-self-contained.zip`)
        }
      ]
    },
    {
      title: "框架依赖版本",
      description: "体积较小，需先安装 .NET 9 运行时",
      versions: [
        {
          id: "x64-framework",
          name: "64位",
          arch: "x64",
          size: "~5MB",
          description: "已安装 .NET 9 的 64位系统",
          recommended: false,
          requiresDotNet: true,
          href: getReleaseUrl(VERSION, `iiCourseWPF-v${VERSION}-win-x64-framework.zip`),
          mirrorHref: getMirrorUrl(VERSION, `iiCourseWPF-v${VERSION}-win-x64-framework.zip`)
        },
        {
          id: "x86-framework",
          name: "32位",
          arch: "x86",
          size: "~4MB",
          description: "已安装 .NET 9 的 32位系统",
          recommended: false,
          requiresDotNet: true,
          href: getReleaseUrl(VERSION, `iiCourseWPF-v${VERSION}-win-x86-framework.zip`),
          mirrorHref: getMirrorUrl(VERSION, `iiCourseWPF-v${VERSION}-win-x86-framework.zip`)
        },
        {
          id: "arm64-framework",
          name: "ARM64",
          arch: "arm64",
          size: "~4MB",
          description: "已安装 .NET 9 的 ARM 设备",
          recommended: false,
          requiresDotNet: true,
          href: getReleaseUrl(VERSION, `iiCourseWPF-v${VERSION}-win-arm64-framework.zip`),
          mirrorHref: getMirrorUrl(VERSION, `iiCourseWPF-v${VERSION}-win-arm64-framework.zip`)
        }
      ]
    },
    {
      title: "纯框架依赖版本",
      description: "体积最小，仅适合开发者或磁盘空间极紧张的用户",
      versions: [
        {
          id: "dotnet9-required",
          name: "纯框架依赖",
          arch: "any",
          size: "~200KB",
          description: "开发者或磁盘空间极紧张",
          recommended: false,
          requiresDotNet: true,
          href: getReleaseUrl(VERSION, `iiCourseWPF-v${VERSION}-dotnet9-required.zip`),
          mirrorHref: getMirrorUrl(VERSION, `iiCourseWPF-v${VERSION}-dotnet9-required.zip`)
        }
      ]
    }
  ],

  downloadButton: {
    text: "下载 64位自包含版",
    icon: "🪟",
    href: getReleaseUrl(VERSION, `iiCourseWPF-v${VERSION}-win-x64-self-contained.zip`),
    mirrorHref: getMirrorUrl(VERSION, `iiCourseWPF-v${VERSION}-win-x64-self-contained.zip`)
  },
  changelogButton: {
    text: "查看更新日志",
    href: "/updates"
  },

  downloadNote: "完全免费 · 开源透明 · 本地运行",

  systemRequirementsTitle: "系统要求",
  systemRequirements: [
    { icon: "🪟", title: "操作系统", desc: "Windows 10 1809+ / Windows 11" },
    { icon: "💻", title: "运行环境", desc: "自包含版无需 .NET 9" },
    { icon: "📦", title: "安装包大小", desc: "45MB - 50MB" },
    { icon: "🔌", title: "网络", desc: "需要校园网或 VPN" }
  ],

  installStepsTitle: "安装步骤",
  installSteps: [
    { step: 1, title: "下载安装包", desc: "选择上方对应版本下载" },
    { step: 2, title: "解压文件", desc: "将 zip 文件解压到任意文件夹" },
    { step: 3, title: "运行应用", desc: "双击 iiCourseWPF.exe 启动" },
    { step: 4, title: "开始使用", desc: "使用学号登录，开始查课表" }
  ],

  faqTitle: "常见问题",
  faqItems: [
    {
      question: "应用安全吗？",
      answer: "完全安全。应用完全本地运行，不会上传任何数据到第三方服务器。源代码开源，可审计。"
    },
    {
      question: "需要联网吗？",
      answer: "需要连接校园网或使用 VPN 访问教务系统。查询操作需要实时连接学校服务器。"
    },
    {
      question: "会保存密码吗？",
      answer: "密码可以选择保存到本地（加密存储），也可以每次手动输入，完全由你决定。"
    },
    {
      question: "如何更新？",
      answer: "应用会自动检查更新，也可以在设置中手动检查。更新时会保留所有配置。"
    },
    {
      question: "该选哪个版本？",
      answer: "不确定就选「自包含 64位」。它最大但最省心，无需安装任何运行环境。"
    },
    {
      question: "ARM64 是什么？",
      answer: "ARM64 是 ARM 架构的 64位版本，用于 Surface Pro X、新款 Copilot+ PC 等设备。"
    }
  ]
};

export default downloadConfig;
