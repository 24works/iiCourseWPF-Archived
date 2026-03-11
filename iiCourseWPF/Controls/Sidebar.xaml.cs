using System.Windows;
using System.Windows.Controls;

namespace iiCourseWPF.Controls
{
    /// <summary>
    /// 侧边栏控件
    /// </summary>
    public partial class Sidebar : UserControl
    {
        public event Action<string>? MenuClicked;

        public Sidebar()
        {
            InitializeComponent();
            SetActiveMenu("UserInfo");
        }

        /// <summary>
        /// 设置当前激活的菜单项
        /// </summary>
        public void SetActiveMenu(string menuTag)
        {
            // 重置所有按钮样式
            BtnUserInfo.Style = Resources["MenuButtonStyle"] as Style;
            BtnClassSchedule.Style = Resources["MenuButtonStyle"] as Style;
            BtnScore.Style = Resources["MenuButtonStyle"] as Style;
            BtnSpareClassroom.Style = Resources["MenuButtonStyle"] as Style;
            BtnEvaluation.Style = Resources["MenuButtonStyle"] as Style;
            BtnSettings.Style = Resources["MenuButtonStyle"] as Style;
            BtnPrivacy.Style = Resources["MenuButtonStyle"] as Style;

            // 设置当前按钮为激活状态
            Button? activeButton = menuTag switch
            {
                "UserInfo" => BtnUserInfo,
                "ClassSchedule" => BtnClassSchedule,
                "Score" => BtnScore,
                "SpareClassroom" => BtnSpareClassroom,
                "Evaluation" => BtnEvaluation,
                "Settings" => BtnSettings,
                "Privacy" => BtnPrivacy,
                _ => null
            };

            if (activeButton != null)
            {
                activeButton.Style = Resources["ActiveMenuButtonStyle"] as Style;
            }
        }

        /// <summary>
        /// 更新登录状态显示
        /// </summary>
        public void UpdateLoginStatus(bool isLoggedIn, string name = "", string studentId = "")
        {
            if (isLoggedIn)
            {
                StatusText.Text = "已登录";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
                UserNameText.Text = name;
                StudentIdText.Text = studentId;
            }
            else
            {
                StatusText.Text = "未登录";
                StatusText.Foreground = System.Windows.Media.Brushes.Gray;
                UserNameText.Text = "";
                StudentIdText.Text = "";
            }
        }

        /// <summary>
        /// 菜单点击事件处理
        /// </summary>
        private void OnMenuClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                // 不在这里立即更新UI状态
                // 选中状态由 MainWindow 在视图切换完成后统一更新
                // 避免异步加载导致的闪烁问题
                MenuClicked?.Invoke(tag);
            }
        }
    }
}