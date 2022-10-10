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

namespace Mehcan_EKilit
{

    public sealed partial class MainWindow : Window
    {
        List<string> teacher_device_ids = new List<string>();
        AppWindow main_window;
        OverlappedPresenter main_window_presenter;
        DispatcherTimer Clock_timer = new DispatcherTimer();
        DispatcherTimer Lesson_timer = new DispatcherTimer();
        BackgroundWorker bgwDriveDetector = new BackgroundWorker();
        public OverlappedPresenter GetAppWindowAndPresenter()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId main_window_id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

            main_window = AppWindow.GetFromWindowId(main_window_id);
            main_window_presenter = main_window.Presenter as OverlappedPresenter;
            return main_window_presenter;
        }
        public MainWindow()
        {
            var teacher_devices = new List<string>();

            this.InitializeComponent();

            main_window_presenter = GetAppWindowAndPresenter();
            main_window = GetAppWindowForCurrentWindow();
            main_window.SetPresenter(AppWindowPresenterKind.FullScreen);



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

        }

        public void Check_teacher_usb_startup()
        {
            foreach (USBDeviceInfo currentusb in GetUSBDevices())
            {
                string device = currentusb.DeviceId.Substring(currentusb.DeviceId.IndexOf(@"\") + 1);
                device = device.Split(@"\")[0];
                device = usbget.FindPath(device);
                string current_device_letter = usbget.GetDriveLetter(device);
                string location1 = current_device_letter + @"\mehcan.dat";
                string location2 = current_device_letter + @"\BELGELER\mehcan.dat";
                string location3 = current_device_letter + @"\mehcan00.dat";
                string location4 = current_device_letter + @"\BELGELER\mehcan00.dat";
                string location5 = current_device_letter + @"\mehcan0.dat";
                string location6 = current_device_letter + @"\BELGELER\mehcan0.dat";
                if (current_device_letter != null && (File.Exists(location1) || File.Exists(location2) || File.Exists(location3) || File.Exists(location4) || File.Exists(location5) || File.Exists(location6)))
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
            string _time2 = time[0].ToString() + time[1].ToString() + time[3].ToString() + time[4].ToString();
            int time_3 = Int16.Parse(_time2);
            if (0830 <= time_3 && time_3 < 910)
            {
                ders_text.Text = "1.Ders";
            }
            else if (0910 <= time_3 && time_3 < 0920)
            {
                ders_text.Text = "1.Teneffüs";
            }
            else if (0920 <= time_3 && time_3 < 1000)
            {
                ders_text.Text = "2.Ders";
            }
            else if (1000 <= time_3 && time_3 < 1010)
            {
                ders_text.Text = "2.Teneffüs";
            }
            else if (1010 <= time_3 && time_3 < 1050)
            {
                ders_text.Text = "3.Ders";
            }
            else if (1050 <= time_3 && time_3 < 1100)
            {
                ders_text.Text = "3.Teneffüs";
            }
            else if (1100 <= time_3 && time_3 < 1140)
            {
                ders_text.Text = "4.Ders";
            }
            else if (1140 <= time_3 && time_3 < 1150)
            {
                ders_text.Text = "4.Teneffüs";
            }
            else if (1150 <= time_3 && time_3 < 1230)
            {
                ders_text.Text = "5.Ders";
            }
            else if (1230 <= time_3 && time_3 < 1330)
            {
                ders_text.Text = "Öğle Arası";
            }
            else if (1330 <= time_3 && time_3 < 1410)
            {
                ders_text.Text = "6.Ders";
            }
            else if (1410 <= time_3 && time_3 < 1420)
            {
                ders_text.Text = "6.Teneffüs";
            }
            else if (1420 <= time_3 && time_3 < 1500)
            {
                ders_text.Text = "7.Ders";
            }
            else if (1500 <= time_3 && time_3 < 1510)
            {
                ders_text.Text = "7.Teneffüs";
            }
            else if (1510 <= time_3 && time_3 < 1550)
            {
                ders_text.Text = "8.Ders";
            }
            else
            {
                ders_text.Text = "Ders Dışı";
            }

        }
        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

            return AppWindow.GetFromWindowId(myWndId);
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

            var anahtar_gir_window = new Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(anahtar_gir_window);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            anahtar_gir_apwindow = AppWindow.GetFromWindowId(myWndId);
            anahtar_gir_window_presenter = anahtar_gir_apwindow.Presenter as OverlappedPresenter;
            Border anahtar_gir_title_bar = new Border();
            anahtar_gir_window.Title = "Anahtar Gir";
            anahtar_gir_window.ExtendsContentIntoTitleBar = true;
            SetWindowSize(hwnd, 400, 400);
            anahtar_gir_window_presenter.IsResizable = false;
            anahtar_gir_window_presenter.IsMaximizable = false;
            anahtar_gir_window_presenter.IsMinimizable = false;
            anahtar_gir_window_presenter.IsAlwaysOnTop = true;

            Grid ana_grid = new Grid();

            Grid alt_grid = new Grid();
            alt_grid.Width = 200;
            alt_grid.Height = 260;
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


            NumberBox anahtar_textBox = new NumberBox();
            anahtar_textBox.FontSize = 23;
            anahtar_textBox.Width = 198;
            anahtar_textBox.Margin = new Thickness(0, 20, 23, 0);
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


            Button keypad_tick = new Button();
            keypad_tick.Content = ":)";
            keypad_tick.Width = 60;
            keypad_tick.Height = 60;
            Grid.SetRow(keypad_tick, 3);
            Grid.SetColumn(keypad_tick, 2);
            keypad_tick.Click += (sender, EventArgs) => { keypad_tick_Click(sender, EventArgs, anahtar_textBox.Text); };


            Button keypad_back = new Button();
            keypad_back.Content = "<";
            keypad_back.Width = 60;
            keypad_back.Height = 60;
            Grid.SetRow(keypad_back, 3);
            Grid.SetColumn(keypad_back, 0);
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
            alt_grid.Margin = new Thickness(0, 30, 20, 0);

            ana_grid.Children.Add(alt_grid);

            anahtar_gir_window.Content = ana_grid;
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
                }

            }
            void keypad_tick_Click(object sender, RoutedEventArgs e, string text_box_text)
            {
                if (text_box_text == "314159265")
                {
                    Taskbar.Show();
                    anahtar_gir_apwindow.Hide();
                    main_window.Hide();
                }
                anahtar_textBox.Text = "";
            }
        }
        private void SetWindowSize(IntPtr hwnd, int width, int height)
        {
            // Win32 uses pixels and WinUI 3 uses effective pixels, so you should apply the DPI scale factor
            var dpi = PInvoke.User32.GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            width = (int)(width * scalingFactor);
            height = (int)(height * scalingFactor);

            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE);
        }
        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            string current_letter = e.NewEvent.Properties["DriveName"].Value.ToString();

            string location1 = current_letter + @"\mehcan.dat";
            string location2 = current_letter + @"\BELGELER\mehcan.dat";
            string location3 = current_letter + @"\mehcan00.dat";
            string location4 = current_letter + @"\BELGELER\mehcan00.dat";
            string location5 = current_letter + @"\mehcan0.dat";
            string location6 = current_letter + @"\BELGELER\mehcan0.dat";

            if (File.Exists(location1) || File.Exists(location2) || File.Exists(location3) || File.Exists(location4) || File.Exists(location5) || File.Exists(location6))
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

            if (current_devices.Count == 0 || current_devices == null)
            {
                main_window.Show();
                Taskbar.Hide();
            }
            else if (current_devices != null && current_devices.Count > 0)
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

                if (paired_teacher_usb_ids.Count == 0)
                {
                    main_window.Show();
                    Taskbar.Hide();
                    teacher_device_ids.Clear();
                }
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
                Debug.WriteLine("No drive identified!");
                return null;
            }
            return null;
        }
    }
}
