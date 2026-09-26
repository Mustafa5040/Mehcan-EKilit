# Mehcan E-Kilit Modern (WinUI 3) 🔒🖥️

A modern, fluent redesign of the classic smartboard (interactive whiteboard) lock system commonly deployed in Turkish high schools, rebuilt from the ground up using **WinUI 3 (Windows App SDK)**, **.NET 6**, and **Win32 P/Invoke**.

---

## 💡 Origin & Story

In Turkish high schools, smartboards are typically locked between class sessions with legacy kiosk utilities (such as *Mehcan E-Kilit*), which require teachers to insert a dedicated physical USB flash drive containing an authorization token (`mehcan00.dat`) or enter a numeric bypass code.

This project was built during high school as a personal deep dive into desktop system programming: re-engineering the legacy lock behavior into a sleek, touch-friendly, **Windows 11 Fluent Design** interface featuring Mica / Desktop Acrylic backdrops and seamless hardware event hooks.

---

## ✨ Features

- **🔐 Dual Unlock Mechanisms**:
  - **Hardware USB Key**: Background WMI (`ManagementEventWatcher`) listener detects USB drive insertion/removal (`Win32_DeviceChangeEvent`), locates the volume path, and inspects token files (`mehcan00.dat` / `BELGELER\mehcan00.dat`).
  - **On-Screen Numeric Keypad**: Dynamic PIN modal dialog for manual password override.
- **🖥️ Kiosk & Lock Enforcement**:
  - Full-screen top-most window presentation (`AppWindowPresenterKind.FullScreen`).
  - Windows Taskbar suppression via native Win32 `FindWindow("Shell_TrayWnd")` and `ShowWindow(SW_HIDE / SW_SHOW)`.
  - Disables minimize and maximize system interactions during lock state.
  - Quick floating unlock controller widget for active teaching sessions.
- **⏰ Smart Schedule & Bell Timetable**:
  - Automated state engine that parses current time & weekday against class and break periods (teneffüs).
  - Automatically transitions between lesson lock modes and shows the active course title on the lock screen.
- **🎨 Windows 11 Fluent UI**:
  - Built with WinUI 3 controls.
  - Native Desktop Acrylic backdrops with automatic Dark/Light theme synchronization.
  - Integrated `slidetoshutdown.exe` touch action.

---

## 🛠️ Tech Stack & Architecture

- **Framework**: .NET 6 (`net6.0-windows10.0.19041.0`)
- **UI Platform**: WinUI 3 (Windows App SDK 1.1.5)
- **MVVM / Toolkit**: `CommunityToolkit.Mvvm`
- **Native Interop**:
  - `PInvoke.User32` for window management (`SetWindowPos`, DPI scaling).
  - Direct P/Invoke to `user32.dll` for taskbar control (`Shell_TrayWnd`).
- **Hardware & Device Query**: `System.Management` (WMI queries over `Win32_PnPEntity`, `Win32_DiskDrive`, and `Win32_LogicalDisk`).

---

## 📁 Project Structure

```text
Mehcan-EKilit/
├── App.xaml / App.xaml.cs            # Entry point & Taskbar Win32 controller
├── MainWindow.xaml / .xaml.cs       # Lock screen layout, WMI USB watcher, Schedule timer & Keypad dialog
├── Package.appxmanifest             # MSIX packaging & Full Trust capabilities
├── app.manifest                     # Per-monitor DPI awareness declarations
└── Assets/                          # Lock logos, splash screens, and QR visual assets
```

---

## 🚀 Getting Started

### Prerequisites

- **Windows 10 (Build 1809+) or Windows 11**
- **Visual Studio 2022** with the **Windows App SDK / WinUI 3** workload installed.
- **.NET 6 SDK** or higher.

### Building & Running

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/Mehcan-EKilit.git
   cd Mehcan-EKilit
   ```
2. Open `Mehcan-EKilit.sln` in Visual Studio 2022.
3. Set the build architecture to `x64` (or `arm64` if on ARM).
4. Build and run via `F5` (Packaged or Unpackaged profile).

> **Note**: Because the application hides the Windows taskbar and forces full-screen topmost behavior, ensure you have a flash drive with `mehcan00.dat` or know the keypad passcode (`314159265`) before running on your primary workstation.

---

## 📌 Retrospective & Takeaways

As one of my earliest projects with **WinUI 3**:
- Bridging modern XAML with lower-level Win32 APIs proved how flexible the Windows App SDK can be for system utilities.
- Implemented real-time hardware plug-and-play event consumption using asynchronous background threads.
- Learned the nuances of window positioning, DPI scaling, and custom window presenters on Windows desktop.