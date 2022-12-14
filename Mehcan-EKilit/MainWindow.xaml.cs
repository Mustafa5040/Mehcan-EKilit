using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using WinRT;

namespace Mehcan_EKilit
{

    public sealed partial class MainWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        List<string> teacher_device_ids = new List<string>();
        bool is_anahtar_gir_open;
        bool is_opened_via_password;
        bool hided_automatically;
        bool is_mini_window_opened;
        WindowsSystemDispatcherQueueHelper m_wsdqHelper; // See separate sample below for implementation
        Microsoft.UI.Composition.SystemBackdrops.MicaController m_micaController;
        Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController m_acrylicController;
        Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration m_configurationSource;

        IntPtr hwnd_main;
        AppWindow main_window;
        OverlappedPresenter main_window_presenter;
        DispatcherTimer Clock_timer = new DispatcherTimer();
        DispatcherTimer Lesson_timer = new DispatcherTimer();
        BackgroundWorker bgwDriveDetector = new BackgroundWorker();

        public MainWindow()
        {
            var teacher_devices = new List<string>();

            this.InitializeComponent();
            hwnd_main = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId main_window_id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd_main);

            main_window = AppWindow.GetFromWindowId(main_window_id);
            main_window_presenter = main_window.Presenter as OverlappedPresenter;

            SetWindowSize(hwnd_main, 1920, 1080, 1, false);

            main_window.SetPresenter(AppWindowPresenterKind.FullScreen);

            TrySetAcrylicBackdrop();

            main_window_presenter.IsAlwaysOnTop = true;
            main_window_presenter.IsMinimizable = false;
            main_window_presenter.IsMaximizable = false;
            Clock_timer.Tick += Timer_Tick;
            Clock_timer.Interval = new TimeSpan(0, 0, 1);
            Clock_timer.Start();

            Lesson_timer.Tick += Lesson_timer_Tick;
            Lesson_timer.Interval = new TimeSpan(0, 0, 1);
            Lesson_timer.Start();

           
            bgwDriveDetector.DoWork += bgwDriveDetector_DoWork;
            bgwDriveDetector.RunWorkerAsync();

            bool TrySetAcrylicBackdrop()
            {
                if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
                {
                    m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                    m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                    // Hooking up the policy object
                    m_configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
                    this.Activated += Window_Activated;
                    this.Closed += Window_Closed;
                    ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                    // Initial configuration state.
                    m_configurationSource.IsInputActive = true;
                    SetConfigurationSourceTheme();

                    m_acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController();

                    // Enable the system backdrop.
                    // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                    m_acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                    m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                    return true; // succeeded
                }

                return false; // Acrylic is not supported on this system
            }

            void Window_Activated(object sender, WindowActivatedEventArgs args)
            {
                m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
            }

            void Window_Closed(object sender, WindowEventArgs args)
            {
                // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
                // use this closed window.
                if (m_acrylicController != null)
                {
                    m_acrylicController.Dispose();
                    m_acrylicController = null;
                }
                this.Activated -= Window_Activated;
                m_configurationSource = null;
            }

            void Window_ThemeChanged(FrameworkElement sender, object args)
            {
                if (m_configurationSource != null)
                {
                    SetConfigurationSourceTheme();
                }
            }

