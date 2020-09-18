using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Core
{
    /// <summary>
    /// MessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OpenFileWindow : Window
    {
        /// <summary>
        /// 保存“是否打开文件夹”选项状态
        /// </summary>
        private static bool openFolder = true;

        private OpenFileModel model = new OpenFileModel() { IsOpenFolder = openFolder };

        public OpenFileWindow()
        {
            InitializeComponent();
        }

        public OpenFileWindow(string content)
        {
            InitializeComponent();

            model.Content = content;
        }

        public OpenFileWindow(string content, string title)
        {
            InitializeComponent();

            model.Content = content;
            model.Title = title;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = model;
            //this.UpdateLayout();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = openFolder = model.IsOpenFolder;
            this.Close();
        }
    }
}
