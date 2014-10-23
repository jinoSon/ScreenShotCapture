using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using Com.CurtisRutland.WpfHotkeys;
using Delay;
using System.Runtime.InteropServices;
using MouseKeyboardLibrary;
using System.Diagnostics;

namespace ScreenShotCapture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public static bool recodeMode = false;

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private MyDataContext myDataContext;
        private Hotkey globalHotkey;
        private CustomWindow customWindow;
       

        public MainWindow()
        {
            InitializeComponent();
            myDataContext = new MyDataContext();
            folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.DataContext = myDataContext;
            MinimizeToTray.Enable(this);
            
            
        }

        void MouseHook_MouseAction(object sender, MouseEventArgs e)
        {

            if (this.WindowState == WindowState.Minimized)
            {
                activieWindowSceen();
                //makeFullScreen(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
            }
            
           
           
        }

     
       
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            globalHotkey = new Hotkey(Modifiers.NoMod, Keys.PrintScreen, this, true);
            globalHotkey.HotkeyPressed += setRecodeMode;

            MouseHook.Start();
            MouseHook.MouseAction += MouseHook_MouseAction;
          
        }


      

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            globalHotkey.Dispose();
            MouseHook.stop();
       
        }

        private void selectFolder(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                myDataContext.Folder = folderBrowserDialog.SelectedPath;
            }
            
        }

        private void setRecodeMode(object sender, HotkeyEventArgs e)
        {
            if (recodeMode)
            {
                recodeMode = false;
                this.Icon = getWindowIcon();
               // MinimizeToTray.RecodeMode(new System.Drawing.Icon("favicon.ico"));
                // "pack://application:,,/icon/GB4.bmp"
            }
            else{
                recodeMode = true;
                this.Icon = getWindowIcon();
                //MinimizeToTray.RecodeMode(new System.Drawing.Icon("faviconActive.ico"));
            }
           
        }
        public static ImageSource getWindowIcon()
        {
            Uri address = null;

            if (recodeMode)
            {
                recodeMode = false;
                address = new Uri("pack://application:,,/favicon.ico");
                // "pack://application:,,/icon/GB4.bmp"
            }
            else
            {
                recodeMode = true;
                //this.Icon = new BitmapImage(new Uri("pack://application:,,/faviconActive.ico"));
                address = new Uri("pack://application:,,/faviconActive.ico");
            }

            return new BitmapImage(address);

        }  



        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref System.Drawing.Rectangle rect);

        private void activieWindowSceen()
        {
            System.Drawing.Rectangle resolution = new System.Drawing.Rectangle();
            IntPtr tempPtr = GetForegroundWindow();
            GetWindowRect(tempPtr, ref resolution);


            if (resolution.Width != 0 && resolution.Height != 00)
            {
                System.Drawing.Bitmap image = ScreenShotMaker.CaptureScreen(resolution.Width - resolution.X, 
                                                                            resolution.Height - resolution.Y, 
                                                                            resolution.X, resolution.Y);
                saveImage(image);
            }

                    

        }
        private void makeFullScreen(int x = 0, int y = 0)
        {
            //double k = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;

            //System.Drawing.Rectangle resolution = System.Windows.Forms.Screen.GetBounds()


            //System.Drawing.Rectangle resolution = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;


            int key = 0; 

            System.Windows.Forms.Screen[] tempScreenList = System.Windows.Forms.Screen.AllScreens;

            for (int i = 0; i < tempScreenList.Length; i++)
            {
                if(tempScreenList[i].Bounds.Contains(new System.Drawing.Point(x, y))){
                    key = i;
                    break;
                }
                    
            }
            System.Drawing.Rectangle resolution = tempScreenList[key].Bounds;
            System.Drawing.Bitmap image = ScreenShotMaker.CaptureScreen(resolution.Width, resolution.Height, resolution.X, resolution.Y);
            saveImage(image);


        }

        private void saveImage(System.Drawing.Bitmap image)
        {
            createDirectory();
            string path = System.IO.Path.Combine(myDataContext.Folder, DateTime.Now.ToString("yyyyMMdd-HHmmss")).ToString() + ".png";
            image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }

        private void createDirectory()
        {
            try
            {
                if (Directory.Exists(myDataContext.Folder))
                {
                    return;
                }
                DirectoryInfo di = Directory.CreateDirectory(myDataContext.Folder);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("The process failed: " + e.ToString());
            }
            finally { }
        }

        private void createCustomScreenShot(object sender, RoutedEventArgs e)
        {
            customWindow = new CustomWindow();
            customWindow.Closing += CustomWindow_Closed;
            customWindow.Show();
        }

        private void CustomWindow_Closed(object sender, EventArgs e)
        {
            saveImage(customWindow.bitmap);
        }
    }
    public static class MouseHook
    {
        public static event MouseEventHandler MouseAction = delegate { };

        public static void Start()
        {
            _hookID = SetHook(_proc);


        }
        public static void stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                  GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
          int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                //MouseAction(null, new MouseEventHandler());
                MouseAction(null, new MouseEventArgs(InputManager.Current.PrimaryMouseDevice, 0 ));
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
          LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
          IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


    }
}