            void SetConfigurationSourceTheme()
            {
                switch (((FrameworkElement)this.Content).ActualTheme)
                {
                    case ElementTheme.Dark: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark; break;
                    case ElementTheme.Light: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light; break;
                    case ElementTheme.Default: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default; break;
                }
            }
        }

        public void Check_teacher_usb_startup()
        {
            foreach (USBDeviceInfo currentusb in GetUSBDevices())
            {
                string device = currentusb.DeviceId.Substring(currentusb.DeviceId.IndexOf(@"\") + 1);
                device = device.Split(@"\")[0];
                device = usbget.FindPath(device);
                string current_device_letter = usbget.GetDriveLetter(device);
                string location1 = current_device_letter + @"\mehcan00.dat";
                string location2 = current_device_letter + @"\BELGELER\mehcan00.dat";
                if (current_device_letter != null && (File.Exists(location1) || File.Exists(location2)))
                {
                    main_window.Hide();
                    Taskbar.Show();
                    if (!teacher_device_ids.Contains(currentusb.DeviceId))
                    {
                        teacher_device_ids.Add(currentusb.DeviceId);
                    }
                }
            }
        }
        private void Timer_Tick(object sender, object e)
        {
            saat_buton_text.Text = DateTime.Now.ToString("HH:mm");

        }
        private void Lesson_timer_Tick(object sender, object e)
        {
           
            string time = DateTime.Now.ToString("HH:mm");
            string day = DateTime.Now.DayOfWeek.ToString();
            string _time2 = time[0].ToString() + time[1].ToString() + time[3].ToString() + time[4].ToString();
            int time_3 = Int16.Parse(_time2);

            if (0830 <= time_3 && time_3 < 0910)
            {
                if (day != DayOfWeek.Monday.ToString())
                {
                    main_window.Show();
                    Taskbar.Hide();
                    hided_automatically = false;
                }
                if (day == DayOfWeek.Monday.ToString())
                {
                    ders_text.Text = "1.Ders: Edebiyat";
                    main_window.Hide();
                    Taskbar.Show();
                    hided_automatically = true;
                }
              
                if (day == DayOfWeek.Tuesday.ToString())
                {
                    ders_text.Text = "1.Ders: Beden";
                }
                if (day == DayOfWeek.Wednesday.ToString())
                {
                    ders_text.Text = "1.Ders: Matematik";
                }
                if (day == DayOfWeek.Thursday.ToString())
                {
                    ders_text.Text = "1.Ders: Fizik <3";
                }
                if (day == DayOfWeek.Friday.ToString())
                {
                    ders_text.Text = "1.Ders: Kimya";
                }
                else
                {
                    ders_text.Text = "1.Ders";
                }

            }
            else if (0910 <= time_3 && time_3 < 0920)
            {
                if (hided_automatically)
                {
                    main_window.Show();
                    Taskbar.Hide();
                }
                ders_text.Text = "1.Teneffüs";
            }
            else if (0920 <= time_3 && time_3 < 1000)
            {
                if (day != DayOfWeek.Monday.ToString())
                {
                    main_window.Show();
                    Taskbar.Hide();
                    hided_automatically = false;
                }
                if (day == DayOfWeek.Monday.ToString())
                {
                    ders_text.Text = "2.Ders: Edebiyat";
                    main_window.Hide();
                    Taskbar.Show();
                    hided_automatically = true;
                }
                if (day == DayOfWeek.Tuesday.ToString())
                {
                    ders_text.Text = "2.Ders: Beden";
                }
                if (day == DayOfWeek.Wednesday.ToString())
                {
                    ders_text.Text = "2.Ders: Edebiyat";
                }
                if (day == DayOfWeek.Thursday.ToString())
                {
                    ders_text.Text = "2.Ders: Fizik <3";
                }
                if (day == DayOfWeek.Friday.ToString())
                {
                    ders_text.Text = "2.Ders: Kimya";
                }
                else
                {
                    ders_text.Text = "2.Ders";
                }
            }
            else if (1000 <= time_3 && time_3 < 1010)
            {
                if (hided_automatically)
                {
                    main_window.Show();
                    Taskbar.Hide();
                }
                ders_text.Text = "2.Teneffüs";
            }
            else if (1010 <= time_3 && time_3 < 1050)
            {
                if (day != DayOfWeek.Thursday.ToString())
                {
                    main_window.Show();
                    Taskbar.Hide();
                    hided_automatically = false;
                }
                if (day == DayOfWeek.Monday.ToString())
                {
                    ders_text.Text = "3.Ders: İngilizce";
                }
                if (day == DayOfWeek.Tuesday.ToString())
                {
                    ders_text.Text = "3.Ders: Matematik";
                }
                if (day == DayOfWeek.Wednesday.ToString())
                {
                    ders_text.Text = "3.Ders: Almanca";
                }
                if (day == DayOfWeek.Thursday.ToString())
                {
                    ders_text.Text = "3.Ders: Edebiyat";
                    main_window.Hide();
                    Taskbar.Show();
                    hided_automatically = true;
                }
                if (day == DayOfWeek.Friday.ToString())
                {
                    ders_text.Text = "3.Ders: Matematik";
                }
                else
                {
                    ders_text.Text = "3.Ders";
                }
            }
            else if (1050 <= time_3 && time_3 < 1100)
            {
                if (hided_automatically)
                {
                    main_window.Show();
                    Taskbar.Hide();
                }
                ders_text.Text = "3.Teneffüs";
            }
            else if (1100 <= time_3 && time_3 < 1140)
            {
                if (day != DayOfWeek.Thursday.ToString())
                {
                    main_window.Show();
                    Taskbar.Hide();
                    hided_automatically = false;
                }

                if (day == DayOfWeek.Monday.ToString())
                {
                    ders_text.Text = "4.Ders: İngilizce";
                }
                if (day == DayOfWeek.Tuesday.ToString())
                {
                    ders_text.Text = "4.Ders: Matematik";
                }
                if (day == DayOfWeek.Wednesday.ToString())
                {
                    ders_text.Text = "4.Ders: Almanca";
                }
                if (day == DayOfWeek.Thursday.ToString())
                {
                    ders_text.Text = "4.Ders: Edebiyat";
                    main_window.Hide();
                    Taskbar.Show();
                    hided_automatically = true;
                }
                if (day == DayOfWeek.Friday.ToString())
                {
                    ders_text.Text = "4.Ders: Matematik";
                }
                else
                {
                    ders_text.Text = "4.Ders";
                }
            }
            else if (1140 <= time_3 && time_3 < 1150)
            {
                ders_text.Text = "4.Teneffüs";
            }
            else if (1150 <= time_3 && time_3 < 1230)
            {
                if (day == DayOfWeek.Monday.ToString())
                {
                    ders_text.Text = "5.Ders: İngilizce";
                }
                if (day == DayOfWeek.Tuesday.ToString())
                {
                    ders_text.Text = "5.Ders: Matematik";
                }
                if (day == DayOfWeek.Wednesday.ToString())
                {
                    ders_text.Text = "5.Ders: Almanca";
                }
                if (day == DayOfWeek.Thursday.ToString())
                {
                    ders_text.Text = "5.Ders: Matematik";
                   
                }
                if (day == DayOfWeek.Friday.ToString())
                {
                    ders_text.Text = "5.Ders: İngilizce";
                }
                else
                {
                    ders_text.Text = "5.Ders";
                }
            }
            else if (1230 <= time_3 && time_3 < 1330)
            {
                if (hided_automatically)
                {
                    main_window.Show();
                    Taskbar.Hide();
                }
                ders_text.Text = "Öğle Arası";
            }
            else if (1330 <= time_3 && time_3 < 1410)
            {
                if (hided_automatically)
                {
                    main_window.Show();
                    Taskbar.Hide();
                }
                if (day == DayOfWeek.Monday.ToString())
                {
                    ders_text.Text = "6.Ders: Fizik";
                }
                if (day == DayOfWeek.Tuesday.ToString())
                {
                    ders_text.Text = "6.Ders: Felsefe";
                }
                if (day == DayOfWeek.Wednesday.ToString())
                {
                    ders_text.Text = "6.Ders: Biyoloji";
                }
                if (day == DayOfWeek.Thursday.ToString())
                {
                    ders_text.Text = "6.Ders: Müzik";
                }
                if (day == DayOfWeek.Friday.ToString())
                {
                    ders_text.Text = "6.Ders: İngilizce";
                }
                else
                {
                    ders_text.Text = "6.Ders";
                }
            }
            else if (1410 <= time_3 && time_3 < 1420)
            {
                if (hided_automatically)
                {
                    main_window.Show();
                    Taskbar.Hide();
                }
                ders_text.Text = "6.Teneffüs";
            }
            else if (1420 <= time_3 && time_3 < 1500)
            {
                if(hided_automatically)
                {
                    main_window.Show();
                    Taskbar.Hide();
                }
                if (DateTime.Now.DayOfWeek.ToString() == DayOfWeek.Monday.ToString())
                {
                    ders_text.Text = "7.Ders: Din";
                }
                if (DateTime.Now.DayOfWeek.ToString() == DayOfWeek.Tuesday.ToString())
                {
                    ders_text.Text = "7.Ders: Biyoloji";
                }
                if (DateTime.Now.DayOfWeek.ToString() == DayOfWeek.Wednesday.ToString())
                {
                    ders_text.Text = "7.Ders: Kimya";
                }
                if (DateTime.Now.DayOfWeek.ToString() == DayOfWeek.Thursday.ToString())
                {
                    ders_text.Text = "7.Ders: Müzik";
                }
                if (DateTime.Now.DayOfWeek.ToString() == DayOfWeek.Friday.ToString())
                {
                    ders_text.Text = "7.Ders: Tarih";
                }
                else
                {
                    ders_text.Text = "7.Ders";
                }
            }
            else if (1500 <= time_3 && time_3 < 1510)
            {
                if (hided_automatically)
                {
                    main_window.Show();
                    Taskbar.Hide();
                }
                ders_text.Text = "7.Teneffüs";
            }
            else if (1510 <= time_3 && time_3 < 1550)
            {
                if (hided_automatically)
                {
                    main_window.Show();
                    Taskbar.Hide();
                }
                if (DateTime.Now.DayOfWeek.ToString() == DayOfWeek.Monday.ToString())
                {
                    ders_text.Text = "8.Ders: Din";
                }
                if (DateTime.Now.DayOfWeek.ToString() == DayOfWeek.Tuesday.ToString())
                {
                    ders_text.Text = "8.Ders: Biyoloji";
                }
                if (DateTime.Now.DayOfWeek.ToString() == DayOfWeek.Wednesday.ToString())
                {
                    ders_text.Text = "8.Ders: Kimya";
                }
                if (DateTime.Now.DayOfWeek.ToString() == DayOfWeek.Thursday.ToString())
                {
                    ders_text.Text = "8.Ders: Rehberlik";
                }
                if (DateTime.Now.DayOfWeek.ToString() == DayOfWeek.Friday.ToString())
                {
                    ders_text.Text = "8.Ders: Tarih";
                }
                else
                {
                    ders_text.Text = "8.Ders";
                }
            }
            else
            {
                if (hided_automatically)
                {
                    main_window.Show();
                    Taskbar.Hide();
                }
                ders_text.Text = "Ders Dışı";
            }

        }
        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

            return AppWindow.GetFromWindowId(myWndId);
        }
        public class WindowsSystemDispatcherQueueHelper
        {
            [StructLayout(LayoutKind.Sequential)]
            struct DispatcherQueueOptions
            {
                internal int dwSize;
                internal int threadType;
                internal int apartmentType;
            }

            [DllImport("CoreMessaging.dll")]
            private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

            object m_dispatcherQueueController = null;
            public void EnsureWindowsSystemDispatcherQueueController()
            {
                if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
                {
                    // one already exists, so we'll just use it.
                    return;
                }

                if (m_dispatcherQueueController == null)
                {
                    DispatcherQueueOptions options;
                    options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                    options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                    options.apartmentType = 2; // DQTAT_COM_STA

                    CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
                }
            }
        }



        public class Taskbar
        {
            [DllImport("user32.dll")]
            private static extern int FindWindow(string className, string windowText);

            [DllImport("user32.dll")]
            private static extern int ShowWindow(int hwnd, int command);

            [DllImport("user32.dll")]
            public static extern int FindWindowEx(int parentHandle, int childAfter, string className, int windowTitle);

            [DllImport("user32.dll")]
            private static extern int GetDesktopWindow();

            private const int SW_HIDE = 0;
            private const int SW_SHOW = 1;

            protected static int Handle
            {
                get
                {
                    return FindWindow("Shell_TrayWnd", "");
                }
            }

            protected static int HandleOfStartButton
            {
                get
                {
                    int handleOfDesktop = GetDesktopWindow();
                    int handleOfStartButton = FindWindowEx(handleOfDesktop, 0, "button", 0);
                    return handleOfStartButton;
                }
            }

            public static void Show()
            {
                ShowWindow(Handle, SW_SHOW);
                ShowWindow(HandleOfStartButton, SW_SHOW);
            }

            public static void Hide()
            {
                ShowWindow(Handle, SW_HIDE);
                ShowWindow(HandleOfStartButton, SW_HIDE);
            }
        }

        private void kilidi_ac_buton_Click(object sender, RoutedEventArgs e)
        {
            AppWindow anahtar_gir_apwindow;
            OverlappedPresenter anahtar_gir_window_presenter;

            if (!is_anahtar_gir_open)
            {
                is_anahtar_gir_open = true;
                
                var anahtar_gir_window = new Window();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(anahtar_gir_window);
                WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                anahtar_gir_apwindow = AppWindow.GetFromWindowId(myWndId);
                anahtar_gir_window_presenter = anahtar_gir_apwindow.Presenter as OverlappedPresenter;
                Border anahtar_gir_title_bar = new Border();
                anahtar_gir_window.Title = "Anahtar Gir";
                anahtar_gir_apwindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
                anahtar_gir_window.ExtendsContentIntoTitleBar = true;
                SetWindowSize(hwnd_main, 1920, 1080, 3, false);
                SetWindowSize(hwnd, 400, 400, 1, true);

                anahtar_gir_window_presenter.IsResizable = false;
                anahtar_gir_window_presenter.IsMaximizable = false;
                anahtar_gir_window_presenter.IsMinimizable = false;
                anahtar_gir_window_presenter.IsAlwaysOnTop = true;

                Grid ana_grid = new Grid();

                Grid alt_grid = new Grid();
                alt_grid.Width = 200;
                alt_grid.Height = 300;
                alt_grid.HorizontalAlignment = HorizontalAlignment.Right;
                alt_grid.VerticalAlignment = VerticalAlignment.Center;

                ColumnDefinition colDef1 = new ColumnDefinition();
                ColumnDefinition colDef2 = new ColumnDefinition();
                ColumnDefinition colDef3 = new ColumnDefinition();

                alt_grid.ColumnDefinitions.Add(colDef1);
                alt_grid.ColumnDefinitions.Add(colDef2);
                alt_grid.ColumnDefinitions.Add(colDef3);

                RowDefinition rowDef1 = new RowDefinition();
                RowDefinition rowDef2 = new RowDefinition();
                RowDefinition rowDef3 = new RowDefinition();
                RowDefinition rowDef4 = new RowDefinition();


                alt_grid.RowDefinitions.Add(rowDef1);
                alt_grid.RowDefinitions.Add(rowDef2);
                alt_grid.RowDefinitions.Add(rowDef3);
                alt_grid.RowDefinitions.Add(rowDef4);


                TextBox anahtar_textBox = new TextBox();
                anahtar_textBox.FontSize = 23;
                anahtar_textBox.Width = 200;
                anahtar_textBox.Margin = new Thickness(0, 30, 23, 0);
                anahtar_textBox.VerticalAlignment = VerticalAlignment.Top;
                anahtar_textBox.HorizontalAlignment = HorizontalAlignment.Right;
                ana_grid.Children.Add(anahtar_textBox);


                Button keypad1 = new Button();
                keypad1.Content = "1";
                keypad1.Width = 60;
                keypad1.Height = 60;
                Grid.SetRow(keypad1, 0);
                Grid.SetColumn(keypad1, 0);
                keypad1.Click += (sender, EventArgs) => { keypad_num_Click(sender, EventArgs, "1"); };


                Button keypad2 = new Button();
                keypad2.Content = "2";
                keypad2.Width = 60;
                keypad2.Height = 60;
                Grid.SetRow(keypad2, 0);
                Grid.SetColumn(keypad2, 1);
                keypad2.Click += (sender, EventArgs) => { keypad_num_Click(sender, EventArgs, "2"); };

                Button keypad3 = new Button();
                keypad3.Content = "3";
                keypad3.Width = 60;
                keypad3.Height = 60;
                Grid.SetRow(keypad3, 0);
                Grid.SetColumn(keypad3, 2);
                keypad3.Click += (sender, EventArgs) => { keypad_num_Click(sender, EventArgs, "3"); };

                Button keypad4 = new Button();
                keypad4.Content = "4";
                keypad4.Width = 60;
                keypad4.Height = 60;
                Grid.SetRow(keypad4, 1);
                Grid.SetColumn(keypad4, 0);
                keypad4.Click += (sender, EventArgs) => { keypad_num_Click(sender, EventArgs, "4"); };

                Button keypad5 = new Button();
                keypad5.Content = "5";
                keypad5.Width = 60;
                keypad5.Height = 60;
                Grid.SetRow(keypad5, 1);
                Grid.SetColumn(keypad5, 1);
                keypad5.Click += (sender, EventArgs) => { keypad_num_Click(sender, EventArgs, "5"); };

                Button keypad6 = new Button();
                keypad6.Content = "6";
                keypad6.Width = 60;
                keypad6.Height = 60;
                Grid.SetRow(keypad6, 1);
                Grid.SetColumn(keypad6, 2);
                keypad6.Click += (sender, EventArgs) => { keypad_num_Click(sender, EventArgs, "6"); };

                Button keypad7 = new Button();
                keypad7.Content = "7";
                keypad7.Width = 60;
                keypad7.Height = 60;
                Grid.SetRow(keypad7, 2);
                Grid.SetColumn(keypad7, 0);
                keypad7.Click += (sender, EventArgs) => { keypad_num_Click(sender, EventArgs, "7"); };

                Button keypad8 = new Button();
                keypad8.Content = "8";
                keypad8.Width = 60;
                keypad8.Height = 60;
                Grid.SetRow(keypad8, 2);
                Grid.SetColumn(keypad8, 1);
                keypad8.Click += (sender, EventArgs) => { keypad_num_Click(sender, EventArgs, "8"); };

                Button keypad9 = new Button();
                keypad9.Content = "9";
                keypad9.Width = 60;
                keypad9.Height = 60;
                Grid.SetRow(keypad9, 2);
                Grid.SetColumn(keypad9, 2);
                keypad9.Click += (sender, EventArgs) => { keypad_num_Click(sender, EventArgs, "9"); };

                Button keypad0 = new Button();
                keypad0.Content = "0";
                keypad0.Width = 60;
                keypad0.Height = 60;    
                Grid.SetRow(keypad0, 3);
                Grid.SetColumn(keypad0, 1);
                keypad0.Click += (sender, EventArgs) => { keypad_num_Click(sender, EventArgs, "0"); };

                AppBarButton keypad_tick = new AppBarButton();
                keypad_tick.Icon = new SymbolIcon(Symbol.Accept);
                
                keypad_tick.Width = 60;
                keypad_tick.Height = 70;

                Grid.SetRow(keypad_tick, 3);
                Grid.SetColumn(keypad_tick, 2);
                keypad_tick.Label = "Tamam";
                keypad_tick.Click += (sender, EventArgs) => { keypad_tick_Click(sender, EventArgs, anahtar_textBox.Text); };


                AppBarButton keypad_back = new AppBarButton();
                keypad_back.Icon = new SymbolIcon(Symbol.Back);
                keypad_back.Width = 60;
                keypad_back.Height = 70;
                Grid.SetRow(keypad_back, 3);
                Grid.SetColumn(keypad_back, 0);
                keypad_back.Label = "Sil";
                keypad_back.Click += back_button_Click;


                alt_grid.Children.Add(keypad1);
                alt_grid.Children.Add(keypad2);
                alt_grid.Children.Add(keypad3);
                alt_grid.Children.Add(keypad4);
                alt_grid.Children.Add(keypad5);
                alt_grid.Children.Add(keypad6);
                alt_grid.Children.Add(keypad7);
                alt_grid.Children.Add(keypad8);
                alt_grid.Children.Add(keypad9);
                alt_grid.Children.Add(keypad0);
                alt_grid.Children.Add(keypad_tick);
                alt_grid.Children.Add(keypad_back);
                alt_grid.Margin = new Thickness(0, 55, 20, 0);

                ana_grid.Children.Add(alt_grid);

                anahtar_gir_window.Content = ana_grid;
                anahtar_gir_window.Closed += Anahtar_gir_window_Closed;
                anahtar_gir_window.Activate();



                void keypad_num_Click(object sender, RoutedEventArgs e, string number)
                {
                    anahtar_textBox.Text += number;
                }
                void back_button_Click(object sender, RoutedEventArgs e)
                {
                    if (anahtar_textBox.Text.Length != 0)
                    {
                        anahtar_textBox.Text = anahtar_textBox.Text.Remove(anahtar_textBox.Text.Length - 1);
                        anahtar_textBox.SelectionStart = anahtar_textBox.Text.Length;
                        anahtar_textBox.SelectionLength = 0;
                    }
                }
                void keypad_tick_Click(object sender, RoutedEventArgs e, string text_box_text)
                {
                    if (text_box_text == "314159265")
                    {
                        Taskbar.Show();
                        anahtar_gir_apwindow.Hide();
                        main_window.Hide();
                        is_anahtar_gir_open = false;
                        SetWindowSize(hwnd_main, 1920, 1080, 1, false);

                        Grid maingrid = new Grid();

                        if(!is_mini_window_opened)
                        {
                            var mini_window = new Window();
                            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mini_window);
                            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hwnd);
                            var mini_window_appwindow = AppWindow.GetFromWindowId(myWndId);
                            var mini_window_presenter = mini_window_appwindow.Presenter as OverlappedPresenter;
                            mini_window.ExtendsContentIntoTitleBar = true;
                            mini_window_presenter.IsResizable = false;
                            mini_window_presenter.IsMaximizable = false;
                            mini_window_presenter.IsMinimizable = true;
                            mini_window_presenter.IsAlwaysOnTop = false;

                            AppBarButton lock_btn = new AppBarButton();
                            lock_btn.Icon = new SymbolIcon(Symbol.ProtectedDocument);
                            lock_btn.Width = 60;
                            lock_btn.Height = 60;
                            lock_btn.Label = "Kilitle";
                            lock_btn.Click += Lock_btn_Click;
                            lock_btn.VerticalAlignment = VerticalAlignment.Center;
                            lock_btn.HorizontalAlignment = HorizontalAlignment.Center;

                            maingrid.Children.Add(lock_btn);

                            mini_window.Content = maingrid;

                            SetWindowSize(hwnd, 200, 200, 3, true);

                            mini_window.Activate();
                            is_mini_window_opened = true;
                        }
                        
                    }
                    anahtar_textBox.Text = "";

                }
            }
        }

        private void Anahtar_gir_window_Closed(object sender, WindowEventArgs args)
        {
            is_anahtar_gir_open = false;

        }

        private void Lock_btn_Click(object sender, RoutedEventArgs e)
        {
            main_window.Show();
            Taskbar.Hide();
            SetWindowSize(hwnd_main, 1920, 1080, 1, false);
            is_opened_via_password = false;
            is_anahtar_gir_open = false;
        }

        public void SetWindowSize(IntPtr hwnd, int width, int height, int istopmost, bool isdpiscalingenabled)
        {
            if (isdpiscalingenabled)
            {
                var dpi = PInvoke.User32.GetDpiForWindow(hwnd);
                float scalingFactor = (float)dpi / 96;
                width = (int)(width * scalingFactor);
                height = (int)(height * scalingFactor);
            }


            if (istopmost == 1)
            {
                PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOPMOST,
                                        0, 0, width, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE);
            }
            else if (istopmost == 2)
            {
                PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE);
            }
            else if (istopmost == 3)
            {
                PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_NOTOPMOST,
                                        0, 0, width, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE);
            }

        }
        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            string current_letter = e.NewEvent.Properties["DriveName"].Value.ToString();

            string location1 = current_letter + @"\mehcan00.dat";
            string location2 = current_letter + @"\BELGELER\mehcan00.dat";

            if (File.Exists(location1) || File.Exists(location2))
            {
                main_window.Hide();
                Taskbar.Show();

                foreach (USBDeviceInfo currentusb in GetUSBDevices())
                {
                    string device = currentusb.DeviceId.Substring(currentusb.DeviceId.IndexOf(@"\") + 1);
                    device = device.Split(@"\")[0];

                    device = usbget.FindPath(device);
                    string current_device_letter = usbget.GetDriveLetter(device);

                    if (current_device_letter != null && current_letter[0] == current_device_letter[0])
                    {
                        if (!teacher_device_ids.Contains(currentusb.DeviceId))
                        {
                            is_opened_via_password = false;
                            teacher_device_ids.Add(currentusb.DeviceId);
                        }
                        break;
                    }
                }
            }

        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            List<string> paired_teacher_usb_ids = new List<string>();

            var current_devices = GetUSBDevices();

            if (current_devices != null && current_devices.Count != 0)
            {
                foreach (USBDeviceInfo currentusb in current_devices)
                {
                    if (currentusb == null) continue;

                    foreach (string teacher_ids in teacher_device_ids)
                    {
                        if (teacher_ids == currentusb.DeviceId)
                        {
                            paired_teacher_usb_ids.Add(teacher_ids);
                        }
                    }
                }
            }
            if (paired_teacher_usb_ids.Count == 0)
            {
                main_window.Show();
                Taskbar.Hide();
                teacher_device_ids.Clear();
            }
            var difference = paired_teacher_usb_ids.Except(teacher_device_ids);
            difference = difference.ToList();

            foreach (string different_teacher_id in difference)
            {
                teacher_device_ids.Remove(different_teacher_id);
            }
        }

        void bgwDriveDetector_DoWork(object sender, DoWorkEventArgs e)
        {
            var insertQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");

            var insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += DeviceInsertedEvent;
            insertWatcher.Start();

            var removeQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
            var removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += DeviceRemovedEvent;
            removeWatcher.Start();
        }
        static List<USBDeviceInfo> GetUSBDevices()
        {
            var devices = new List<USBDeviceInfo>();

            using (var mos = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity"))
            {
                using (ManagementObjectCollection collection = mos.Get())
                {
                    foreach (var device in collection)
                    {
                        var id = device.GetPropertyValue("DeviceId").ToString();

                        if (!id.StartsWith("USB", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var name = device.GetPropertyValue("Name").ToString();
                        var description = device.GetPropertyValue("Description").ToString();
                        devices.Add(new USBDeviceInfo(id, name, description));
                    }
                }
            }

            return devices;
        }

        private void kapat_buton_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo("slidetoshutdown.exe");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }
    }
    class USBDeviceInfo
    {
        public USBDeviceInfo(string deviceId, string name, string description)
        {
            DeviceId = deviceId;
            Name = name;
            Description = description;
        }

        public string DeviceId { get; }
        public string Name { get; }
        public string Description { get; }

        public override string ToString()
        {
            return Name;
        }
    }

    class usbget
    {
        static public string FindPath(string pattern)
        {
            var USBobjects = new List<string>();
            string Entity = "*none*";


            foreach (ManagementObject entity in new ManagementObjectSearcher(
                     $"select * from Win32_PnPEntity Where DeviceID Like '%{pattern}%'").Get())
            {
                Entity = entity["DeviceID"].ToString();

                foreach (ManagementObject controller in entity.GetRelated("Win32_USBController"))
                {
                    foreach (ManagementObject obj in new ManagementObjectSearcher(
                             "ASSOCIATORS OF {Win32_USBController.DeviceID='"
                             + controller["PNPDeviceID"].ToString() + "'}").Get())
                    {
                        if (obj.ToString().Contains("DeviceID"))
                            USBobjects.Add(obj["DeviceID"].ToString());

                    }
                }
            }

            int VidPidposition = USBobjects.IndexOf(Entity);
            for (int i = VidPidposition; i <= USBobjects.Count; i++)
            {
                if (USBobjects[i].Contains("USBSTOR"))
                {
                    return USBobjects[i];
                }
            }
            return "*none*";
        }

        public static string GetDriveLetter(string device)
        {
            int driveCount = 0;

            foreach (ManagementObject drive in new ManagementObjectSearcher("select * from Win32_DiskDrive").Get())
            {
                if (drive["PNPDeviceID"].ToString() == device)
                {
                    foreach (ManagementObject o in drive.GetRelated("Win32_DiskPartition"))
                    {
                        foreach (ManagementObject i in o.GetRelated("Win32_LogicalDisk"))
                        {
                            //Debug.WriteLine("Disk: " + i["Name"].ToString());
                            driveCount++;
                            return i["Name"].ToString();
                        }
                    }
                }
            }

            if (driveCount == 0)
            {
                //Debug.WriteLine("No drive identified!");
                return null;
            }
            return null;
        }

    }
}