// #if UNITY_EDITOR_WIN
// namespace _EditorTools.LayoutSwitcherUtility.Scripts {
//     using System;
//     using System.Runtime.InteropServices;
//     using UnityEditor;
//     using UnityEngine;
//
//     [InitializeOnLoad]
//     public static class EditorWindowResizeDetector {
//         const int SM_CXSCREEN = 0;
//         const int SM_CYSCREEN = 1;
//
//         static IntPtr          hWnd;
//         static int             screenWidth;
//         static int             screenHeight;
//         static bool            wasHalfScreen = false;
//         static DateTime lastCheckTime;
//
//         static EditorWindowResizeDetector() {
//             Debug.Log("EditorWindowResizeDetector initialized.");
//
//             hWnd = GetUnityEditorWindowHandle();
//             if (hWnd == IntPtr.Zero) {
//                 Debug.LogError("Не удалось найти окно Unity Editor (hWnd == IntPtr.Zero).");
//                 return;
//             }
//             Debug.Log("Получен дескриптор окна Unity Editor: " + hWnd);
//
//             screenWidth  = GetSystemMetrics(SM_CXSCREEN);
//             screenHeight = GetSystemMetrics(SM_CYSCREEN);
//             Debug.Log("Размер экрана: ширина = " + screenWidth + ", высота = " + screenHeight);
//
//             lastCheckTime            =  DateTime.Now;
//             EditorApplication.update += OnEditorUpdate;
//             Debug.Log("Подписались на EditorApplication.update.");
//         }
//
//         static void OnEditorUpdate() {
//             if (hWnd == IntPtr.Zero) {
//                 hWnd = GetUnityEditorWindowHandle();
//                 if (hWnd == IntPtr.Zero) {
//                     Debug.LogError("hWnd == IntPtr.Zero в OnEditorUpdate.");
//                     return;
//                 }
//             }
//
//             if ((DateTime.Now - lastCheckTime).TotalSeconds < 0.5)
//                 return;
//             lastCheckTime = DateTime.Now;
//
//             RECT rect;
//             if (GetWindowRect(hWnd, out rect)) {
//                 int windowWidth  = rect.Right - rect.Left;
//                 int windowHeight = rect.Bottom - rect.Top;
//                 // Debug.Log("Размер окна Unity Editor: ширина = " + windowWidth + ", высота = " + windowHeight);
//
//                 bool isHalfScreenWidth = Mathf.Abs(windowWidth - screenWidth / 2) < 10;
//                 if (isHalfScreenWidth && !wasHalfScreen) {
//                     // Debug.Log("Окно Unity Editor стало занимать половину экрана.");
//                     wasHalfScreen = true;
//                 }
//                 else if (!isHalfScreenWidth && wasHalfScreen) {
//                     // Debug.Log("Окно Unity Editor перестало занимать половину экрана.");
//                     wasHalfScreen = false;
//                 }
//             }
//             else {
//                 // int errorCode = Marshal.GetLastWin32Error();
//                 // Debug.LogError("GetWindowRect вернул false. Код ошибки: " + errorCode);
//             }
//         }
//
//         [DllImport("user32.dll", SetLastError = true)]
//         static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
//
//         [DllImport("user32.dll")]
//         static extern int GetSystemMetrics(int nIndex);
//
//         [DllImport("user32.dll", SetLastError = true)]
//         static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
//
//         [DllImport("user32.dll")]
//         static extern IntPtr GetForegroundWindow();
//
//         [StructLayout(LayoutKind.Sequential)]
//         struct RECT {
//             public int Left;
//             public int Top;
//             public int Right;
//             public int Bottom;
//         }
//
//         static IntPtr GetUnityEditorWindowHandle() {
//             string windowTitle = GetEditorWindowTitle();
//             IntPtr handle      = FindWindow(null, windowTitle);
//             if (handle != IntPtr.Zero) {
//                 Debug.Log("Окно найдено через заголовок: " + windowTitle);
//                 return handle;
//             }
//             Debug.LogWarning("Не удалось найти окно через заголовок: " + windowTitle);
//
//             handle = FindWindow("UnityWndClass", null);
//             if (handle != IntPtr.Zero) {
//                 Debug.Log("Окно найдено через класс окна UnityWndClass.");
//                 return handle;
//             }
//             Debug.LogWarning("Не удалось найти окно через класс UnityWndClass.");
//
//             handle = GetForegroundWindow();
//             if (handle != IntPtr.Zero) {
//                 Debug.Log("Получено активное окно.");
//                 return handle;
//             }
//             Debug.LogError("Не удалось получить активное окно.");
//             return IntPtr.Zero;
//         }
//
//         static string GetEditorWindowTitle() {
//             string title = Application.productName + " - Unity Editor";
//             Debug.Log("Ожидаемый заголовок окна Unity Editor: " + title);
//             return title;
//         }
//     }
// }
// #endif