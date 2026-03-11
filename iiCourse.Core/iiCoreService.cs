using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using iiCourse.Core.Models;

namespace iiCourse.Core
{
    /// <summary>
    /// iiCourse Core Service - Provides educational system related functionality
    /// </summary>
    public class iiCoreService : IDisposable
    {
        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;
        private readonly HttpClientHandler _handler;
        private bool _loginStatus;
        private UserInfo? _userInfo;

        /// <summary>
        /// Log callback
        /// </summary>
        public Action<string>? LogCallback { get; set; }

        public bool IsLogin => _loginStatus;

        public iiCoreService()
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
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Login to educational system
        /// </summary>
        public async Task<(bool success, string message)> LoginAsync(string username, string password)
        {
            try
            {
                Log("Starting login process...");
                Log($"Username: {username}");
                Log($"Password length: {password.Length}");

                Log("Step 1: Access login page...");
                var response = await _client.GetAsync("https://cas.sdtbu.edu.cn/cas/login");
                var content = await response.Content.ReadAsStringAsync();
                Log($"Login page response status: {response.StatusCode}");
                Log($"Response length: {content.Length}");

                var currentUrl = response.RequestMessage?.RequestUri?.ToString() ?? "";
                Log($"Current URL: {currentUrl}");

                if (currentUrl == "https://zhss.sdtbu.edu.cn/tp_up/view?m=up")
                {
                    Log("Detected logged-in state, verifying user info...");
                    var verifyResult = await VerifyLoginAsync(username);
                    if (verifyResult)
                    {
                        Log("Verification successful, already logged in");
                        _loginStatus = true;
                        return (true, "Already logged in");
                    }
                }

                Log("Step 2: Parse page to get LT value...");
                var doc = new HtmlDocument();
                doc.LoadHtml(content);
                var ltNode = doc.DocumentNode.SelectSingleNode("//input[@id='lt']");
                if (ltNode == null)
                {
                    Log("Error: Cannot find LT element");
                    Log($"Page content preview: {content[..Math.Min(500, content.Length)]}...");
                    return (false, "Cannot get LT value");
                }
                var lt = ltNode.GetAttributeValue("value", "");
                Log($"Got LT value: {lt}");

                Log("Step 3: Generate RSA encryption...");
                var combinedData = $"{username}{password}{lt}";
                Log($"Combined data: {combinedData}");
                var rsa = DesHelper.StrEnc(combinedData, "1", "2", "3");
                Log($"RSA encryption result: {rsa}");

                var loginData = new Dictionary<string, string>
                {
                    { "rsa", rsa },
                    { "ul", username.Length.ToString() },
                    { "pl", password.Length.ToString() },
                    { "lt", lt },
                    { "execution", "e1s1" },
                    { "_eventId", "submit" }
                };

                Log("Step 4: Send login request to WMH system...");
                var loginUrl = "https://cas.sdtbu.edu.cn/cas/login?service=http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/wxH6/wpHome";
                var postResponse = await _client.PostAsync(loginUrl, new FormUrlEncodedContent(loginData));
                var postContent = await postResponse.Content.ReadAsStringAsync();
                Log($"POST response status: {postResponse.StatusCode}");
                Log($"POST response length: {postContent.Length}");

                Log("Step 5: Access WMH home page...");
                var wmhResponse = await _client.GetAsync("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/wxH6/wpHome");
                Log($"WMH home page response status: {wmhResponse.StatusCode}");

                Log("Step 6: Access zhss to verify user info...");
                var zhssResponse = await _client.GetAsync("https://cas.sdtbu.edu.cn/cas/login?service=https://zhss.sdtbu.edu.cn/tp_up/");
                Log($"zhss response status: {zhssResponse.StatusCode}");

                Log("Step 7: Verify login status...");
                var verifyResult2 = await VerifyLoginAsync(username);
                if (verifyResult2)
                {
                    _loginStatus = true;
                    Log("Login successful!");
                    return (true, "Login successful");
                }
                else
                {
                    Log("Login verification failed: User info mismatch");
                    return (false, "Login verification failed");
                }
            }
            catch (Exception ex)
            {
                Log($"Login exception: {ex.Message}");
                Log($"Stack trace: {ex.StackTrace}");
                return (false, $"Login exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Verify login status by getting user info
        /// </summary>
        private async Task<bool> VerifyLoginAsync(string username)
        {
            try
            {
                Log("Verifying user info...");
                var beOptId = DesHelper.StrEnc(username, "tp", "des", "param");
                var data = new { BE_OPT_ID = beOptId };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

                var response = await _client.PostAsync("https://zhss.sdtbu.edu.cn/tp_up/sys/uacm/profile/getUserInfo", content);
                var result = await response.Content.ReadAsStringAsync();
                Log($"User info response: {result[..Math.Min(200, result.Length)]}...");

                var jsonDoc = JObject.Parse(result);

                if (jsonDoc["ID_NUMBER"] != null)
                {
                    var idNumber = jsonDoc["ID_NUMBER"]?.ToString() ?? "";
                    Log($"Returned student ID: {idNumber}");
                    Log($"Input student ID: {username}");
                    
                    if (idNumber == username)
                    {
                        Log("Student ID match, verification successful");
                        _userInfo = new UserInfo
                        {
                            StudentId = idNumber,
                            Name = jsonDoc["USER_NAME"]?.ToString() ?? "",
                            Gender = jsonDoc["USER_SEX"]?.ToString() ?? "",
                            College = jsonDoc["UNIT_NAME"]?.ToString() ?? ""
                        };
                        return true;
                    }
                }
                
                Log("Student ID mismatch or cannot get student ID");
                return false;
            }
            catch (Exception ex)
            {
                Log($"Verification exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get user info
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
                    StudentId = jsonDoc["ID_NUMBER"]?.ToString() ?? "",
                    Name = jsonDoc["USER_NAME"]?.ToString() ?? "",
                    Gender = jsonDoc["USER_SEX"]?.ToString() ?? "",
                    College = jsonDoc["UNIT_NAME"]?.ToString() ?? ""
                };
                return _userInfo;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get card info
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
                    LastConsumeTime = first["TBSJ"]?.ToString() ?? "",
                    Balance = first["YE"]?.ToString() ?? ""
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get exam scores
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
        /// Get building list
        /// </summary>
        /// <param name="campusId">Campus ID, 1 for East Campus, 2 for West Campus</param>
        public async Task<List<BuildingInfo>> GetBuildingsAsync(string campusId = "1")
        {
            var result = new List<BuildingInfo>();
            try
            {
                var data = new { xq = campusId };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

                var response = await _client.PostAsync("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/kxclassroom/getbuild", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseContent) || responseContent.TrimStart().StartsWith('<'))
                {
                    Log("Failed to get building list: Server returned non-JSON data, may be not logged in");
                    return result;
                }

                var jsonDoc = JArray.Parse(responseContent);

                foreach (var item in jsonDoc)
                {
                    result.Add(new BuildingInfo
                    {
                        Name = item["BUILD"]?.ToString() ?? "",
                        ID = item["BUILD_ID"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to get building list: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// Get spare classrooms
        /// </summary>
        public async Task<List<SpareClassroom>> GetSpareClassroomAsync(int buildingId)
        {
            var result = new List<SpareClassroom>();
            try
            {
                var tasks = new List<Task<HttpResponseMessage>>();
                for (int i = 1; i <= 10; i++)
                {
                    var data = new { build = buildingId.ToString(), time = i.ToString() };
                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
                    tasks.Add(_client.PostAsync("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/kxclassroom/getclassroom", content));
                }

                var responses = await Task.WhenAll(tasks);
                foreach (var response in responses)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(responseContent) || responseContent.TrimStart().StartsWith('<'))
                {
                    Log("Failed to get spare classrooms: Server returned non-JSON data, may be not logged in");
                    continue;
                }

                    var jsonDoc = JArray.Parse(responseContent);
                    foreach (var item in jsonDoc)
                    {
                        result.Add(new SpareClassroom
                        {
                            ClassroomName = item["KXJS"]?.ToString() ?? "",
                            BuildingName = buildingId.ToString(),
                            Period = item["SKJC"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Get spare classrooms exception: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// Get class info
        /// </summary>
        public async Task<List<ClassInfo>> GetClassInfoAsync()
        {
            try
            {
                var content = new StringContent("{}", Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
                var infoResponse = await _client.PostAsync("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/wxH6/wpHome/getLearnweekbyDate", content);
                var infoResult = await infoResponse.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(infoResult) || infoResult.TrimStart().StartsWith('<'))
                {
                    Log("Failed to get class info: Server returned HTML page, may be not logged in or session expired");
                    throw new InvalidOperationException("Failed to get class info, please login again");
                }

                var infoDoc = JObject.Parse(infoResult);

                var isHoliday = infoDoc["isHoliday"]?.ToString() ?? "";
                if (isHoliday == "y")
                {
                    throw new InvalidOperationException("Currently in holiday period, no class info available");
                }

                var learnWeek = infoDoc["learnWeek"]?.ToString() ?? "";
                var schoolYear = infoDoc["schoolYear"]?.ToString() ?? "";
                var semester = infoDoc["semester"]?.ToString() ?? "";

                var classData = new { learnWeek, schoolYear, semester };
                var classJson = JsonConvert.SerializeObject(classData);
                var classContent = new StringContent(classJson, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

                var classResponse = await _client.PostAsync("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/wxH6/wpHome/getWeekClassbyUserId", classContent);
                var classResult = await classResponse.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(classResult) || classResult.TrimStart().StartsWith('<'))
                {
                    Log("Failed to get class details: Server returned HTML page");
                    throw new InvalidOperationException("Failed to get class details");
                }

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
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log($"Get class info exception: {ex.Message}");
                return new List<ClassInfo>();
            }
        }

        private static string GetStringOrDefault(JToken item, string propertyName)
        {
            return item[propertyName]?.ToString() ?? "";
        }

        #region Custom Query Schedule API

        /// <summary>
        /// Get school year list
        /// </summary>
        public async Task<List<SchoolYearInfo>> GetSchoolYearsAsync()
        {
            var result = new List<SchoolYearInfo>();
            try
            {
                var data = new { mapping = "getSchoolYear" };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

                var response = await _client.PostAsync("https://zhss.sdtbu.edu.cn/tp_up/up/widgets/getSchoolYear", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseContent) || responseContent.TrimStart().StartsWith('<'))
                {
                    Log("Failed to get school years: Server returned non-JSON data");
                    return result;
                }

                var jsonDoc = JArray.Parse(responseContent);
                foreach (var item in jsonDoc)
                {
                    result.Add(new SchoolYearInfo
                    {
                        SCHOOL_YEAR = item["SCHOOL_YEAR"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                Log($"Get school years exception: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// Get all user classes (for custom query)
        /// </summary>
        public async Task<List<UserClassInfo>> GetUserClassesAsync(string schoolYear, string semester, string learnWeek)
        {
            var result = new List<UserClassInfo>();
            try
            {
                var data = new { schoolYear, semester, learnWeek };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

                var response = await _client.PostAsync("https://zhss.sdtbu.edu.cn/tp_up/up/widgets/getClassbyUserInfo", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseContent) || responseContent.TrimStart().StartsWith('<'))
                {
                    Log("Failed to get user classes: Server returned non-JSON data");
                    return result;
                }

                var jsonDoc = JArray.Parse(responseContent);
                foreach (var item in jsonDoc)
                {
                    result.Add(new UserClassInfo
                    {
                        SKZC = item["SKZC"]?.ToString() ?? "",
                        JSGH = item["JSGH"]?.ToString() ?? "",
                        KKXND = item["KKXND"]?.ToString() ?? "",
                        JXDD = item["JXDD"]?.ToString() ?? "",
                        KCMC = item["KCMC"]?.ToString() ?? "",
                        XH = item["XH"]?.ToString() ?? "",
                        KKXQM = item["KKXQM"]?.ToString() ?? "",
                        KCH = item["KCH"]?.ToString() ?? "",
                        CXJC = item["CXJC"]?.Value<int>() ?? 1,
                        JXBMC = item["JXBMC"]?.ToString() ?? "",
                        JXBH = item["JXBH"]?.ToString() ?? "",
                        SKXQ = item["SKXQ"]?.Value<int>() ?? 1,
                        SKJC = item["SKJC"]?.Value<int>() ?? 1,
                        QSZ = item["QSZ"]?.ToString() ?? "",
                        ZZZ = item["ZZZ"]?.ToString() ?? "",
                        JSXM = item["JSXM"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                Log($"Get user classes exception: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// Get week date info
        /// </summary>
        public async Task<WeekDateInfo?> GetWeekDatesAsync(string schoolYear, string semester, string learnWeek)
        {
            try
            {
                var data = new { SCHOOL_YEAR = schoolYear, SEMESTER = semester, learnWeek };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

                var response = await _client.PostAsync("https://zhss.sdtbu.edu.cn/tp_up/up/widgets/getDatebyLearnweek", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseContent) || responseContent.TrimStart().StartsWith('<'))
                {
                    Log("Failed to get week dates: Server returned non-JSON data");
                    return null;
                }

                var jsonDoc = JObject.Parse(responseContent);
                return new WeekDateInfo
                {
                    Date1 = jsonDoc["date1"]?.ToString() ?? "",
                    Date2 = jsonDoc["date2"]?.ToString() ?? "",
                    Date3 = jsonDoc["date3"]?.ToString() ?? "",
                    Date4 = jsonDoc["date4"]?.ToString() ?? "",
                    Date5 = jsonDoc["date5"]?.ToString() ?? "",
                    Date6 = jsonDoc["date6"]?.ToString() ?? "",
                    Date7 = jsonDoc["date7"]?.ToString() ?? ""
                };
            }
            catch (Exception ex)
            {
                Log($"Get week dates exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get classes by selected time
        /// </summary>
        public async Task<List<SelectedTimeClassInfo>> GetClassesByTimeAsync(string schoolYear, string semester, string learnWeek, List<UserClassInfo> classList)
        {
            var result = new List<SelectedTimeClassInfo>();
            try
            {
                var data = new
                {
                    schoolYear,
                    semester,
                    learnWeek,
                    classList
                };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

                var response = await _client.PostAsync("https://zhss.sdtbu.edu.cn/tp_up/up/widgets/getClassbyTime", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseContent) || responseContent.TrimStart().StartsWith('<'))
                {
                    Log("Failed to get classes by time: Server returned non-JSON data");
                    return result;
                }

                var jsonDoc = JArray.Parse(responseContent);
                foreach (var item in jsonDoc)
                {
                    result.Add(new SelectedTimeClassInfo
                    {
                        SKZC = item["SKZC"]?.ToString() ?? "",
                        JSGH = item["JSGH"]?.ToString() ?? "",
                        KKXND = item["KKXND"]?.ToString() ?? "",
                        JXDD = item["JXDD"]?.ToString() ?? "",
                        KCMC = item["KCMC"]?.ToString() ?? "",
                        XH = item["XH"]?.ToString() ?? "",
                        KKXQM = item["KKXQM"]?.ToString() ?? "",
                        KCH = item["KCH"]?.ToString() ?? "",
                        CXJC = item["CXJC"]?.Value<int>() ?? 1,
                        JXBMC = item["JXBMC"]?.ToString() ?? "",
                        JXBH = item["JXBH"]?.ToString() ?? "",
                        SKXQ = item["SKXQ"]?.Value<int>() ?? 1,
                        SKJC = item["SKJC"]?.Value<int>() ?? 1,
                        QSZ = item["QSZ"]?.ToString() ?? "",
                        ZZZ = item["ZZZ"]?.ToString() ?? "",
                        JSXM = item["JSXM"]?.ToString() ?? "",
                        SKZ = item["SKZ"]?.ToString() ?? "",
                        ColorNum = item["colorNum"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                Log($"Get classes by time exception: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// Custom query schedule (integrates all steps)
        /// </summary>
        public async Task<(List<SelectedTimeClassInfo> classes, WeekDateInfo? dates, string message)> QueryCustomScheduleAsync(CustomQueryParams parameters)
        {
            try
            {
                var userClasses = await GetUserClassesAsync(parameters.SchoolYear, parameters.Semester, parameters.LearnWeek);
                if (userClasses.Count == 0)
                {
                    return (new List<SelectedTimeClassInfo>(), null, "No class info found");
                }

                var weekDates = await GetWeekDatesAsync(parameters.SchoolYear, parameters.Semester, parameters.LearnWeek);
                var classes = await GetClassesByTimeAsync(parameters.SchoolYear, parameters.Semester, parameters.LearnWeek, userClasses);

                return (classes, weekDates, $"Successfully loaded {classes.Count} classes");
            }
            catch (Exception ex)
            {
                return (new List<SelectedTimeClassInfo>(), null, $"Query failed: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// Get student review list
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
                            YearSemester = tds[1].InnerText.Trim(),
                            Category = tds[2].InnerText.Trim(),
                            Batch = tds[3].InnerText.Trim(),
                            CourseType = tds[4].InnerText.Trim(),
                            StartTime = tds[5].InnerText.Trim(),
                            EndTime = tds[6].InnerText.Trim(),
                            Url = "http://wfw.sdtbu.edu.cn" + (link?.GetAttributeValue("href", "") ?? "")
                        });
                    }
                }

                return new ApiResponse<List<StudentReview>> { Code = 200, Data = result };
            }
            catch
            {
                return new ApiResponse<List<StudentReview>> { Code = 500, Message = "Failed to get review list" };
            }
        }

        /// <summary>
        /// Get student review details
        /// </summary>
        public async Task<ApiResponse<List<StudentReviewDetail>>> GetStudentReviewsDetailAsync()
        {
            try
            {
                var reviewsData = await GetStudentReviewsAsync();
                if (reviewsData.Data == null || reviewsData.Data.Count == 0)
                {
                    return new ApiResponse<List<StudentReviewDetail>> { Code = 404, Message = "No review data" };
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
                            TeacherId = tds[1].InnerText.Trim(),
                            TeacherName = tds[2].InnerText.Trim(),
                            Department = tds[3].InnerText.Trim(),
                            ReviewType = tds[4].InnerText.Trim(),
                            TotalScore = tds[5].InnerText.Trim(),
                            IsReviewed = tds[6].InnerText.Trim(),
                            IsSubmitted = tds[7].InnerText.Trim(),
                            Url = "http://wfw.sdtbu.edu.cn" + (link?.GetAttributeValue("href", "") ?? "")
                        });
                    }
                }

                return new ApiResponse<List<StudentReviewDetail>> { Code = 200, Data = result };
            }
            catch
            {
                return new ApiResponse<List<StudentReviewDetail>> { Code = 500, Message = "Failed to get review details" };
            }
        }

        /// <summary>
        /// Finish student reviews
        /// </summary>
        public async Task<ApiResponse<List<string>>> FinishStudentReviewsAsync()
        {
            try
            {
                var detailData = await GetStudentReviewsDetailAsync();
                if (detailData.Data == null || detailData.Data.Count == 0)
                {
                    return new ApiResponse<List<string>> { Code = 404, Message = "No reviews to complete" };
                }

                var completedTeachers = new HashSet<string>();
                var result = new List<string>();

                foreach (var item in detailData.Data)
                {
                    if (item.IsSubmitted == "Yes")
                    {
                        completedTeachers.Add(item.TeacherId);
                        continue;
                    }
                    if (completedTeachers.Contains(item.TeacherId)) continue;

                    completedTeachers.Add(item.TeacherId);
                    result.Add(item.TeacherName);
                    await FinishOneStudentReviewAsync(item.Url);
                    await Task.Delay(500);
                }

                return new ApiResponse<List<string>> { Code = 200, Data = result };
            }
            catch
            {
                return new ApiResponse<List<string>> { Code = 500, Message = "Failed to complete reviews" };
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

                formData.Add(new KeyValuePair<string, string>("jynr", "Teacher lectures very well, very serious, very responsible, very patient, very caring, very responsible, very educational responsibility"));
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
