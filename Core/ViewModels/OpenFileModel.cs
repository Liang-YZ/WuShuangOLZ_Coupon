using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.ViewModels
{
    public class OpenFileModel : INotifyPropertyChanged
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

        private bool isOpenFolder = true;

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; } = "下载成功！";

        /// <summary>
        /// 标题（默认值 “提示”）
        /// </summary>
        public string Title { get; set; } = "提示";

        /// <summary>
        /// 是否打开文件夹（默认 true）
        /// </summary>
        public bool IsOpenFolder 
        { 
            get { return isOpenFolder; }
            
            set 
            {
                isOpenFolder = value;
                NotifyPropertyChanged("IsOpenFolder");
            }
        }
    }
}
