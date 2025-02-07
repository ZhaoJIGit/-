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
        // 定义静态事件
        public static event EventHandler ReloadWindow;
        private Point _mouseDownPosition;
        private bool _isDrawerOpen = false;
        private NoteModel _noteModel;
        private MyMessage myMessage;

        public MainWindow(NoteModel noteModel)
        {
            InitializeComponent();
            _noteModel = noteModel;
            this.DataContext = _noteModel;
            // 创建并初始化 MessagePopupHelper
            MessagePopupHelper popupHelper = new MessagePopupHelper(this);

            // 创建 MyMessage 实例并传入 MessagePopupHelper
            myMessage = new MyMessage(popupHelper);

            // 默认显示 Page1
            MainFrame.Navigate(new HomePage());
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置 DataContext
            var service = new NoteService();
            //_noteModel = service.SelectNote("b2a642c94a654175b455cb2337b1012d");
            if (_noteModel == null)
            {
                MessageBox.Show("便签不存在！");
                return;
            }
            if (_noteModel.Fixed)
            {
                
                btnFixed.Content = "\uE840";
            }
            else
            {
                btnFixed.Content = "\uE718";
            }
            this.Topmost = _noteModel.Fixed;
            //isTopUpBox.IsChecked = _noteModel.IsTopUp;

            pageBorder.Background = _noteModel.PageBackgroundColor.ToSolidColorBrush();
            MyColorPicker.SelectedColor = _noteModel.BackgroundColor.ToColor();
            this.Width = _noteModel.Width;
            this.Height = _noteModel.Height;
            if (_noteModel.XAxis > 0 || _noteModel.YAxis > 0)
            {
                this.Left = _noteModel.XAxis;
                this.Top = _noteModel.YAxis;
            }
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
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    _mouseDownPosition = e.GetPosition(this);
                }
            }
            e.Handled = true;
        }
     
        private void TextButton_Click(object sender, MouseButtonEventArgs e)
        {
            // 创建便签窗口实例
            var note = NoteModel.CreateNote();
            MainWindow stickyNoteWindow = new MainWindow(note);
            stickyNoteWindow.Tag = note.NoteId;
            // 打开便签窗口
            stickyNoteWindow.Show();

            // 触发事件，通知 Window A
            ReloadWindow?.Invoke(this, EventArgs.Empty);
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
        private void SaveNote()
        {
            var service = new NoteService();
            if (service.SaveNote(_noteModel))
            {
                Console.WriteLine("保存成功");
                // 触发事件，通知 Window A
                ReloadWindow?.Invoke(this, EventArgs.Empty);
            }
        }
        public void ChangedTextEvent() {
                ReloadWindow?.Invoke(this, EventArgs.Empty);
        }
        private void ResizeHandle_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // 获取当前窗口
            var window = this;

            // 调整窗口的宽度和高度
            window.Width = Math.Max(window.MinWidth, window.Width + e.HorizontalChange);
            window.Height = Math.Max(window.MinHeight, window.Height + e.VerticalChange);
            _noteModel.Height = window.Height;
            _noteModel.Width = window.Width;
            SaveNote();
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    // 获取当前鼠标位置
                    Point currentPosition = e.GetPosition(this);

                    // 判断鼠标移动距离（避免误触）
                    if (Math.Abs(currentPosition.X - _mouseDownPosition.X) > 10 ||
                        Math.Abs(currentPosition.Y - _mouseDownPosition.Y) > 10)
                    {
                        // 退出全屏，并调整窗口位置
                        if (this.WindowState == WindowState.Maximized)
                        {
                            this.WindowState = WindowState.Normal;

                            // 计算鼠标相对位置，保持窗口位置不跳变
                            this.Top = Math.Max(0, currentPosition.Y - 25);
                            this.Left = Math.Max(0, currentPosition.X - (this.Width / 2));
                        }
                        // 执行拖拽
                        this.DragMove();
                    }
                }
                else
                {
                    // 执行拖拽
                    this.DragMove();
                }
                // 获取当前窗口位置
                _noteModel.XAxis = this.Left;
                _noteModel.YAxis = this.Top;
                SaveNote();
            }
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
                _noteModel.Color = ColorHelper.GetColorByBackground(_noteModel.BackgroundColor);
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
            //TrayIcon.Dispose(); // 清理托盘图标资源
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
            var service = new NoteService();
            service.DeleteNote(_noteModel!.NoteId);
            this.Close();
            // 触发事件，通知 Window A
            ReloadWindow?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// 查看列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            var windows = Application.Current.Windows.OfType<Window>().FirstOrDefault(i => i.Name == "listWindow");
            if (windows != null)
            {
                windows.Show();
                windows.WindowState = WindowState.Normal;
                windows.Activate();
            }
        }

        private void Fix_Click(object sender, RoutedEventArgs e)
        {
            _noteModel.Fixed = !_noteModel.Fixed;
            if (_noteModel.Fixed)
            {
                btnFixed.Content = "\uE840";
            }
            else
            {
              
                btnFixed.Content = "\uE718";
            }
            this.Topmost = _noteModel.Fixed;
            SaveNote();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.D) &&
         ((Keyboard.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows))
            {
                // 如果按下的是 Win + D，取消默认处理
                e.Handled = true;
            }
        }

        //private void TopUp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    isTopUpBox.IsChecked = !isTopUpBox.IsChecked;
        //}

        //private void TopUpBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (isTopUpBox.IsChecked.Value)
        //    {
        //        this.Topmost = true;
        //        _noteModel.IsTopUp = true;
        //        SaveNote();
        //    }
        //}

        //private void TopUpBox_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (!isTopUpBox.IsChecked.Value)
        //    {
        //        this.Topmost = false;
        //        _noteModel.IsTopUp = false;
        //        SaveNote();
        //    }
        //}
    }
}