using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using iisdtbu.Models;

namespace iisdtbu
{
    /// <summary>
    /// 智慧山商服务类
    /// </summary>
    public class ZHSSService : IDisposable
    {
        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;
        private readonly HttpClientHandler _handler;
        private bool _loginStatus;
        private UserInfo? _userInfo;

        /// <summary>
        /// 日志回调
        /// </summary>
        public Action<string>? LogCallback { get; set; }

        public bool IsLogin => _loginStatus;

        public ZHSSService()
        {
            _cookieContainer = new CookieContainer();
            _handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true
            };
            _client = new HttpClient(_handler);
            _client.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0");
            _loginStatus = false;
        }

        private void Log(string message)
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            LogCallback?.Invoke(logMessage);
        }

        public void Dispose()
        {
            _client.Dispose();
            _handler.Dispose();
        }

        /// <summary>
        /// 登录智慧山商
        /// </summary>
        public async Task<(bool success, string message)> LoginAsync(string username, string password)
        {
            try
            {
                Log("开始登录流程...");
                Log($"用户名: {username}");
                Log($"密码长度: {password.Length}");

                Log("步骤1: 访问登录页面...");
                var response = await _client.GetAsync("https://cas.sdtbu.edu.cn/cas/login");
                var content = await response.Content.ReadAsStringAsync();
                Log($"登录页面响应状态: {response.StatusCode}");
                Log($"响应长度: {content.Length}");

                var currentUrl = response.RequestMessage?.RequestUri?.ToString() ?? "";
                Log($"当前URL: {currentUrl}");

                if (currentUrl == "https://zhss.sdtbu.edu.cn/tp_up/view?m=up")
                {
                    Log("检测到已登录状态，验证用户信息...");
                    var verifyResult = await VerifyLoginAsync(username);
                    if (verifyResult)
                    {
                        Log("验证成功，已登录");
                        _loginStatus = true;
                        return (true, "已经登录");
                    }
                }

                Log("步骤2: 解析页面获取LT值...");
                var doc = new HtmlDocument();
                doc.LoadHtml(content);
                var ltNode = doc.DocumentNode.SelectSingleNode("//input[@id='lt']");
                if (ltNode == null)
                {
                    Log("错误: 无法找到LT元素");
                    Log($"页面内容预览: {content.Substring(0, Math.Min(500, content.Length))}...");
                    return (false, "无法获取LT值");
                }
                var lt = ltNode.GetAttributeValue("value", "");
                Log($"获取到LT值: {lt}");

                Log("步骤3: 生成RSA加密...");
                var combinedData = $"{username}{password}{lt}";
                Log($"组合数据: {combinedData}");
                var rsa = DesHelper.StrEnc(combinedData, "1", "2", "3");
                Log($"RSA加密结果: {rsa}");

                var loginData = new Dictionary<string, string>
                {
                    { "rsa", rsa },
                    { "ul", username.Length.ToString() },
                    { "pl", password.Length.ToString() },
                    { "lt", lt },
                    { "execution", "e1s1" },
                    { "_eventId", "submit" }
                };

                Log("步骤4: 发送登录请求...");
                var loginUrl = "https://cas.sdtbu.edu.cn/cas/login?service=https://zhss.sdtbu.edu.cn/tp_up/";
                var postResponse = await _client.PostAsync(loginUrl, new FormUrlEncodedContent(loginData));
                var postContent = await postResponse.Content.ReadAsStringAsync();
                Log($"POST响应状态: {postResponse.StatusCode}");
                Log($"POST响应长度: {postContent.Length}");

                Log("步骤5: 访问tp_up...");
                var tpResponse = await _client.GetAsync("https://zhss.sdtbu.edu.cn/tp_up/");
                Log($"tp_up响应状态: {tpResponse.StatusCode}");

                Log("步骤6: 访问成绩系统...");
                var scoreResponse = await _client.GetAsync("https://cas.sdtbu.edu.cn/cas/login?service=http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/score");
                Log($"成绩系统响应状态: {scoreResponse.StatusCode}");

                Log("步骤7: 验证登录状态...");
                var verifyResult2 = await VerifyLoginAsync(username);
                if (verifyResult2)
                {
                    _loginStatus = true;
                    Log("登录成功!");
                    return (true, "登录成功");
                }
                else
                {
                    Log("登录验证失败：用户信息不匹配");
                    return (false, "登录验证失败");
                }
            }
            catch (Exception ex)
            {
                Log($"登录异常: {ex.Message}");
                Log($"堆栈: {ex.StackTrace}");
                return (false, $"登录异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 验证登录状态 - 通过获取用户信息验证
        /// </summary>
        private async Task<bool> VerifyLoginAsync(string username)
        {
            try
            {
                Log("正在验证用户信息...");
                var beOptId = DesHelper.StrEnc(username, "tp", "des", "param");
                var data = new { BE_OPT_ID = beOptId };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

                var response = await _client.PostAsync("https://zhss.sdtbu.edu.cn/tp_up/sys/uacm/profile/getUserInfo", content);
                var result = await response.Content.ReadAsStringAsync();
                Log($"用户信息响应: {result.Substring(0, Math.Min(200, result.Length))}...");

                var jsonDoc = JObject.Parse(result);

                if (jsonDoc["ID_NUMBER"] != null)
                {
                    var idNumber = jsonDoc["ID_NUMBER"]?.ToString() ?? "";
                    Log($"返回的学号: {idNumber}");
                    Log($"输入的学号: {username}");
                    
                    if (idNumber == username)
                    {
                        Log("学号匹配，验证成功");
                        _userInfo = new UserInfo
                        {
                            学号 = idNumber,
                            姓名 = jsonDoc["USER_NAME"]?.ToString() ?? "",
                            性别 = jsonDoc["USER_SEX"]?.ToString() ?? "",
                            学院 = jsonDoc["UNIT_NAME"]?.ToString() ?? ""
                        };
                        return true;
                    }
                }
                
                Log("学号不匹配或无法获取学号");
                return false;
            }
            catch (Exception ex)
            {
                Log($"验证异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public async Task<UserInfo?> GetUserInfoAsync(string username)
        {
            if (_userInfo != null) return _userInfo;

            try
            {
                var beOptId = DesHelper.StrEnc(username, "tp", "des", "param");
                var data = new { BE_OPT_ID = beOptId };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

                var response = await _client.PostAsync("https://zhss.sdtbu.edu.cn/tp_up/sys/uacm/profile/getUserInfo", content);
                var result = await response.Content.ReadAsStringAsync();
                var jsonDoc = JObject.Parse(result);

                _userInfo = new UserInfo
                {
                    学号 = jsonDoc["ID_NUMBER"]?.ToString() ?? "",
                    姓名 = jsonDoc["USER_NAME"]?.ToString() ?? "",
                    性别 = jsonDoc["USER_SEX"]?.ToString() ?? "",
                    学院 = jsonDoc["UNIT_NAME"]?.ToString() ?? ""
                };
                return _userInfo;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取一卡通信息
        /// </summary>
        public async Task<CardInfo?> GetCardInfoAsync()
        {
            try
            {
                var content = new StringContent("{}", Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
                var response = await _client.PostAsync("https://zhss.sdtbu.edu.cn/tp_up/up/subgroup/getOneCardBlance", content);
                var result = await response.Content.ReadAsStringAsync();
                var jsonDoc = JArray.Parse(result);
                var first = jsonDoc[0];

                return new CardInfo
                {
                    上次消费时间 = first["TBSJ"]?.ToString() ?? "",
                    余额 = first["YE"]?.ToString() ?? ""
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取考试成绩
        /// </summary>
        public async Task<string?> GetExamScoreAsync()
        {
            try
            {
                var content = new StringContent("{}", Encoding.UTF8, "application/json");
                var timeResponse = await _client.PostAsync("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/score/getscoretime", content);
                var timeResult = await timeResponse.Content.ReadAsStringAsync();
                var timeObj = JObject.Parse(timeResult);

                var xn = timeObj["XN"]?.ToString() ?? "";
                var xq = timeObj["XQ"]?.ToString() ?? "";

                var scoreData = new { nian = xn, xueqi = xq };
                var scoreJson = JsonConvert.SerializeObject(scoreData);
                var scoreContent = new StringContent(scoreJson, Encoding.UTF8, "application/json");

                var scoreResponse = await _client.PostAsync("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/score/getScoreShow", scoreContent);
                var scoreResult = await scoreResponse.Content.ReadAsStringAsync();
                return scoreResult;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取空教室
        /// </summary>
        public async Task<List<SpareClassroom>> GetSpareClassroomAsync(int buildingId)
        {
            var result = new List<SpareClassroom>();
            try
            {
                var tasks = new List<Task<HttpResponseMessage>>();
                for (int i = 1; i <= 11; i++)
                {
                    var data = new { build = buildingId, time = i };
                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
                    tasks.Add(_client.PostAsync("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/kxclassroom/getclassroom", content));
                }

                var responses = await Task.WhenAll(tasks);
                foreach (var response in responses)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JArray.Parse(responseContent);
                    foreach (var item in jsonDoc)
                    {
                        result.Add(new SpareClassroom
                        {
                            教室名称 = item["jsmc"]?.ToString() ?? "",
                            教学楼 = item["jzwmc"]?.ToString() ?? "",
                            节次 = item["jc"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        /// <summary>
        /// 获取课程信息
        /// </summary>
        public async Task<List<ClassInfo>> GetClassInfoAsync()
        {
            try
            {
                var content = new StringContent("{}", Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
                var infoResponse = await _client.PostAsync("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/wxH6/wpHome/getLearnweekbyDate", content);
                var infoResult = await infoResponse.Content.ReadAsStringAsync();
                var infoDoc = JObject.Parse(infoResult);

                // 检测假期状态
                var isHoliday = infoDoc["isHoliday"]?.ToString() ?? "";
                if (isHoliday == "y")
                {
                    throw new InvalidOperationException("当前处于假期时间段，暂无课程信息");
                }

                var learnWeek = infoDoc["learnWeek"]?.ToString() ?? "";
                var schoolYear = infoDoc["schoolYear"]?.ToString() ?? "";
                var semester = infoDoc["semester"]?.ToString() ?? "";

                var classData = new { learnWeek, schoolYear, semester };
                var classJson = JsonConvert.SerializeObject(classData);
                var classContent = new StringContent(classJson, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

                var classResponse = await _client.PostAsync("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/wxH6/wpHome/getWeekClassbyUserId", classContent);
                var classResult = await classResponse.Content.ReadAsStringAsync();
                var classDoc = JArray.Parse(classResult);

                var result = new List<ClassInfo>();
                foreach (var item in classDoc)
                {
                    result.Add(new ClassInfo
                    {
                        JSXM = GetStringOrDefault(item, "JSXM"),
                        JXBMC = GetStringOrDefault(item, "JXBMC"),
                        ZZZ = GetStringOrDefault(item, "ZZZ"),
                        XH = GetStringOrDefault(item, "XH"),
                        KCMC = GetStringOrDefault(item, "KCMC"),
                        JXDD = GetStringOrDefault(item, "JXDD"),
                        KKXND = GetStringOrDefault(item, "KKXND"),
                        JXBH = GetStringOrDefault(item, "JXBH"),
                        KKXQM = GetStringOrDefault(item, "KKXQM"),
                        JSGH = GetStringOrDefault(item, "JSGH"),
                        CXJC = GetStringOrDefault(item, "CXJC"),
                        QSZ = GetStringOrDefault(item, "QSZ"),
                        ZCSM = GetStringOrDefault(item, "ZCSM"),
                        SKXQ = GetStringOrDefault(item, "SKXQ"),
                        SKJC = GetStringOrDefault(item, "SKJC"),
                        KCH = GetStringOrDefault(item, "KCH")
                    });
                }
                return result;
            }
            catch
            {
                return new List<ClassInfo>();
            }
        }

        private static string GetStringOrDefault(JToken item, string propertyName)
        {
            return item[propertyName]?.ToString() ?? "";
        }

        /// <summary>
        /// 获取学生评教列表
        /// </summary>
        public async Task<ApiResponse<List<StudentReview>>> GetStudentReviewsAsync()
        {
            try
            {
                await _client.GetAsync("http://wfw.sdtbu.edu.cn/sso.jsp");
                await _client.GetAsync("https://cas.sdtbu.edu.cn/cas/login?service=http://wfw.sdtbu.edu.cn/sso.jsp");
                var infoResponse = await _client.GetAsync("http://wfw.sdtbu.edu.cn/jsxsd/xspj/xspj_find.do");
                var info = await infoResponse.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(info);
                var rows = doc.DocumentNode.SelectNodes("//table[@id='Form1']//tr");

                var result = new List<StudentReview>();
                if (rows != null)
                {
                    foreach (var row in rows.Skip(1))
                    {
                        var tds = row.SelectNodes(".//td");
                        if (tds == null || tds.Count < 8) continue;

                        var link = tds[7].SelectSingleNode(".//a");
                        result.Add(new StudentReview
                        {
                            学年学期 = tds[1].InnerText.Trim(),
                            评价分类 = tds[2].InnerText.Trim(),
                            评价批次 = tds[3].InnerText.Trim(),
                            评价课程类别 = tds[4].InnerText.Trim(),
                            开始时间 = tds[5].InnerText.Trim(),
                            结束时间 = tds[6].InnerText.Trim(),
                            Url = "http://wfw.sdtbu.edu.cn" + (link?.GetAttributeValue("href", "") ?? "")
                        });
                    }
                }

                return new ApiResponse<List<StudentReview>> { Code = 200, Data = result };
            }
            catch
            {
                return new ApiResponse<List<StudentReview>> { Code = 500, Message = "获取评教列表失败" };
            }
        }

        /// <summary>
        /// 获取学生评教详情
        /// </summary>
        public async Task<ApiResponse<List<StudentReviewDetail>>> GetStudentReviewsDetailAsync()
        {
            try
            {
                var reviewsData = await GetStudentReviewsAsync();
                if (reviewsData.Data == null || !reviewsData.Data.Any())
                {
                    return new ApiResponse<List<StudentReviewDetail>> { Code = 404, Message = "没有评教数据" };
                }

                var firstReview = reviewsData.Data.First();
                var infoResponse = await _client.GetAsync(firstReview.Url);
                var info = await infoResponse.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(info);
                var rows = doc.DocumentNode.SelectNodes("//table[@id='dataList']//tr");

                var result = new List<StudentReviewDetail>();
                if (rows != null)
                {
                    foreach (var row in rows.Skip(1))
                    {
                        var tds = row.SelectNodes(".//td");
                        if (tds == null || tds.Count < 9) continue;

                        var link = tds[8].SelectSingleNode(".//a");
                        result.Add(new StudentReviewDetail
                        {
                            教师编号 = tds[1].InnerText.Trim(),
                            教师姓名 = tds[2].InnerText.Trim(),
                            所属院系 = tds[3].InnerText.Trim(),
                            评教类别 = tds[4].InnerText.Trim(),
                            总评分 = tds[5].InnerText.Trim(),
                            已评 = tds[6].InnerText.Trim(),
                            是否提交 = tds[7].InnerText.Trim(),
                            Url = "http://wfw.sdtbu.edu.cn" + (link?.GetAttributeValue("href", "") ?? "")
                        });
                    }
                }

                return new ApiResponse<List<StudentReviewDetail>> { Code = 200, Data = result };
            }
            catch
            {
                return new ApiResponse<List<StudentReviewDetail>> { Code = 500, Message = "获取评教详情失败" };
            }
        }

        /// <summary>
        /// 完成学生评教
        /// </summary>
        public async Task<ApiResponse<List<string>>> FinishStudentReviewsAsync()
        {
            try
            {
                var detailData = await GetStudentReviewsDetailAsync();
                if (detailData.Data == null || !detailData.Data.Any())
                {
                    return new ApiResponse<List<string>> { Code = 404, Message = "没有需要评教的课程" };
                }

                var completedTeachers = new HashSet<string>();
                var result = new List<string>();

                foreach (var item in detailData.Data)
                {
                    if (item.是否提交 == "是")
                    {
                        completedTeachers.Add(item.教师编号);
                        continue;
                    }
                    if (completedTeachers.Contains(item.教师编号)) continue;

                    completedTeachers.Add(item.教师编号);
                    result.Add(item.教师姓名);
                    await FinishOneStudentReviewAsync(item.Url);
                    await Task.Delay(500);
                }

                return new ApiResponse<List<string>> { Code = 200, Data = result };
            }
            catch
            {
                return new ApiResponse<List<string>> { Code = 500, Message = "完成评教失败" };
            }
        }

        private async Task FinishOneStudentReviewAsync(string url)
        {
            try
            {
                var pageResponse = await _client.GetAsync(url);
                var pageContent = await pageResponse.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(pageContent);

                var formData = new List<KeyValuePair<string, string>>
                {
                    new("issubmit", "1")
                };

                var hiddenInputs = doc.DocumentNode.SelectNodes("//form[@id='Form1']//input[@type='hidden']");
                if (hiddenInputs != null)
                {
                    foreach (var input in hiddenInputs.Skip(1))
                    {
                        var name = input.GetAttributeValue("name", "");
                        var value = input.GetAttributeValue("value", "");
                        if (!string.IsNullOrEmpty(name))
                        {
                            formData.Add(new KeyValuePair<string, string>(name, value));
                        }
                    }
                }

                var tableRows = doc.DocumentNode.SelectNodes("//form[@id='Form1']//table//tr");
                if (tableRows != null)
                {
                    var sign = false;
                    foreach (var row in tableRows.Skip(1).Take(tableRows.Count - 4))
                    {
                        var tds = row.SelectNodes(".//td");
                        if (tds == null || tds.Count < 2) continue;

                        var firstInput = tds[0].SelectSingleNode(".//input");
                        var radioInputs = tds[1].SelectNodes(".//input");

                        if (firstInput == null || radioInputs == null || radioInputs.Count < 10) continue;

                        formData.Add(new KeyValuePair<string, string>(
                            firstInput.GetAttributeValue("name", ""),
                            firstInput.GetAttributeValue("value", "")));

                        var indices = sign ? new[] { 0, 1, 2, 3, 5, 7, 9 } : new[] { 1, 2, 3, 3, 5, 7, 9 };
                        for (int i = 0; i < Math.Min(indices.Length, radioInputs.Count); i++)
                        {
                            var radio = radioInputs[indices[i]];
                            formData.Add(new KeyValuePair<string, string>(
                                radio.GetAttributeValue("name", ""),
                                radio.GetAttributeValue("value", "")));
                        }
                        sign = true;
                    }

                    var lastRows = tableRows.Skip(tableRows.Count - 3).Take(3).ToList();
                    if (lastRows.Count >= 3)
                    {
                        var lastInput = lastRows[0].SelectSingleNode(".//input");
                        if (lastInput != null)
                        {
                            formData.Add(new KeyValuePair<string, string>(
                                lastInput.GetAttributeValue("name", ""),
                                lastInput.GetAttributeValue("value", "")));
                        }
                    }
                }

                formData.Add(new KeyValuePair<string, string>("jynr", "老师讲课很好，很认真，很负责，很有耐心，很有爱心，很有责任心，很有教育责任心"));
                formData.Add(new KeyValuePair<string, string>("isxtjg", "1"));

                var formContent = new FormUrlEncodedContent(formData);
                var request = new HttpRequestMessage(HttpMethod.Post, "http://wfw.sdtbu.edu.cn/jsxsd/xspj/xspj_save.do")
                {
                    Content = formContent
                };
                request.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                
                await _client.SendAsync(request);
            }
            catch
            {
            }
        }
    }
}
