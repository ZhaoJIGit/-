﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TaskMGPro.Common
{
    /// <summary>
    /// 申明page后，页面中要引入自定义page
    /// 用于加载页面配置文件等等信息
    /// 共有处理方式
    /// </summary>
    public class BasePage : Page
    {
        public string MyName="zs";
        public string BackgroundColor = "#ffffff";
        public string ForegroundColor = "#000000";
        public BasePage()
        {
            MyName = "zj";
        }
    }
}