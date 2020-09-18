using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        None,

        /// <summary>
        /// 正在获取验证码
        /// </summary>
        GettingVerificationCode,

        /// <summary>
        /// 验证码为空
        /// </summary>
        VerificationCodeEmpty,

        /// <summary>
        /// 验证码不正确
        /// </summary>
        VerificationCodeIncorrect,

        /// <summary>
        /// 验证码已失效
        /// </summary>
        VerificationCodeExpired,

        /// <summary>
        /// 用户名为空
        /// </summary>
        UserNameEmpty,

        /// <summary>
        /// 用户名不存在
        /// </summary>
        UserNameNonExistent,

        /// <summary>
        /// 密码为空
        /// </summary>
        PasswordEmpty,

        /// <summary>
        /// 密码不正确
        /// </summary>
        PasswordIncorrect,

        /// <summary>
        /// 单点登录异常
        /// </summary>
        SSOError,

        /// <summary>
        /// 登录中
        /// </summary>
        LoggingIn,

        /// <summary>
        /// 退出中
        /// </summary>
        LoggingOut,

        /// <summary>
        /// 登录成功
        /// </summary>
        LoginSuccess,

        /// <summary>
        /// 登录失败
        /// </summary>
        LoginFailed,

        /// <summary>
        /// 登录已失效
        /// </summary>
        LoginExpired,

        /// <summary>
        /// 正在获取道具箱信息
        /// </summary>
        GettingPageInfo,

        /// <summary>
        /// 起始页码大于结束页码
        /// </summary>
        PageStartGreaterThanPageEnd,

        /// <summary>
        /// 结束页码大于总页数
        /// </summary>
        PageEndGreaterThanPageTotal,

        /// <summary>
        /// 下载中
        /// </summary>
        Doanloading,

        /// <summary>
        /// 下载成功
        /// </summary>
        DownloadSuccess,

        /// <summary>
        /// 无任何数据
        /// </summary>
        NoData,

        /// <summary>
        /// 已取消保存
        /// </summary>
        SaveCanceled,

        /// <summary>
        /// HttpRequest 错误
        /// </summary>
        HttpRequestError,

        /// <summary>
        /// 未知错误
        /// </summary>
        UnknownError
    }
}
