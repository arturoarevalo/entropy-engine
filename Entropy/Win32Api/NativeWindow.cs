namespace Entropy.Win32Api
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    public class NativeWindow {
        public delegate IntPtr WndProc (IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public string ClassName { get; protected set; }
        public IntPtr Handle { get; protected set; }
        public IntPtr Instance { get; protected set; }

        protected WndProc WindowProcedureDelegate;

        public NativeWindow (string className, string caption, int width, int height) {
            ClassName = className;
            Handle = IntPtr.Zero;
            Instance = System.Diagnostics.Process.GetCurrentProcess ().Handle;
            WindowProcedureDelegate = WindowProcedure;

            RegisterClass ();
            CreateWindow (caption, width, height);
        }

        protected void RegisterClass () {
            WNDCLASSEX wcx = new WNDCLASSEX {
                                                cbSize = Marshal.SizeOf<WNDCLASSEX> (),
                                                style = (int) (ClassStyles.VerticalRedraw | ClassStyles.HorizontalRedraw),
                                                lpfnWndProc = Marshal.GetFunctionPointerForDelegate (WindowProcedureDelegate),
                                                cbClsExtra = 0,
                                                cbWndExtra = 0,
                                                hInstance = Instance,
                                                hIcon = Externals.LoadIcon (IntPtr.Zero, new IntPtr ((int) SystemIcons.IDI_APPLICATION)),
                                                hCursor = Externals.LoadCursor (IntPtr.Zero, (int) Win32_IDC_Constants.IDC_ARROW),
                                                hbrBackground = Externals.GetStockObject (StockObjects.WHITE_BRUSH),
                                                lpszMenuName = @"MainMenu",
                                                lpszClassName = ClassName
                                            };

            const int ERROR_CLASS_ALREADY_EXISTS = 1410;
            if (Externals.RegisterClassEx (ref wcx) == 0 && Marshal.GetLastWin32Error () != ERROR_CLASS_ALREADY_EXISTS) {
                throw new Win32Exception (Marshal.GetLastWin32Error ());
            }
        }

        protected void CreateWindow (string caption, int width, int height) {
            Handle = Externals.CreateWindowEx (WindowStylesEx.WS_EX_APPWINDOW,
                                               ClassName,
                                               caption,
                                               WindowStyles.WS_OVERLAPPED | WindowStyles.WS_SYSMENU,
                                               Win32_CW_Constant.CW_USEDEFAULT,
                                               Win32_CW_Constant.CW_USEDEFAULT,
                                               width,
                                               height,
                                               IntPtr.Zero,
                                               IntPtr.Zero,
                                               Instance,
                                               IntPtr.Zero);

            if (Handle == IntPtr.Zero) {
                throw new Win32Exception (Marshal.GetLastWin32Error ());
            }

            Externals.ShowWindow (Handle, ShowWindowCommands.Normal);
            Externals.UpdateWindow (Handle);
        }

        public void MessageLoop () {
            sbyte hasMessage;
            MSG msg;

            while ((hasMessage = Externals.GetMessage (out msg, IntPtr.Zero, 0, 0)) != 0 && hasMessage != -1) {
                Externals.TranslateMessage (ref msg);
                Externals.DispatchMessage (ref msg);
            }
        }

        public bool RealtimeMessageLoop () {
            MSG msg;

            while (Externals.PeekMessage (out msg, IntPtr.Zero, 0, 0, 1)) {
                Externals.TranslateMessage (ref msg);
                Externals.DispatchMessage (ref msg);
            }

            return msg.message == (uint) WM.QUIT;
        }

        protected virtual IntPtr WindowProcedure (IntPtr handle, uint msg, IntPtr wParam, IntPtr lParam) {
            switch ((WM) msg) {
                case WM.PAINT:
                {
                    PAINTSTRUCT ps;
                    RECT rect;

                    IntPtr hdc = Externals.BeginPaint (handle, out ps);
                    Externals.GetClientRect (handle, out rect);
                    Externals.DrawText (hdc, "Hello, Entropy!", -1, ref rect, (uint) (DrawTextFormat.DT_SINGLELINE | DrawTextFormat.DT_CENTER | DrawTextFormat.DT_VCENTER));
                    Externals.EndPaint (handle, ref ps);
                    return IntPtr.Zero;
                }

                case WM.CLOSE:
                {
                    Externals.DestroyWindow (handle);
                    return IntPtr.Zero;
                }

                case WM.DESTROY:
                {
                    Externals.PostQuitMessage (0);
                    return IntPtr.Zero;
                }
            }

            return Externals.DefWindowProc (handle, msg, wParam, lParam);
        }
    }
}