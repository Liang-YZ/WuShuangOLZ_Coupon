using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Core.Common
{
    public class IntStringConverter : IValueConverter
    {
        /// <summary>
        /// Int To String
        /// 将值从“绑定源”传播到“绑定目标”时会调用此方法
        /// </summary>
        /// <param name="value">绑定源生成的值</param>
        /// <param name="targetType">绑定目标属性的类型</param>
        /// <param name="parameter">要使用的转换器参数</param>
        /// <param name="culture">要用在转换器中的区域性</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        /// <summary>
        /// String To Int
        /// 将值从“绑定目标”传播到“绑定源”时调用此方法
        /// </summary>
        /// <param name="value">文本值</param>
        /// <param name="targetType">要转换为的类型</param>
        /// <param name="parameter">要使用的转换器参数</param>
        /// <param name="culture">要用在转换器中的区域性</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString())) 
                return int.Parse(Regex.Replace(value.ToString().Trim(), "\\D+", ""));
            return 0;
        }
    }
}
