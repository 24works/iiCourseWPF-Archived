namespace iiCourse.Core.Services
{
    /// <summary>
    /// 凭据数据模型
    /// </summary>
    public class CredentialData
    {
        /// <summary>
        /// 用户名（学号）
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool RememberPassword { get; set; }
    }

    /// <summary>
    /// 凭据服务接口
    /// </summary>
    public interface ICredentialService
    {
        /// <summary>
        /// 保存凭据到本地
        /// </summary>
        void SaveCredentials(string username, string password, bool rememberPassword);

        /// <summary>
        /// 加载保存的凭据
        /// </summary>
        CredentialData? LoadCredentials();

        /// <summary>
        /// 清除保存的凭据
        /// </summary>
        void ClearCredentials();

        /// <summary>
        /// 检查是否存在保存的凭据
        /// </summary>
        bool HasSavedCredentials();
    }
}
