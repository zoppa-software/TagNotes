using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TagNotes.Helper
{
    /// <summary>ヘルパー機能です（表示関連）</summary>
    internal static partial class WindowHelper
    {
        /// <summary>指定されたウィンドウの DPI を取得します。</summary>
        /// <param name="hwnd">ウィンドウハンドル。</param>
        /// <returns>DPI値。</returns>
        [LibraryImport("User32.dll")]
        private static partial int GetDpiForWindow(nint hwnd);

        /// <summary>指定したウィンドウの表示状態を設定します。</summary>
        /// <param name="hWnd">ウィンドウハンドル。</param>
        /// <param name="nCmdShow">表示方法制御値。</param>
        /// <returns>表示状態。</returns>
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>指定したウィンドウを作成したスレッドをフォアグラウンドに移動します。</summary>
        /// <param name="hWnd">ウィンドウハンドル。</param>
        /// <returns>ウィンドウが前景に移動された場合、0 以外。</returns>
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetForegroundWindow(IntPtr hWnd);

        [LibraryImport("kernel32.dll", EntryPoint = "CreateEventW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr CreateEvent(
            IntPtr lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset,
            [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string lpName);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetEvent(IntPtr hEvent);

        [LibraryImport("ole32.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        private static partial uint CoWaitForMultipleObjects(
            uint dwFlags, uint dwMilliseconds, ulong nHandles,
            IntPtr[] pHandles, out uint dwIndex);

        /// <summary>デフォルトの DPI 値。</summary>
        public const double DefaultPixelsPerInch = 96D;

        private static IntPtr redirectEventHandle = IntPtr.Zero;

        public static bool DecideRedirection()
        {
            AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
            AppInstance keyInstance = AppInstance.FindOrRegisterForKey("Zoppa.TagNotes");

            if (keyInstance.IsCurrent) {
                return false;
            }
            else {
                RedirectActivationTo(args, keyInstance);
                return true;
            }
        }

        public static void RedirectActivationTo(AppActivationArguments args,
                                        AppInstance keyInstance)
        {
            redirectEventHandle = CreateEvent(IntPtr.Zero, true, false, null);
            Task.Run(() =>
            {
                keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
                SetEvent(redirectEventHandle);
            });

            uint CWMO_DEFAULT = 0;
            uint INFINITE = 0xFFFFFFFF;
            _ = CoWaitForMultipleObjects(
               CWMO_DEFAULT, INFINITE, 1,
               [redirectEventHandle], out uint handleIndex);

            // Bring the window to the foreground
            Process process = Process.GetProcessById((int)keyInstance.ProcessId);
            SetForegroundWindow(process.MainWindowHandle);
        }

        /// <summary>ウィンドウの DPI スケールを取得します。</summary>
        /// <param name="window">ウィンドウインスタンス。</param>
        /// <returns>DPIスケール。</returns>
        public static double GetDpiScale(this Window window)
        {
            nint windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
            return GetDpiForWindow(windowHandle) / DefaultPixelsPerInch;
        }

        /// <summary>ウィンドウを表示します。</summary>
        /// <param name="window">ウィンドウインスタンス。</param>
        public static void ShowWindow(Window window)
        {
            // ウィンドウハンドルを取得
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // 最小化されたウィンドウを復元し、前景に移動
            ShowWindow(hwnd, 0x00000009);
            SetForegroundWindow(hwnd);
        }
    }
}
