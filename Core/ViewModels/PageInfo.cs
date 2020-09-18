using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;

namespace Core.ViewModels
{
    /// <summary>
    /// 页信息
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// 总页数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 页参数集合（隐藏域）
        /// </summary>
        public List<KeyValuePair<string, string>> FormDataList { get; set; }
    }
}
