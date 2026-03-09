using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace iiCourseWPF.Services
{
    /// <summary>
    /// 安全凭据存储服务
    /// 使用 Windows DPAPI 加密存储账号密码
    /// </summary>
    public class CredentialService
    {
        // 凭据文件存储路径
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "iiCourseWPF"
        );

        private static readonly string CredentialFilePath = Path.Combine(AppDataPath, "credentials.dat");

        // 用于额外保护的熵值（增加破解难度）
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("iiCourseWPF_Credential_Protection_2024");

        /// <summary>
        /// 保存凭据到本地
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="rememberPassword">是否记住密码</param>
        public void SaveCredentials(string username, string password, bool rememberPassword)
        {
            try
            {
                // 确保目录存在
                if (!Directory.Exists(AppDataPath))
                {
                    Directory.CreateDirectory(AppDataPath);
                }

                var credential = new CredentialData
                {
                    Username = username,
                    // 只有勾选记住密码才保存密码
                    Password = rememberPassword ? password : string.Empty,
                    RememberPassword = rememberPassword
                };

                // 序列化为 JSON
                var json = JsonSerializer.Serialize(credential);

                // 使用 DPAPI 加密
                var plainBytes = Encoding.UTF8.GetBytes(json);
                var encryptedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);

                // 写入文件
                File.WriteAllBytes(CredentialFilePath, encryptedBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存凭据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载保存的凭据
        /// </summary>
        /// <returns>凭据数据，如果没有保存则返回 null</returns>
        public CredentialData? LoadCredentials()
        {
            try
            {
                if (!File.Exists(CredentialFilePath))
                {
                    return null;
                }

                // 读取加密数据
                var encryptedBytes = File.ReadAllBytes(CredentialFilePath);

                // 使用 DPAPI 解密
                var plainBytes = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);
                var json = Encoding.UTF8.GetString(plainBytes);

                // 反序列化
                return JsonSerializer.Deserialize<CredentialData>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载凭据失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 清除保存的凭据
        /// </summary>
        public void ClearCredentials()
        {
            try
            {
                if (File.Exists(CredentialFilePath))
                {
                    File.Delete(CredentialFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清除凭据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查是否存在保存的凭据
        /// </summary>
        public bool HasSavedCredentials()
        {
            return File.Exists(CredentialFilePath);
        }
    }

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
}
