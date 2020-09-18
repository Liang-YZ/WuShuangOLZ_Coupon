using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.ViewModels
{
    public class WuShuang : INotifyPropertyChanged
    {
        #region 属性更改通知
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 属性更改通知
        /// </summary>
        /// <param name="propertyName">属性名</param>
        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string username;

        private string password;

        private bool isShowRealValue;

        private string validateCode;

        private string propIndex = string.Empty;

        private string status = string.Empty;

        private int fileType = 0;

        private bool isSaveToRoot = true;

        private int pageStart;

        private int pageEnd;

        private int pageTotal;

        private bool isLogin;

        
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName 
        {
            get { return username; }
            set
            {
                username = value;
                NotifyPropertyChanged("UserName");
            }
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get { return password; }

            set
            {
                password = value;
                NotifyPropertyChanged("Password");
            }
        }

        /// <summary>
        /// 是否显示真实密码值（默认值 false）
        /// </summary>
        public bool IsShowRealValue
        {
            get { return isShowRealValue; }

            set
            {
                isShowRealValue = value;
                NotifyPropertyChanged("IsShowRealValue");
            }
        }

        /// <summary>
        /// 验证码
        /// </summary>
        public string ValidateCode
        {
            get { return validateCode; }
            set
            {
                validateCode = value;
                NotifyPropertyChanged("ValidateCode");
            }
        }

        /// <summary>
        /// 道具索引
        /// </summary>
        public string PropIndex
        {
            get { return propIndex; }
            set
            {
                propIndex = value;
                NotifyPropertyChanged("PropIndex");
            }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                NotifyPropertyChanged("Status");
            }
        }

        /// <summary>
        /// 文件类型（0-excel 1-txt 默认 0）
        /// </summary>
        public int FileType
        {
            get { return fileType; }

            set
            {
                fileType = value;
                NotifyPropertyChanged("FileType");
            }
        }

        /// <summary>
        /// 保存到根目录（默认值为 true）
        /// </summary>
        public bool IsSaveToRoot
        {
            get { return isSaveToRoot; }
            set
            {
                isSaveToRoot = value;
                NotifyPropertyChanged("IsSaveToRoot");
            }
        }

        /// <summary>
        /// 起始页码
        /// </summary>
        public int PageStart
        {
            get { return pageStart; }

            set
            {
                pageStart = value > pageEnd ? pageEnd : value;
                NotifyPropertyChanged("PageStart");
            }
        }

        /// <summary>
        /// 结束页码
        /// </summary>
        public int PageEnd
        {
            get { return pageEnd; }

            set
            {
                pageEnd = value > pageTotal ? pageTotal : value;
                NotifyPropertyChanged("PageEnd");
            }
        }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageTotal
        {
            get { return pageTotal; }

            set
            {
                pageTotal = value;
                NotifyPropertyChanged("PageTotal");
            }
        }

        /// <summary>
        /// 是否已登录（默认值 false）
        /// </summary>
        public bool IsLogin 
        {
            get{ return isLogin; }
            
            set
            {
                isLogin = value;
                NotifyPropertyChanged("IsLogin");
                NotifyPropertyChanged("IsLogout");
            }
         }

        /// <summary>
        /// 是否已退出（默认值 true）
        /// </summary>
        public bool IsLogout
        {
            get { return !isLogin; }
        }

        /// <summary>
        /// 页信息
        /// </summary>
        public PageInfo PageInfo { get; set; }

        /// <summary>
        /// 保存文件完整路径
        /// </summary>
        public string SaveFullPath { get; set; }
    }
}
