using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Core.Common
{
    /// <summary>
    /// 密码显示字符转换
    /// </summary>
    public class PassWordConverter : IValueConverter
    {
        /// <summary>
        /// 是否显示真实值（默认值 false）
        /// </summary>
        public bool IsShowRealValue { get; set; }

        private string password = string.Empty;
        /// <summary>
        /// 密码（真实值）
        /// </summary>
        //public string Password { get; set; }

        /// <summary>
        /// 掩码字符（默认值“●”）
        /// </summary>
        public char PasswordChar { get; set; } = '●';

        /// <summary>
        /// 真实值 转换到 掩码字符
        /// 将值从“绑定源”传播到“绑定目标”时会调用此方法
        /// </summary>
        /// <param name="value">密码真实值</param>
        /// <param name="targetType">绑定目标属性的类型</param>
        /// <param name="parameter">要使用的转换器参数</param>
        /// <param name="culture">要用在转换器中的区域性</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null) password = value.ToString();
            //Debug.WriteLine($"password={password}");
            return IsShowRealValue ? password : string.Empty.PadRight(password.Length, PasswordChar);
        }

        /// <summary>
        /// 掩码字符 转换到 真实值
        /// 将值从“绑定目标”传播到“绑定源”时调用此方法
        /// </summary>
        /// <param name="value">文本值</param>
        /// <param name="targetType">要转换为的类型</param>
        /// <param name="parameter">要使用的转换器参数</param>
        /// <param name="culture">要用在转换器中的区域性</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            if(value != null)
            {
                string strValue = value.ToString();
                //Debug.WriteLine($"value={strValue}; password={password}");
                for (int index = 0; index < strValue.Length; index++)
                {
                    if (strValue.ElementAt(index) == PasswordChar)
                    {
                        result += password.ElementAt(index);
                    }
                    else
                    {
                        password = password.Insert(index, strValue.ElementAt(index).ToString());
                        result += strValue.ElementAt(index);
                    }
                }
            }
            return result;
        }
    }
}
