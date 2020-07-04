using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Prism.Mvvm;
using PositionMonitor.Models;

namespace PositionMonitor.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _Title = "position (???,???)";
        public string Title
        {
            get { return _Title; }
            set { SetProperty(ref _Title, value); }
        }

        // デリゲート
        private readonly NativeMethods.LowLevelMouseKeyboardProc _mouseProc;

        // メソッドを識別するID
        private static IntPtr _mouseHookId = IntPtr.Zero;

        public MainWindowViewModel()
        {
            _mouseProc = MouseInputCallback;
            using (Process currentProcess = Process.GetCurrentProcess())
            using (ProcessModule currentModule = currentProcess.MainModule)
            {
                // メソッドをマウスのイベントに紐づける。
                _mouseHookId = NativeMethods.SetWindowsHookEx(
                    (int)NativeMethods.HookType.WH_MOUSE_LL,
                    _mouseProc,
                    NativeMethods.GetModuleHandle(currentModule.ModuleName),
                    0
                );
            }
        }

        ~MainWindowViewModel()
        {
            NativeMethods.UnhookWindowsHookEx(_mouseHookId);
        }

        // マウス操作のイベントが発生したら実行されるメソッド
        private IntPtr MouseInputCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                // マウスのイベントに紐付けられた次のメソッドを実行する。メソッドがなければ処理終了。
                return NativeMethods.CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
            }

            Win32Point mousePosition = new Win32Point
            {
                X = 0,
                Y = 0
            };

            // マウスのどのようなイベントが発生したのかで処理を分岐する。
            switch ((NativeMethods.MouseMessage)wParam)
            {
                case NativeMethods.MouseMessage.WM_MOUSEMOVE:
                    // 現在のマウスポインタの位置を取得する。以降の処理で使いたい場合は使う。
                    NativeMethods.GetCursorPos(ref mousePosition);
                    Title = $"position ({mousePosition.X},{mousePosition.Y})";
                    break;
                default:
                    // 何もしない。
                    break;
            }

            // マウスのイベントに紐付けられた次のメソッドを実行する。メソッドがなければ処理終了。
            return NativeMethods.CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }
    }
}
