using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Core.Common
{
    /// <summary>
    /// Int-Bool 数据绑定转换
    /// </summary>
    public class IntToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Int To Bool
        /// 将值从“绑定源”传播到“绑定目标”时会调用此方法
        /// </summary>
        /// <param name="value">绑定源生成的值</param>
        /// <param name="targetType">绑定目标属性的类型</param>
        /// <param name="parameter">要使用的转换器参数</param>
        /// <param name="culture">要用在转换器中的区域性</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return value == null ? false : value.Equals(parameter);
            return value == null ? false : value.Equals(int.Parse(parameter.ToString()));
        }

        /// <summary>
        /// Bool To Int
        /// 将值从“绑定目标”传播到“绑定源”时调用此方法
        /// </summary>
        /// <param name="value">绑定目标生成的值</param>
        /// <param name="targetType">要转换为的类型</param>
        /// <param name="parameter">要使用的转换器参数</param>
        /// <param name="culture">要用在转换器中的区域性</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return value != null && value.Equals(true) ? parameter : Binding.DoNothing;
            return value != null && value.Equals(true) ? int.Parse(parameter.ToString()) : Binding.DoNothing;
        }
    }
}
