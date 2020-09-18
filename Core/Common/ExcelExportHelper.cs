using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    /// <summary>
    /// Excel导出帮助类
    /// </summary>
    public class ExcelExportHelper
    {
        #region DataTable 导出 Excel

        #region 同步导出
        /// <summary>
        /// 导出 Excel
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="fileFullPath">Excel 文件保存完整路径</param>
        /// <returns></returns>
        public static void ExportExcel(DataTable dt, string fileFullPath)
        {
            ExportExcel(dt, fileFullPath, "");
        }

        /// <summary>
        /// 导出 Excel
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="fileFullPath">Excel 文件保存完整路径</param>
        /// <param name="heading">工作表名称/标题</param>
        /// <returns></returns>
        public static void ExportExcel(DataTable dt, string fileFullPath, string heading)
        {
            ExportExcel(dt, fileFullPath, heading, null);
        }

        /// <summary>
        /// 导出 Excel
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="fileFullPath">Excel 文件保存完整路径</param>
        /// <param name="heading">工作表名称/标题</param>
        /// <param name="columnsToTake">要导出的列</param>
        /// <returns></returns>
        public static void ExportExcel(DataTable dt, string fileFullPath, string heading = "", params string[] columnsToTake)
        {
            //Polyform Noncommercial license（非商业许可协议）
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add($"{heading}Data");
                int startRowFrom = string.IsNullOrWhiteSpace(heading) ? 1 : 2;//开始的行，序号从 1 开始

                //移除无关列
                if (columnsToTake != null && columnsToTake.Length > 0)
                {
                    List<string> ignoreColName = new List<string>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (!columnsToTake.Contains(col.ColumnName))
                            ignoreColName.Add(col.ColumnName);
                    }
                    foreach(string colName in ignoreColName)
                    {
                        dt.Columns.Remove(colName);
                    }
                }
                int colCount = dt.Columns.Count;
                //读取数据到工作表
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dt, true);// A1-单元格 1,1  B2-单元格 2,2

                //格式化日期
                int colIndex = 0;
                while(colIndex < colCount)
                {
                    if(dt.Columns[colIndex].DataType.Equals(typeof(DateTime)))
                        workSheet.Column(colIndex + 1).Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                    colIndex++;
                }

                if (!string.IsNullOrEmpty(heading))
                {
                    //标题行样式
                    //合并单元格（跨行跨列） worksheet.Cells[int fromRow, int fromCol, int toRow,int toCol].Merge = true;
                    workSheet.Cells[1, 1, 1, colCount].Merge = true;
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Name = "微软雅黑";//字体
                    workSheet.Cells["A1"].Style.Font.Bold = true;//粗体
                    workSheet.Cells["A1"].Style.Font.Size = 20;//字体大小
                    //workSheet.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.Black);//颜色
                    workSheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//居中对齐

                }
                //列头行样式 
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, colCount])
                {
                    r.Style.Font.Name = "微软雅黑";//字体
                    r.Style.Font.Bold = true;//粗体
                    r.Style.Font.Size = 12;//字体大小
                    //r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                    
                }
                //列单元格样式
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dt.Rows.Count, colCount])
                {
                    r.Style.Font.Name = "微软雅黑";//字体
                    r.Style.Font.Size = 10;//字体大小
                    //设置边框：
                    //r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    //r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    //r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    //r.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    //r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    //r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    //r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    //r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }

                //自动适应列
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

                //保存到文件
                package.SaveAs(new System.IO.FileInfo(fileFullPath));
            }
        }
        #endregion

        #region 异步导出
        /// <summary>
        /// 导出 Excel
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="fileFullPath">Excel 文件保存完整路径</param>
        /// <returns></returns>
        public static Task ExportExcelAsync(DataTable dt, string fileFullPath)
        {
            return ExportExcelAsync(dt, fileFullPath, "");
        }

        /// <summary>
        /// 导出 Excel
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="fileFullPath">Excel 文件保存完整路径</param>
        /// <param name="heading">工作表名称/标题</param>
        /// <returns></returns>
        public static Task ExportExcelAsync(DataTable dt, string fileFullPath, string heading)
        {
            return ExportExcelAsync(dt, fileFullPath, heading, null);
        }

        /// <summary>
        /// 导出 Excel
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="fileFullPath">Excel 文件保存完整路径</param>
        /// <param name="heading">工作表名称/标题</param>
        /// <param name="columnsToTake">要导出的列</param>
        /// <returns></returns>
        public static async Task ExportExcelAsync(DataTable dt, string fileFullPath, string heading = "", params string[] columnsToTake)
        {
            //Polyform Noncommercial license（非商业许可协议）
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add($"{heading}Data");
                int startRowFrom = string.IsNullOrWhiteSpace(heading) ? 1 : 2;//开始的行，序号从 1 开始

                //移除无关列
                if (columnsToTake != null && columnsToTake.Length > 0)
                {
                    List<string> ignoreColName = new List<string>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (!columnsToTake.Contains(col.ColumnName))
                            ignoreColName.Add(col.ColumnName);
                    }
                    foreach (string colName in ignoreColName)
                    {
                        dt.Columns.Remove(colName);
                    }
                }
                int colCount = dt.Columns.Count;
                //读取数据到工作表
                //workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dt, true);// A1-单元格 1,1  B2-单元格 2,2
                await workSheet.Cells["A" + startRowFrom].LoadFromDataReaderAsync(dt.CreateDataReader(), true);

                //格式化日期
                int colIndex = 0;
                while (colIndex < colCount)
                {
                    if (dt.Columns[colIndex].DataType.Equals(typeof(DateTime)))
                        workSheet.Column(colIndex + 1).Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                    colIndex++;
                }

                if (!string.IsNullOrEmpty(heading))
                {
                    //标题行样式
                    //合并单元格（跨行跨列） worksheet.Cells[int fromRow, int fromCol, int toRow,int toCol].Merge = true;
                    workSheet.Cells[1, 1, 1, colCount].Merge = true;
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Name = "微软雅黑";//字体
                    workSheet.Cells["A1"].Style.Font.Bold = true;//粗体
                    workSheet.Cells["A1"].Style.Font.Size = 20;//字体大小
                    //workSheet.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.Black);//颜色
                    workSheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//居中对齐

                }
                //列头行样式 
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, colCount])
                {
                    r.Style.Font.Name = "微软雅黑";//字体
                    r.Style.Font.Bold = true;//粗体
                    r.Style.Font.Size = 12;//字体大小
                                           //r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                           //r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));

                }
                //列单元格样式
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dt.Rows.Count, colCount])
                {
                    r.Style.Font.Name = "微软雅黑";//字体
                    r.Style.Font.Size = 10;//字体大小
                    //设置边框：
                    //r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    //r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    //r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    //r.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    //r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    //r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    //r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    //r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }

                //自动适应列
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

                //保存到文件
                await package.SaveAsAsync(new System.IO.FileInfo(fileFullPath));
            }
        }
        #endregion

        #endregion

        #region List<T> 导出到 Excel
        /// <summary>
        /// 导出 Excel
        /// </summary>
        /// <param name="data">数据源</param>
        /// <param name="fileFullPath">Excel 文件保存完整路径</param>
        /// <returns></returns>
        public static void ExportExcel<T>(List<T> data, string fileFullPath)
        {
            ExportExcel(data, fileFullPath, "");
        }

        /// <summary>
        /// 导出 Excel
        /// </summary>
        /// <param name="data">数据源</param>
        /// <param name="fileFullPath">Excel 文件保存完整路径</param>
        /// <param name="heading">工作表名称/标题</param>
        /// <returns></returns>
        public static void ExportExcel<T>(List<T> data, string fileFullPath, string heading)
        {
            ExportExcel(data, fileFullPath, heading, null);
        }

        /// <summary>
        /// 导出 Excel
        /// </summary>
        /// <param name="data">数据源</param>
        /// <param name="fileFullPath">Excel 文件保存完整路径</param>
        /// <param name="heading">工作表名称/标题</param>=
        /// <param name="columnsToTake">要导出的列</param>
        /// <returns></returns>
        public static void ExportExcel<T>(List<T> data, string fileFullPath, string heading = "", params string[] columnsToTake)
        {
            ExportExcel(ListToDataTable(data), fileFullPath, heading, columnsToTake);
        }
        #endregion

        #region List转DataTable
        /// <summary>
        /// List转DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private static DataTable ListToDataTable<T>(List<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable dataTable = new DataTable();
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor property = properties[i];
                dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }
            object[] values = new object[properties.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = properties[i].GetValue(item);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
        #endregion
    }
}
