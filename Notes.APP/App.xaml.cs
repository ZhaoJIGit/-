﻿using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Notes.APP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // 引入必要的 Windows API 函数
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(int processId);
        [DllImport("user32.dll")]
        private static extern bool SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        private const int WM_COPYDATA = 0x004A;

        private const int SW_RESTORE = 9;
        private static Mutex _mutex;
        private static ListWindow _listWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "MyUniqueWpfAppMutexName"; // 请确保这个名称全局唯一
            bool createdNew;

            // 尝试创建一个全局 Mutex
            _mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // 发送消息通知已存在的实例
                NotifyExistingInstance();
                //ActivateOtherInstance();
                Shutdown();
            }
            base.OnStartup(e);

           
        }
        [DllImport("user32.dll")]
        private static extern bool SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        private void NotifyExistingInstance()
        {
            IntPtr hWnd = FindWindow(null, "便利贴"); // 确保标题匹配
            if (hWnd != IntPtr.Zero)
            {
                string message = "SHOW";
                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
                IntPtr lpData = Marshal.AllocHGlobal(messageBytes.Length);
                Marshal.Copy(messageBytes, 0, lpData, messageBytes.Length);

                COPYDATASTRUCT cds = new COPYDATASTRUCT
                {
                    dwData = IntPtr.Zero,
                    cbData = messageBytes.Length,
                    lpData = lpData
                };

                SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref cds);
                Marshal.FreeHGlobal(lpData);
            }
        }
        /// <summary>
        /// 激活已存在的实例窗口
        /// </summary>
        private void ActivateOtherInstance()
        {
            // 这里的窗口标题必须与主窗口的 Title 保持一致
            const string windowTitle = "便利贴";
            IntPtr hWnd = FindWindow(null, windowTitle);

            if (hWnd != IntPtr.Zero)
            {
                Process currentProcess = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);

                foreach (Process process in processes)
                {
                    if (process.Id != currentProcess.Id)
                    {
                        // 允许目标进程获得前台权限
                        AllowSetForegroundWindow(process.Id);
                        break;
                    }
                }

                if (IsIconic(hWnd))
                {
                    ShowWindowAsync(hWnd, SW_RESTORE); // 还原窗口
                }
                SetForegroundWindow(hWnd); // 置前窗口
            }
        }
    }

}
