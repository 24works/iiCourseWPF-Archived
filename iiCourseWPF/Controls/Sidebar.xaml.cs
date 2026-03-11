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
        public void UpdateLoginStatus(bool isLoggedIn, string userName = "")
        {
            if (isLoggedIn)
            {
                StatusText.Text = "已登录";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
                UserNameText.Text = userName;
            }
            else
            {
                StatusText.Text = "未登录";
                StatusText.Foreground = System.Windows.Media.Brushes.Gray;
                UserNameText.Text = "";
            }
        }

        /// <summary>
        /// 菜单点击事件处理
        /// </summary>
        private void OnMenuClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                SetActiveMenu(tag);
                MenuClicked?.Invoke(tag);
            }
        }
    }
}