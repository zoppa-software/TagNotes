using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;

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

        /// <summary>デフォルトの DPI 値。</summary>
        public const double DefaultPixelsPerInch = 96D;

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
