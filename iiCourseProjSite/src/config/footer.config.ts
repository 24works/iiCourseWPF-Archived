// Footer 配置文件 - 类似 Docusaurus 的配置方式
// 修改此文件即可更新 Footer 内容，无需修改组件源码

export interface FooterLink {
  label: string;
  href: string;
}

export interface FooterColumn {
  title: string;
  links: FooterLink[];
}

export interface FooterConfig {
  // 品牌区域
  brand: {
    name: string;
    tagline: string;
  };
  // 链接列
  columns: FooterColumn[];
  // 版权信息
  copyright: string;
}

export const footerConfig: FooterConfig = {
  brand: {
    name: "iiCourse",
    tagline: "智慧校园桌面应用，让教务管理更简单。"
  },
  columns: [
    {
      title: "链接",
      links: [
        { label: "功能介绍", href: "/features" },
        { label: "更新日志", href: "/updates" },
        { label: "新闻动态", href: "/news" }
      ]
    },
    {
      title: "开源",
      links: [
        { label: "GitHub", href: "https://github.com/24works/iiCourseWPF-Archived.git" },
        { label: "问题反馈", href: "https://github.com/24works/iiCourseWPF-Archived/issues" }
      ]
    }
  ],
  copyright: "© 2026 iiCourse. All rights reserved."
};

export default footerConfig;
