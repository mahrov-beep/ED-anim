namespace _EditorTools.LayoutSwitcherUtility.Scripts {
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;
    public enum EResizeWindow {
        None,
        Fullscreen,
        LeftHalf,
        RightHalf
    }
    
#if UNITY_EDITOR && UNITY_EDITOR_WIN
    public static class EditorWindowResizer {
        const int SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(
                        IntPtr hWnd,
                        IntPtr hWndInsertAfter,
                        int X,
                        int Y,
                        int cx,
                        int cy,
                        uint uFlags);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        public static void ResizeEditorWindow(EResizeWindow resizeOption) {
            if (resizeOption == EResizeWindow.None) {
                return;
            }

            // Получаем дескриптор окна Unity Editor
            IntPtr hWnd = GetUnityEditorWindowHandle();
            if (hWnd == IntPtr.Zero) {
                Debug.LogError("Не удалось найти окно Unity Editor.");
                return;
            }

            // Получаем размер экрана
            int screenWidth  = GetSystemMetrics(0); // SM_CXSCREEN
            int screenHeight = GetSystemMetrics(1); // SM_CYSCREEN

            // В зависимости от опции изменяем размер окна
            switch (resizeOption) {
                case EResizeWindow.Fullscreen:
                    SetWindowPos(hWnd, IntPtr.Zero, 0, 0, screenWidth, screenHeight, SWP_SHOWWINDOW);
                    break;
                case EResizeWindow.LeftHalf:
                    SetWindowPos(hWnd, IntPtr.Zero, 0, 0, screenWidth / 2, screenHeight, SWP_SHOWWINDOW);
                    break;
                case EResizeWindow.RightHalf:
                    SetWindowPos(hWnd, IntPtr.Zero, screenWidth / 2, 0, screenWidth / 2, screenHeight, SWP_SHOWWINDOW);
                    break;
                default:
                    break;
            }
        }

        static IntPtr GetUnityEditorWindowHandle() {
            // Попытка через заголовок окна
            IntPtr hWnd = FindWindow(null, GetEditorWindowTitle());
            if (hWnd != IntPtr.Zero) {
                return hWnd;
            }

            // Попытка через класс окна
            hWnd = FindWindow("UnityWndClass", null);
            if (hWnd != IntPtr.Zero) {
                return hWnd;
            }

            // Попытка получить активное окно
            hWnd = GetForegroundWindow();
            return hWnd;
        }

        static string GetEditorWindowTitle() {
            // Получаем название окна редактора
            return Application.productName + " - Unity Editor";
        }
    }
#endif
}
