using Core.Common;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Object lockObj = new object();
        private static SocketsHttpHandler socketHandler;
        private static readonly HttpClient httpClient;
        private static WuShuang ws;
        private static Dictionary<MessageType, string> messageDic;
        CancellationTokenSource TokenSource;

        #region 静态构造函数（初始化静态变量）
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static MainWindow()
        {
            //UseCookies：默认为 true，指示处理程序是否使用 CookieContainer 属性来存储服务器 Cookie 并在发送请求时使用这些 Cookie
            //MaxConnectionsPerServer：允许向单个服务器连接的最大并行 TCP 连接数，默认 2
            //ConnectTimeout：接建立超时前等待的时间跨度，默认值为 InfiniteTimeSpan（无限等待）
            socketHandler = new SocketsHttpHandler() { UseCookies = true, MaxConnectionsPerServer = 500 };//UseProxy = false
            httpClient = new HttpClient(socketHandler);
            httpClient.Timeout = TimeSpan.FromSeconds(10);//设置超时时间 30 秒
            ws = new WuShuang();

            messageDic = new Dictionary<MessageType, string>()
            {
                { MessageType.None, null },
                { MessageType.GettingVerificationCode, "获取验证码......" },
                { MessageType.VerificationCodeEmpty, "请输入验证码！" },
                { MessageType.VerificationCodeIncorrect, "验证码不正确，请刷新验证码后重新输入！" },
                { MessageType.VerificationCodeExpired, "验证码已经失效，请刷新验证码后重新输入！" },
                { MessageType.UserNameEmpty, "请输入用户名！" },
                { MessageType.UserNameNonExistent, "用户名不存在！" },
                { MessageType.PasswordEmpty, "请输入密码！" },
                { MessageType.PasswordIncorrect, "密码不正确！" },
                { MessageType.SSOError, "单点登录异常！请前往官网退出账号再尝试！" },
                { MessageType.LoggingIn, "正在登录......" },
                { MessageType.LoggingOut, "正在退出......" },
                { MessageType.LoginSuccess, "登录成功！" },
                { MessageType.LoginFailed, "登录失败！请重试！" },
                { MessageType.LoginExpired, "登录已失效！请重新登录！" },
                { MessageType.GettingPageInfo, "获取道具箱信息......" },
                { MessageType.PageStartGreaterThanPageEnd, "起始页码不能大于结束页码" },
                { MessageType.PageEndGreaterThanPageTotal, "结束页码不能大于总页数" },
                { MessageType.Doanloading, "正在下载......" },
                { MessageType.DownloadSuccess,  "下载成功！" },
                { MessageType.NoData,  "没有任何数据！" },
                { MessageType.SaveCanceled,  "已取消保存！" },
                { MessageType.HttpRequestError,  "网络请求错误！" },
                { MessageType.UnknownError,  "未知错误！请重试！" }
            };
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //服务器有检测 Referer
            httpClient.DefaultRequestHeaders.Add("Referer", GetAppSettings("LoginURI"));
        }

        #region 窗口事件
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BindComboBoxData();

            ws.UserName = "Z575758320";
            ws.Password = "Z57575832008";
            //ws.UserName = "Yang252087987";
            //ws.Password = "Yang030201";
            this.DataContext = ws;//会触发道具索引下拉选择框的 SelectionChanged 事件

            //if (!(await isConnAsync()))
            //{
            //    MessageBox.Show("连接超时，请检查您的网络或者本机防火墙设置！", "提示");
            //    return;
            //}

            //GetValidateImg();
            await GetValidateImgAsync();
            

            ////直接进入登录状态，方便测试
            //Cookie cookie = new Cookie("Cjit2_UAC_User", "UserId=lgFsfK3QnJWNTs3B3VIVOA==&UserName=dYPC2pDwFrsG1b2EGssJdA==", "/", GetAppSettings("PropDomain"));
            //socketHandler.CookieContainer.Add(cookie);
            ////获取道具箱页面信息
            //ws.PageInfo = await GetPageInfoAsync();
            //ws.PageTotal = ws.PageInfo.Total;
            //ws.PageEnd = ws.PageInfo.Total;
            //ws.IsLogin = true;
        }

        /// <summary>
        /// 窗口拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !GridWrap.IsMouseOver)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ws.IsLogin)
            {
                Task<MessageType> task = Logout(true);
                ShowLoading(task, MessageType.LoggingOut);
            }
            this.Close();
        }

        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
        #endregion

        #region 密码是否可见状态切换
        /// <summary>
        /// 密码是否可见状态切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EyeBtn_Click(object sender, RoutedEventArgs e)
        {
            ws.IsShowRealValue = !ws.IsShowRealValue;
            PassWordConverter pwc = this.Resources["PassWordConverter"] as PassWordConverter;
            pwc.IsShowRealValue = ws.IsShowRealValue;
            ws.Password = ws.Password;//触发属性更改通知
        }
        #endregion

        #region 验证码图片点击事件
        /// <summary>
        /// 验证码图片点击事件（鼠标指针位于此元素上并且松开鼠标左键时发生）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidateCodeImg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GetValidateImg();
        }
        #endregion

        #region 道具索引选项改变事件
        /// <summary>
        /// 道具索引选项改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PropIndexCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Task.Delay(1);
            if (ws.IsLogin)
            {
                if (ws.PageInfo != null && ws.PageInfo.FormDataList != null)
                {
                    string key = "ctl00$ContentPlaceHolder1$ddlSource";//道具索引 id
                    int index = ws.PageInfo.FormDataList.FindIndex(x => x.Key == key);
                    if (index > -1)
                        ws.PageInfo.FormDataList.RemoveAt(index);
                    ws.PageInfo.FormDataList.Add(new KeyValuePair<string, string>(key, ws.PropIndex));
                }

                //获取道具箱页面信息
                SetPageInfo();
            }
        }
        #endregion

        #region 登录/退出事件 ======
        /// <summary>
        /// 登录/退出事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            //登录
            if(!ws.IsLogin)
            {
                if (string.IsNullOrWhiteSpace(ws.UserName))
                {
                    MessageBox.Show(GetMessage(MessageType.UserNameEmpty), "提示");
                    return;
                }
                if (string.IsNullOrWhiteSpace(ws.Password))
                {
                    MessageBox.Show(GetMessage(MessageType.PasswordEmpty), "提示");
                    return;
                }
                if (string.IsNullOrWhiteSpace(ws.ValidateCode))
                {
                    MessageBox.Show(GetMessage(MessageType.VerificationCodeEmpty), "提示");
                    return;
                }

                //if (!(await isConnAsync()))
                //{
                //    MessageBox.Show("连接超时，请检查您的网络或者本机防火墙设置！", "提示");
                //    return;
                //}

                Task<MessageType> task = Login();
                ShowLoading(task, MessageType.LoggingIn);

                if (ws.IsLogin)
                {
                    //获取道具箱页面信息
                    SetPageInfo();
                }
            }
            else //退出
            {
                Task<MessageType> task = Logout();
                ShowLoading(task, MessageType.LoggingOut);
            }
        }
        #endregion

        #region 下载事件 ======
        /// <summary>
        /// 下载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Download_Click(object sender, RoutedEventArgs e)
        {
            if(ws.PageStart > ws.PageEnd)
            {
                MessageBox.Show(GetMessage(MessageType.PageStartGreaterThanPageEnd), "提示");
                return;
            }
            if (ws.PageEnd > ws.PageTotal)
            {
                MessageBox.Show(GetMessage(MessageType.PageEndGreaterThanPageTotal), "提示");
                return;
            }
            Task<MessageType> task = DownloadAsync();
            ShowLoading(task, MessageType.Doanloading);
        }
        #endregion

        #region 登录 ======
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        private async Task<MessageType> Login()
        {
            MessageType msgType = MessageType.None;
            string submitUrl = GetAppSettings("SubmitUrl");

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserName", ws.UserName),
                new KeyValuePair<string, string>("UserPassword", ws.Password),
                new KeyValuePair<string, string>("ValidateCode", ws.ValidateCode),
                new KeyValuePair<string, string>("returnurl", "")
            });
            try
            {
                using (HttpResponseMessage response = await httpClient.PostAsync(submitUrl, formData).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        //{"status":6,"message":"验证码不正确，请刷新验证码后重新输入。"}
                        if (content.Contains("验证码不正确"))
                        {
                            msgType = MessageType.VerificationCodeIncorrect;
                        }
                        //{"status":5,"message":"验证码已经失效，请刷新验证码后重新输入。"}
                        else if (content.Contains("验证码已经失效"))
                        {
                            //GetValidateImg();
                            //await GetValidateImgAsync();
                            msgType = MessageType.VerificationCodeExpired;
                        }
                        //{"status":222,"message":"该通行证账号不存在"}
                        else if(content.Contains("该通行证账号不存在"))
                        {
                            msgType = MessageType.UserNameNonExistent;
                        }
                        //{"status":7,"message":"对不起，您输入的账号或密码不正确。"}
                        else if(content.Contains("密码不正确"))
                        {
                            msgType = MessageType.PasswordIncorrect;
                        }
                        //{"status":12,"message":"账号SSO登录出现异常，请联系管理员。"}
                        else if(content.Contains("账号SSO登录出现异常"))
                        {
                            msgType = MessageType.SSOError;
                        }
                        //{"status":10001,"message":"超过一个月没修改密码了。。。"}

                        if (msgType != MessageType.None) return msgType;
                    }

                    //获取用于保持登录状态的 Cookie “Cjit2_UAC_User”
                    //  Cjit2_UAC_User：UserId=Base64Str&UserName=Base64Str&StrSiteIds=Base64Str
                    //  “登录页面”与“道具箱页面”分别位于不同的服务器（域名），但都是使用 Cookie“Cjit2_UAC_User”来保持登录状态，
                    //  因此需要获取“登录页面”的“Cjit2_UAC_User”并保存到“道具箱页面”同名的 Cookie 中
                    string cookieName = "Cjit2_UAC_User";
                    IEnumerable<string> values = null;
                    if (response.Headers.TryGetValues("Set-Cookie", out values) && values.Any(x => x.Contains(cookieName)))
                    {
                        string cookieValue = Regex.Match(values.First(x => x.Contains(cookieName)), @"UserId=[A-Za-z0-9\+/=]+&UserName=[A-Za-z0-9\+/=]+")?.Value;
                        //var cookies = socketHandler.CookieContainer.GetCookies(new Uri(GetAppSettings("LoginURI")));
                        //cookieValue = Regex.Replace(cookies.First(x => x.Name == cookieName).Value, @"&StrSiteIds=[A-Za-z0-9\+/=]+", "");

                        Cookie cookie = new Cookie(cookieName, cookieValue, "/", GetAppSettings("PropDomain"));
                        socketHandler.CookieContainer.Add(cookie);

                        //获取道具箱页面信息
                        //SetPageInfo();

                        msgType = MessageType.LoginSuccess;
                        ws.IsLogin = true;//更改为“已登录”状态
                    }
                    else
                        msgType = MessageType.LoginFailed;
                }
            }
            //某个根本问题（例如网络连接、DNS 失败、服务器证书验证或超时）导致请求失败
            catch (HttpRequestException ex)
            {
                msgType = MessageType.HttpRequestError;
                await ErrorLogAsync(ex);
            }
            catch (Exception ex)
            {
                msgType = MessageType.UnknownError;
                await ErrorLogAsync(ex);
            }
            return msgType;
        }
        #endregion

        #region 退出 ======
        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="isClose">是否关闭程序（默认值 false）</param>
        /// <returns></returns>
        private async Task<MessageType> Logout(bool isClose = false)
        {
            string logoutUrl = GetAppSettings("LogoutUrl");
            try
            {
                //HttpCompletionOption.ResponseHeadersRead：操作应在响应可用且读取标头后立即完成，尚未读取内容！
                HttpResponseMessage response = await httpClient.GetAsync(logoutUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                response.Dispose();
            }
            //某个根本问题（例如网络连接、DNS 失败、服务器证书验证或超时）导致请求失败
            catch (HttpRequestException ex)
            {
                await ErrorLogAsync(ex);
                return isClose ? MessageType.None : MessageType.HttpRequestError;
            }

            if (!isClose)
            {
                //恢复默认值
                await RecoveryDefaultAsync();
            }
            return MessageType.None;
        }
        #endregion

        #region 获取我的道具箱数据

        #region 获取道具箱页面信息（总页数、页参数集合（隐藏域））
        /// <summary>
        /// 获取道具箱页面信息（总页数、页参数集合（隐藏域））
        /// </summary>
        /// <returns></returns>
        private async Task<PageInfo> GetPageInfoAsync()
        {
            //第一次进入【我的道具箱】页面，此时加载的是所有类型（道具索引）的数据的第一页（此时不获取道具列表数据，只获取相关请求表单信息）
            PageInfo pageInfo = null;
            string content;
            string propUrl = GetAppSettings("PropUrl");
            Task<string> task; 
            if(ws.PageInfo != null && ws.PageInfo.FormDataList != null)
                // KeyValuePair<TKey, TValue> 是 struct（值类型），ToList() 相当于深拷贝
                task = GetPageContentAsync(propUrl, ws.PageInfo.FormDataList.ToList(), 1, CancellationToken.None);
            else
                task = httpClient.GetStringAsync(propUrl);
            try
            {
                content = await task;
            }
            //某个根本问题（例如网络连接、DNS 失败、服务器证书验证或超时）导致请求失败
            catch (HttpRequestException ex)
            {
                await ErrorLogAsync(ex);
                ShowTips(MessageType.HttpRequestError);
                return pageInfo;
            }
            catch (Exception ex)
            {
                await ErrorLogAsync(ex);
                ShowTips(MessageType.UnknownError);
                return pageInfo;
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                #region 获取所需参数（隐藏域）
                //参数说明（隐藏域）：
                //  __VIEWSTATE：视图状态
                //  __VIEWSTATEGENERATOR：视图状态生长期
                //  __EVENTTARGET：触发回发事件的“控件id”（此处为 ctl00$ContentPlaceHolder1$pager（分页控件id））
                //  __EVENTARGUMENT：回发事件参数（此处为“目标页”）
                //  __EVENTVALIDATION：验证回发事件的参数是否来源于最初呈现这些事件的服务器控件
                //  ctl00$ContentPlaceHolder1$ddlSource：“道具索引选项”
                //  ctl00$ContentPlaceHolder1$pager_input：“当前页”当总页数>50？或者>100？才出现？（没有多余账号来测试￣□￣｜｜）
                List<KeyValuePair<string, string>> formDataList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("__EVENTTARGET", "ctl00$ContentPlaceHolder1$pager"),
                    //new KeyValuePair<string, string>("__EVENTARGUMENT", "1"),
                    new KeyValuePair<string, string>("ctl00$ContentPlaceHolder1$ddlSource", ws.PropIndex)
                    //new KeyValuePair<string, string>("ctl00$ContentPlaceHolder1$pager_input", "1")
                };
                string[] param = { "__VIEWSTATE", "__VIEWSTATEGENERATOR", "__EVENTVALIDATION" };
                foreach (string p in param)
                {
                    string value = Regex.Match(content, $"<input type=\"hidden\" name=\"{p}\" id=\"{p}\" value=\"(\\S*)\"\\s*/>")?.Groups[1].Value;
                    formDataList.Add(new KeyValuePair<string, string>(p, value));
                }
                #endregion

                #region 获取总页数
                //获取总页数：
                //  分页以 10 为一组（1-10、11-20、...；不足 10 的组则只显示拥有的页序号，例如只有 21-24 则显示 21,22,23,24），每页 15 条记录
                //  当总页数“>50？或者>100？（没有多余账号来测试￣□￣｜｜）”会出现“页面跳转输入框和按钮”以及如下字符串：
                //      ANP_checkInput('ctl00$ContentPlaceHolder1$pager_input',pageTotal)，pageTotal 即为“最大页序号”
                //  设置 ctl00$ContentPlaceHolder1$pager_input 为“1”
                int pageTotal = 0;
                //如果存在“页面跳转输入框和按钮”，则提取总页数
                Match match = Regex.Match(content, @"ANP_checkInput\('ctl00\$ContentPlaceHolder1\$pager_input',(\d+)\)");
                if (match.Success)
                {
                    pageTotal = int.Parse(match.Groups[1].Value);
                    formDataList.Add(new KeyValuePair<string, string>("ctl00$ContentPlaceHolder1$pager_input", "1"));
                }
                else //以 10 的倍数值为“组序号”，循环递增“组序号”，最后获取总页数
                {
                    //选中页序号（数量 0 or 1）
                    string pattern_Selected = "<td class=\"on\"><a href=\"#\" onclick=\"return false;\">(\\d+)</a></td>";
                    //其他页序号（数量 >=0 and <=9）
                    string pattern_Other = "<a title=\"转到第\\d+页\" href=\"javascript:__doPostBack\\('ctl00\\$ContentPlaceHolder1\\$pager','\\d+'\\)\">(\\d+)</a>";
                    //下一组（数量 0 or 1）
                    string pattern_NextGroup = "<a href=\"javascript:__doPostBack\\('ctl00\\$ContentPlaceHolder1\\$pager','\\d+1'\\)\" title=\"转到第\\d+1页\">...</a>";
                    Match match_Selected = Regex.Match(content, pattern_Selected);
                    MatchCollection mc_Other = Regex.Matches(content, pattern_Other);
                    Match match_NextGroup = Regex.Match(content, pattern_NextGroup);
                    int groupIndex = 11;
                    while(match_Selected.Success && mc_Other.Count >= 9 && match_NextGroup.Success)
                    {
                        try
                        {
                            // KeyValuePair<TKey, TValue> 是 struct（值类型），ToList() 相当于深拷贝
                            content = await GetPageContentAsync(propUrl, formDataList.ToList(), groupIndex, CancellationToken.None);
                        }
                        //某个根本问题（例如网络连接、DNS 失败、服务器证书验证或超时）导致请求失败
                        catch (HttpRequestException ex)
                        {
                            await ErrorLogAsync(ex);
                            ShowTips(MessageType.HttpRequestError);
                            return pageInfo;
                        }
                        catch(Exception ex)
                        {
                            await ErrorLogAsync(ex);
                            ShowTips(MessageType.UnknownError);
                            return pageInfo;
                        }
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            match_Selected = Regex.Match(content, pattern_Selected);
                            mc_Other = Regex.Matches(content, pattern_Other);
                            match_NextGroup = Regex.Match(content, pattern_NextGroup);

                            groupIndex += 10;
                        }
                        else
                            break;
                    }
                    if (mc_Other.Count > 0)
                        pageTotal = int.Parse(mc_Other[mc_Other.Count - 1].Groups[1].Value);
                    else if (match_Selected.Success)
                        pageTotal = int.Parse(match_Selected.Groups[1].Value);
                }
                #endregion

                pageInfo = new PageInfo();
                pageInfo.Total = pageTotal;
                pageInfo.FormDataList = formDataList;
            }

            return pageInfo;
        }
        #endregion

        #region 获取页内容
        /// <summary>
        /// 获取页内容
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="formDataList"></param>
        /// <param name="page">页序号 >=1</param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<string> GetPageContentAsync(string requestUri, List<KeyValuePair<string, string>> formDataList, int page, CancellationToken token)
        {
            string content = null;
            // __EVENTARGUMENT：分页序号
            formDataList.Add(new KeyValuePair<string, string>("__EVENTARGUMENT", page.ToString()));
            var formData = new FormUrlEncodedContent(formDataList);
            using (HttpResponseMessage response = await httpClient.PostAsync(requestUri, formData, token).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            return content;
        }
        #endregion

        #region 下载 ======
        /// <summary>
        /// 下载
        /// </summary>
        /// <returns></returns>
        private async Task<MessageType> DownloadAsync()
        {
            MessageType msgType = MessageType.NoData;
            if (ws.PageInfo != null && ws.PageInfo.Total > 0 && ws.PageEnd <= ws.PageTotal && ws.PageStart <= ws.PageEnd)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("优惠券号码", typeof(string));
                dt.Columns.Add("优惠券描述", typeof(string));
                dt.Columns.Add("获得类别", typeof(string));
                dt.Columns.Add("获得日期", typeof(DateTime));
                dt.Columns.Add("状态", typeof(string));
                dt.Columns.Add("兑换古钱", typeof(string));

                TokenSource = new CancellationTokenSource();
                string propUrl = GetAppSettings("PropUrl");
                int i = ws.PageStart;
                List<Task> tasks = new List<Task>();
                while(i <= ws.PageEnd)
                {
                    // KeyValuePair<TKey, TValue> 是 struct（值类型），ToList() 相当于深拷贝
                    Task task = CollectPageDataAsync(dt, propUrl, ws.PageInfo.FormDataList.ToList(), i, TokenSource.Token);
                    tasks.Add(task);
                    i++;
                }

                try
                {
                    //等待所有任务完成：
                    //  ①所有任务中的所有异常传播到此处被捕获
                    //  ②任务可能执行成功、失败或者被取消
                    await Task.WhenAll(tasks);
                }
                //一个或者多个错误
                catch (AggregateException ae)
                {
                    // TaskCanceledException 派生自 OperationCanceledException
                    if (ae.InnerExceptions.Any(ex => ex is TaskCanceledException))
                        msgType = MessageType.LoginExpired;
                    //某个根本问题（例如网络连接、DNS 失败、服务器证书验证或超时）导致请求失败
                    else if (ae.InnerExceptions.Any(ex => ex is HttpRequestException))
                        msgType = MessageType.HttpRequestError;
                    else
                        msgType = MessageType.UnknownError;

                    await ErrorLogAsync(ae);
                    return msgType;
                }
                finally
                {
                    TokenSource = null;
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    try
                    {
                        DataView dv = dt.DefaultView;
                        dv.Sort = "获得日期 Desc";//日期倒序
                        if (!string.IsNullOrWhiteSpace(ws.Status))
                            dv.RowFilter = $"状态 = '{ws.Status}'";//状态筛选
                        dt = dv.ToTable();
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            //this.Dispatcher.Invoke(() =>
                            //{
                            //    //保存到 txt 文件
                            //    if (ws.FileType == 1)
                            //        msgType = WriteToTxt(dt);
                            //    //保存到 Excel 文件
                            //    else
                            //        msgType = SaveAsExcel(dt);
                            //});
                            //保存到 txt 文件
                            if (ws.FileType == 1)
                                msgType = await WriteToTxtAsync(dt);
                            //保存到 Excel 文件
                            else
                                msgType = await SaveAsExcelAsync(dt);
                        }
                    }
                    catch(Exception ex)
                    {
                        msgType = MessageType.UnknownError;
                        await ErrorLogAsync(ex);
                    }
                }
            }
            return msgType;
        }
        #endregion

        #region 采集每页的数据
        /// <summary>
        /// 采集每页的数据
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="requestUri"></param>
        /// <param name="formDataList"></param>
        /// <param name="page">页序号 >=1</param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task CollectPageDataAsync(DataTable dt, string requestUri, List<KeyValuePair<string, string>> formDataList, int page, CancellationToken token)
        {
            //任务已取消
            //if (token.IsCancellationRequested) return;
            if (token.IsCancellationRequested) token.ThrowIfCancellationRequested();//引发 OperationCanceledException（派生 TaskCanceledException）

            string content = await GetPageContentAsync(requestUri, formDataList, page, token);
            if (!string.IsNullOrWhiteSpace(content))
            {
                //当服务器 Session 失效（session过期/丢失）或者 本程序全局Cookie丢失/改变，进入【我的道具箱】页面将会提示“请先登录”
                if (content.Contains("alert('请先登录')"))
                {
                    //取消 未执行/正在执行 的任务
                    TokenSource?.Cancel();
                    return;
                }

                string pattern = "<table width=\"100%\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"list\">([\\s\\S]*?)</table>";//*?：非贪心的
                MatchCollection mc = Regex.Matches(content, pattern);
                if (mc.Count > 1)
                {
                    pattern = "<td width=\"\\d{1,2}%\" align=\"center\"[\\s\\S]*?>([\\s\\S]*?)</td>";//*?：非贪心的
                    foreach (Match match in mc)
                    {
                        MatchCollection mcTd = Regex.Matches(match.Groups[1].Value, pattern);

                        DataRow dr = dt.NewRow();
                        dr["优惠券号码"] = Regex.Replace(mcTd[0].Groups[1].Value, "[\\r\\n]", "").Trim();
                        dr["优惠券描述"] = Regex.Replace(mcTd[1].Groups[1].Value, "[\\r\\n]", "").Trim();
                        dr["获得类别"] = Regex.Replace(mcTd[2].Groups[1].Value, "[\\r\\n]", "").Trim();
                        dr["获得日期"] = Regex.Replace(mcTd[3].Groups[1].Value, "[\\r\\n]", "").Trim();
                        dr["状态"] = Regex.Replace(mcTd[4].Groups[1].Value, "[\\r\\n]", "").Trim();
                        dr["兑换古钱"] = Regex.Match(mcTd[5].Groups[1].Value, "[\\u4e00-\\u9fa5]+")?.Value.Trim();

                        lock (lockObj)
                        {
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
        }
        #endregion

        #endregion

        #region 写入txt文件
        /// <summary>
        /// 写入txt文件
        /// </summary>
        /// <param name="dt"></param>
        public async Task<MessageType> WriteToTxtAsync(DataTable dt)
        {
            MessageType msgType = MessageType.None;
            try
            {
                string saveFullPath = string.Empty;
                string fileName = $"我的道具箱_{ws.UserName}";
                
                KeyValueModel propIndex = this.PropIndexCB.SelectedItem as KeyValueModel;
                KeyValueModel status = this.StatusCB.SelectedItem as KeyValueModel;
                //道具索引
                if (!string.IsNullOrWhiteSpace(propIndex.Key))
                    fileName += $"_{propIndex.Value}";
                fileName += $"_{ws.PageStart}-{ws.PageEnd}";
                //状态
                if (!string.IsNullOrWhiteSpace(status.Key))
                    fileName += $"_{status.Value}";

                //保存到程序根目录
                if (ws.IsSaveToRoot)
                {
                    saveFullPath = Path.Combine(GetDirectory(), $"{fileName}.txt");
                }
                else
                {
                    //用户选择文件保存位置
                    System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                    sfd.Filter = "Txt文件|*.txt";
                    sfd.Title = "保存为txt文件";
                    sfd.FilterIndex = 2;
                    sfd.RestoreDirectory = true;
                    sfd.FileName = fileName;
                    sfd.DefaultExt = "txt";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        saveFullPath = sfd.FileName;
                    else
                       return MessageType.SaveCanceled;
                }
                //创建写入文件  FileMode.Create：创建新文件， 如果此文件已存在，则会将其覆盖
                //FileStream fs = new FileStream(strFilePath, FileMode.Create, FileAccess.Write);
                //StreamWriter sw = new StreamWriter(fs, Encoding.UTF8));
                //如果该文件不存在，将创建一个新文件；如果该文件存在，则覆盖。 
                using (StreamWriter sw = new StreamWriter(saveFullPath, false, Encoding.UTF8))
                {
                    string CouponNoCol = "优惠券号码";
                    string CouponDescriptionCol = "优惠券描述";
                    string CategoryCol = "获得类别";
                    string GetDateCol = "获得日期";
                    string StateCol = "状态";
                    string ExchangeCol = "兑换古钱";
                    int CouponDescriptionColWidth = Encoding.UTF8.GetByteCount("");

                    //sw.WriteLine($"{CouponNoCol}{CouponDescriptionCol}{CategoryCol}{GetDateCol}{StateCol}{ExchangeCol}");//写入值
                    //int len = dt.AsEnumerable().Max(dr => dr.Field<string>(CouponDescriptionCol).Length);//获得“优惠券描述”最长字符串的长度
                    //int len = dt.AsEnumerable().Max(dr => Encoding.UTF8.GetByteCount(dr.Field<string>(CouponDescriptionCol)));//获得“优惠券描述”UTF8编码最长字符串的长度
                    string interval = string.Empty.PadRight(10, ' ');
                    foreach (DataRow dr in dt.Rows)
                    {
                        string CouponNo = dr[CouponNoCol].ToString();
                        string CouponDescription = dr[CouponDescriptionCol].ToString().TrimEnd(',');
                        string Category = dr[CategoryCol].ToString();
                        string GetDate = dr[GetDateCol].ToString();
                        string State = dr[StateCol].ToString();
                        string Exchange = dr[ExchangeCol].ToString();

                        int len = 55, strLen = Encoding.UTF8.GetByteCount(CouponDescription);
                        if (len > strLen)
                            CouponDescription = CouponDescription + string.Empty.PadRight(len - strLen, ' ');

                        await sw.WriteLineAsync($"{CouponNo}{interval}{CouponDescription}{interval}{Category}{interval}{GetDate}{interval}{State}{interval}{Exchange}");//写入值
                    }
                }

                ws.SaveFullPath = saveFullPath;
                msgType = MessageType.DownloadSuccess;
            }
            catch (Exception ex)
            {
                msgType = MessageType.UnknownError;
                await ErrorLogAsync(ex);
            }

            return msgType;
        }
        #endregion

        #region 保存到 Excel文件
        /// <summary>
        /// 保存到 Excel文件
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public async Task<MessageType> SaveAsExcelAsync(DataTable dt)
        {
            MessageType msgType = MessageType.None;
            try
            {
                string saveFullPath = string.Empty;
                string fileName = $"我的道具箱_{ws.UserName}";

                KeyValueModel propIndex = this.PropIndexCB.SelectedItem as KeyValueModel;
                KeyValueModel status = this.StatusCB.SelectedItem as KeyValueModel;
                //道具索引
                if (!string.IsNullOrWhiteSpace(propIndex.Key))
                    fileName += $"_{propIndex.Value}";
                fileName += $"_{ws.PageStart}-{ws.PageEnd}";
                //状态
                if (!string.IsNullOrWhiteSpace(status.Key))
                    fileName += $"_{status.Value}";

                //保存到程序根目录
                if (ws.IsSaveToRoot)
                {
                    saveFullPath = Path.Combine(GetDirectory(), $"{fileName}.xlsx");
                }
                else
                {
                    //用户选择文件保存位置
                    System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                    sfd.Filter = "Excel文件|*.xlsx";
                    sfd.Title = "保存为Excel文件";
                    sfd.FilterIndex = 2;
                    sfd.RestoreDirectory = true;
                    sfd.FileName = fileName;
                    sfd.DefaultExt = "xlsx";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        saveFullPath = sfd.FileName;
                    else
                        return MessageType.SaveCanceled;
                }
                await ExcelExportHelper.ExportExcelAsync(dt, saveFullPath, "真三国无双OLZ-我的道具箱");

                ws.SaveFullPath = saveFullPath;
                msgType = MessageType.DownloadSuccess;
            }
            catch (Exception ex)
            {
                msgType = MessageType.UnknownError;
                await ErrorLogAsync(ex);
            }
            return msgType;
        }
        #endregion

        #region 获取验证码图片
        /// <summary>
        /// 获取验证码
        /// </summary>
        private void GetValidateImg()
        {
            Task<MessageType> task = GetValidateImgAsync();
            ShowLoading(task, MessageType.GettingVerificationCode);
        }

        /// <summary>
        /// 获取验证码图片
        /// </summary>
        private async Task<MessageType> GetValidateImgAsync()
        {
            MessageType msgType = MessageType.None;
            string validateCodeUrl = GetAppSettings("ValidateCodeUrl");
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, validateCodeUrl))
                {
                    //request.Headers.Add("Accept", "image/gif,image/x-xbitmap,image/jpeg,image/pjpeg,image/webp,image/apng,image/*,*/*;q=0.8");
                    request.Headers.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");
                    //request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    //request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
                    //request.Headers.Add("Referer", loginURI);//服务器有检测 Referer，必须与验证码地址同一域名
                    //request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.81 Safari/537.36 SE 2.X MetaSr 1.0");
                    //request.Headers.Add("Cookie", "ASP.NET_SessionId=x1zqwyi2port3oca31tzyzui; path=/; HttpOnly");
                    using (HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();

                        byte[] buffer = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        if (buffer != null && buffer.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream(buffer))
                            {
                                DrawImg(ms);
                            }
                        }
                        else
                            DrawImg();
                    }
                }
            }
            //某个根本问题（例如网络连接、DNS 失败、服务器证书验证或超时）导致请求失败
            catch (HttpRequestException ex)
            {
                msgType = MessageType.HttpRequestError;
                await ErrorLogAsync(ex);
            }
            catch(Exception ex)
            {
                msgType = MessageType.UnknownError;
                await ErrorLogAsync(ex);
            }
            return msgType;
        }

        #region 绘图 ======
        /// <summary>
        /// 绘图
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="tips"></param>
        private void DrawImg(Stream stream = null)
        {
            //Invoke 到主线程执行
            this.Dispatcher.Invoke(() =>
            {
                Bitmap bmp = new Bitmap((int)this.ValidateCodeImg.Width, (int)this.ValidateCodeImg.Height);
                if (stream != null)
                    bmp = new Bitmap(stream);
                else
                {
                    Graphics g = Graphics.FromImage(bmp);
                    //清空图片背景色
                    g.Clear(System.Drawing.Color.White);
                    //画图片的干扰线
                    Random random = new Random();
                    int width = (int)this.ValidateCodeImg.Width;
                    int height = (int)this.ValidateCodeImg.Height;
                    for (int i = 0; i < 25; i++)
                    {
                        int x1 = random.Next(width);
                        int x2 = random.Next(width);
                        int y1 = random.Next(height);
                        int y2 = random.Next(height);
                        g.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Silver), x1, y1, x2, y2);
                    }
                    //g.DrawString("点击刷新", new Font("宋体", 12), new SolidBrush(Color.Black), new PointF(14, 13));
                    g.DrawString("点击刷新", new Font("微软雅黑", 12), new SolidBrush(System.Drawing.Color.Black), 14, 13);
                }

                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                BitmapImage bitImg = new BitmapImage();
                bitImg.BeginInit();
                bitImg.StreamSource = ms;
                bitImg.EndInit();
                this.ValidateCodeImg.Source = bitImg;
            });
        }
        #endregion

        #endregion

        #region 设置 WuShuang.PageInfo
        /// <summary>
        /// 设置 WuShuang.PageInfo
        /// </summary>
        private void SetPageInfo()
        {
            //获取道具箱页面信息
            Task<MessageType> task = SetPageInfoAsync();
            //ShowLoading(task, TimeSpan.FromMilliseconds(500), GetMessage(MessageType.GettingPageInfo));
            ShowLoading(task, MessageType.GettingPageInfo);
        }

        /// <summary>
        /// 获取道具箱页面信息并设置 WuShuang.PageInfo
        /// </summary>
        /// <returns></returns>
        private async Task<MessageType> SetPageInfoAsync()
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(500);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();//开始计时
            try
            {
                //获取道具箱页面信息
                ws.PageInfo = await GetPageInfoAsync();
            }
            catch(Exception ex)
            {
                await ErrorLogAsync(ex);
                return MessageType.UnknownError;
            }
            stopwatch.Stop();//停止计时
            TimeSpan ts = stopwatch.Elapsed;
            //大于 任务耗时
            if (timeSpan.CompareTo(ts) > 0)
                await Task.Delay(timeSpan - ts);
            if (ws.PageInfo != null)
            {
                ws.PageTotal = ws.PageInfo.Total;
                ws.PageEnd = ws.PageInfo.Total;
                ws.PageStart = ws.PageTotal > 0 ? 1 : 0;//页序号从“1”开始
            }
            return MessageType.None;
        }
        #endregion

        #region 恢复默认值
        /// <summary>
        /// 恢复默认值
        /// </summary>
        /// <returns></returns>
        private async Task RecoveryDefaultAsync()
        {
            ws.IsLogin = false;//已退出
            ws.UserName = string.Empty;
            ws.Password = string.Empty;
            ws.ValidateCode = string.Empty;
            ws.PropIndex = string.Empty;
            ws.Status = string.Empty;
            //ws.FileType = 0;
            //ws.IsSaveToRoot = true;
            //ws.IsShowRealValue = false;
            ws.PageInfo = null;
            ws.PageTotal = 0;
            ws.PageStart = 0;
            ws.PageEnd = 0;
            ws.SaveFullPath = null;

            //GetValidateImg();
            await GetValidateImgAsync();
        }
        #endregion

        #region 绑定下拉选择框数据
        /// <summary>
        /// 绑定下拉选择框数据
        /// </summary>
        private void BindComboBoxData()
        {
            //道具索引
            List<KeyValueModel> propIndexList = new List<KeyValueModel>()
            { 
                new KeyValueModel(){ Key = "", Value = "请选择"},
                new KeyValueModel(){ Key = "4", Value = "攻城略地"},
                new KeyValueModel(){ Key = "6", Value = "一元店活动"},
                new KeyValueModel(){ Key = "8", Value = "古钱商城"},
                new KeyValueModel(){ Key = "9", Value = "八阵图"},
                new KeyValueModel(){ Key = "10", Value = "活动奖励"}
            };
            this.PropIndexCB.ItemsSource = propIndexList;
            this.PropIndexCB.SelectedIndex = 0;

            //状态
            List<KeyValueModel> statusList = new List<KeyValueModel>()
            {
                new KeyValueModel(){ Key = "", Value = "请选择"},
                new KeyValueModel(){ Key = "可用", Value = "可用"},
                new KeyValueModel(){ Key = "已领取", Value = "已领取"},
                new KeyValueModel(){ Key = "作废", Value = "作废"}
            };
            this.StatusCB.ItemsSource = statusList;
            this.StatusCB.SelectedIndex = 0;
        }
        #endregion

        #region 消息/加载提示

        #region 消息提示
        /// <summary>
        /// 消息提示
        /// </summary>
        /// <param name="tips"></param>
        /// <param name="title"></param>
        private void ShowTips(MessageType msgType, string title = "提示")
        {
            ShowTips(GetMessage(msgType), title);
        }

        /// <summary>
        /// 消息提示
        /// </summary>
        /// <param name="tips"></param>
        /// <param name="title"></param>
        private void ShowTips(string tips, string title = "提示")
        {
            this.Dispatcher.Invoke(() => 
            {
                MessageBox.Show(tips, title);
            });
        }
        #endregion

        #region 运行中提示（正在加载......）
        /// <summary>
        /// 运行中提示
        /// </summary>
        /// <param name="task"></param>
        /// <param name="tips"></param>
        private void ShowLoading(Task<MessageType> task, MessageType msgType)
        {
            ShowLoading(task, GetMessage(msgType));
        }

        /// <summary>
        /// 运行中提示
        /// </summary>
        /// <param name="task"></param>
        /// <param name="tips"></param>
        private void ShowLoading(Task<MessageType> task, string tips = "正在加载......")
        {
            ShowLoading(task, TimeSpan.Zero, tips);
        }

        /// <summary>
        /// 运行中提示
        /// </summary>
        /// <param name="task">待完成任务</param>
        /// <param name="timeSpan">维持显示的最短时间</param>
        /// <param name="tips">提示内容</param>
        private void ShowLoading(Task<MessageType> task, TimeSpan timeSpan, string tips = "正在加载......")
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();//开始计时
            LoadingWindow window = new LoadingWindow(tips);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            task.ContinueWith(completedTask =>
            {
                MessageType msgType;
                //string msg = null;
                //if (completedTask.Status == TaskStatus.RanToCompletion) //已成功完成执行的任务
                //    msg = completedTask.Result;
                //else if (completedTask.Status == TaskStatus.Faulted) //由于未处理异常的原因而完成的任务
                //    msg = $"程序错误：{completedTask.Exception?.InnerException?.Message}";
                try
                {
                    msgType = completedTask.Result;
                }
                catch (AggregateException ae)
                {
                    msgType = MessageType.UnknownError;
                    ErrorLogAsync(ae);
                }
                catch (Exception ex)
                {
                    msgType = MessageType.UnknownError;
                    ErrorLogAsync(ex);
                }
                stopwatch.Stop();//停止计时
                TimeSpan ts = stopwatch.Elapsed;
                //大于 TimeSpan.Zero 并且 大于 任务耗时
                if (timeSpan.CompareTo(TimeSpan.Zero) > 0 && timeSpan.CompareTo(ts) > 0)
                    Thread.Sleep(timeSpan - ts);
                this.Dispatcher.Invoke(() =>
                {
                    //下载成功
                    if (msgType == MessageType.DownloadSuccess)
                    {
                        window.Close();
                        OpenFileWindow openwindow = new OpenFileWindow(GetMessage(msgType));
                        openwindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        openwindow.Owner = this;
                        if (openwindow.ShowDialog() == true)
                        {
                            //打开下载文件所在文件夹
                            ProcessStartInfo startInfo = new ProcessStartInfo(Path.GetDirectoryName(ws.SaveFullPath));
                            startInfo.UseShellExecute = true;//是否应在启动进程时使用 Windows shell（默认 .NET Framework-True，.Net Core-False）
                            Process.Start(startInfo);
                        }
                    }
                    else
                    {
                        string msg = GetMessage(msgType);
                        window.Close();
                        if (!string.IsNullOrWhiteSpace(msg))
                            MessageBox.Show(msg, "提示");
                    }
                });
            });
            window.ShowDialog();
        }
        #endregion

        #region 根据消息类型获取对应消息
        /// <summary>
        /// 根据消息类型获取对应消息
        /// </summary>
        /// <param name="msgType"></param>
        /// <returns></returns>
        private string GetMessage(MessageType msgType)
        {
            if (messageDic != null && messageDic.Count > 0 && messageDic.Keys.Contains(msgType))
                return messageDic[msgType];
            else
                return null;
        }
        #endregion

        #endregion

        #region 读取配置文件 AppSettings
        /// <summary>
        /// 读取配置文件 AppSettings
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetAppSettings(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key].ToString();
            }
            catch
            {
                MessageBox.Show("配置文件缺失！将关闭程序！");
                this.Close();
                return string.Empty;
            }
        }
        #endregion

        #region 获取程序所在目录
        /// <summary>
        /// 获取程序所在目录
        /// </summary>
        /// <returns></returns>
        private string GetDirectory()
        {
            //获取当前目录（即该进程从中启动的目录）的完全限定路径。 X:\xxx\xxx (.exe文件所在的目录)
            return Environment.CurrentDirectory;
        }
        #endregion

        #region 判断是否联网
        /// <summary>
        /// 判断是否联网（false：连接超时，请检查您的网络或者本机防火墙设置！）
        /// </summary>
        /// <returns></returns>
        private async Task<bool> isConnAsync()
        {
            System.Net.NetworkInformation.Ping ping;
            System.Net.NetworkInformation.PingReply res;
            ping = new System.Net.NetworkInformation.Ping();
            try
            {
                res = await ping.SendPingAsync("www.baidu.com");
                if (res.Status != System.Net.NetworkInformation.IPStatus.Success)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }



        #endregion

        #region 错误日志记录
        /// <summary>
        /// 错误日志记录
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private async Task ErrorLogAsync(Exception ex)
        {
            //如果该文件不存在，将创建一个新文件；如果该文件存在，则追加
            using (StreamWriter sw = new StreamWriter(Path.Combine(GetDirectory(), "ErrorLog.txt"), true, Encoding.UTF8))
            {
                if (ex is AggregateException)
                {
                    AggregateException ae = ex as AggregateException;
                    Debug.WriteLine("一个或者多个错误：");
                    await sw.WriteLineAsync($"【{DateTime.Now:yyyy-MM-dd HH:mm:ss}】一个或者多个错误：");
                    foreach (var item in ae.InnerExceptions)
                    {
                        Debug.WriteLine($"  {item.GetType().Name}：{item.Message}");
                        Debug.WriteLine($"      {item.StackTrace}");
                        await sw.WriteLineAsync($"  {item.GetType().Name}：{item.Message}");
                        await sw.WriteLineAsync($"      {item.StackTrace}");
                    }
                }
                else
                {
                    Debug.WriteLine($"错误：{ex.Message}");
                    await sw.WriteLineAsync($"【{DateTime.Now:yyyy-MM-dd HH:mm:ss}】{ex.GetType().Name}：{ex.Message}");
                    await sw.WriteLineAsync($"  {ex.StackTrace}");
                }
            }
        }
        #endregion
    }
}
