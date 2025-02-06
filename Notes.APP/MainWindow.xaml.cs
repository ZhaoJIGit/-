﻿using Notes.APP.Common;
using Notes.APP.Models;
using Notes.APP.Pages;
using Notes.APP.Services;
using System.ComponentModel;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows; 
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Notes.APP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isDrawerOpen = false;
        private NoteModel _noteModel;
        private MyMessage myMessage;

        public MainWindow()
        {
            InitializeComponent();
            // 创建并初始化 MessagePopupHelper
            MessagePopupHelper popupHelper = new MessagePopupHelper(this);

            // 创建 MyMessage 实例并传入 MessagePopupHelper
            myMessage = new MyMessage(popupHelper);
            // 设置 DataContext
            var service = new NoteService();
            _noteModel = service.SelectNote(1);
            if (_noteModel==null) {
                _noteModel = NoteModel.CreateNote();
                service.AddNote(_noteModel);
            }
            this.DataContext = _noteModel;
            pageBorder.Background = _noteModel.PageBackgroundColor.ToSolidColorBrush(); 
            MyColorPicker.SelectedColor = _noteModel.BackgroundColor.ToColor();
            // 默认显示 Page1
            MainFrame.Navigate(new HomePage());
             

        }
        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // 最小化窗口
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide(); // 隐藏窗口
            //this.WindowState = WindowState.Minimized;
        }

        // 最大化或还原窗口
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                ((Button)sender).Content = "🗗"; // 还原图标
            }
            else
            {
                this.WindowState = WindowState.Normal;
                ((Button)sender).Content = "□"; // 最大化图标
            }
        }

        // 关闭窗口
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Hide(); // 隐藏窗口
            this.Close();
        }

        // 实现窗口拖动
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove(); // 拖动窗口
        }

        private void TextButton_Click(object sender, MouseButtonEventArgs e)
        {
            // 创建便签窗口实例
            MainWindow stickyNoteWindow = new MainWindow();

            // 打开便签窗口
            stickyNoteWindow.Show();
        }

        private void More_Click(object sender, RoutedEventArgs e)
        {
            // 根据当前状态切换抽屉的打开或关闭
            if (_isDrawerOpen)
            {
                // 关闭抽屉
                DrawerPanel.Visibility = Visibility.Collapsed;
                CloseArea.Visibility = Visibility.Collapsed;  // 隐藏遮罩层
            }
            else
            {
                // 打开抽屉
                DrawerPanel.Visibility = Visibility.Visible;
                CloseArea.Visibility = Visibility.Visible;  // 显示遮罩层
            }

            // 切换状态
            _isDrawerOpen = !_isDrawerOpen;
        }

        private void CloseArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 关闭抽屉
            //Storyboard closeStoryboard = (Storyboard)FindResource("CloseDrawer");
            //closeStoryboard.Begin();
            CloseArea.Visibility = Visibility.Collapsed;  // 隐藏遮罩层
            DrawerPanel.Visibility = Visibility.Collapsed;

            _isDrawerOpen = false;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_noteModel != null)
            {
                _noteModel.Opacity = e.NewValue;
                SaveNote();
            }
        }
        private void SaveNote() {
            var service = new NoteService();
            if (service.UpdateNote(_noteModel))
            {
                Console.WriteLine("保存成功");
            }
        }
        private void ResizeHandle_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // 获取当前窗口
            var window = this;

            // 调整窗口的宽度和高度
            window.Width = Math.Max(window.MinWidth, window.Width + e.HorizontalChange);
            window.Height = Math.Max(window.MinHeight, window.Height + e.VerticalChange);
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand; // 设置鼠标指针为十字箭头
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow; // 恢复默认箭头指针
        }

        private void ColorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e != null && e.NewValue != null)
            {
                Color colorWithOpacity = ColorHelper.MakeColorTransparent(e.NewValue.Value, 0.7);
                _noteModel.PageBackgroundColor = colorWithOpacity.ToHexColor();
                _noteModel.BackgroundColor = e.NewValue.Value.ToHexColor();
                _noteModel.Color= ColorHelper.GetColorByBackground(_noteModel.BackgroundColor);
                SaveNote();
            }
        }

        private void OnTrayOpenClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void OnTrayExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide(); // 隐藏窗口
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            TrayIcon.Dispose(); // 清理托盘图标资源
            base.OnClosed(e);
        }

        private void TrayIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        /// <summary>
        /// 查看列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {

        }
    }
}