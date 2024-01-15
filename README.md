# Hourglass

[![Latest build](https://github.com/i2van/hourglass/workflows/build/badge.svg)](https://github.com/i2van/hourglass/actions)
[![Latest release](https://img.shields.io/github/downloads/i2van/hourglass/total.svg)](https://github.com/i2van/hourglass/releases/latest)
[![License](https://img.shields.io/badge/license-MIT-yellow)](https://opensource.org/licenses/MIT)

This project is the modified [simple countdown timer for Windows](https://github.com/dziemborowicz/hourglass). The changes were made to the original **Hourglass** can be found [here](#hourglass-changes). The latest **Hourglass** installer or portable distribution can be downloaded [here](https://github.com/i2van/hourglass/releases/latest).

Visit the [original Hourglass site](https://chris.dziemborowicz.com/apps/hourglass) to learn more. **Hourglass** FAQ can be found [here](https://chris.dziemborowicz.com/apps/hourglass/#downloads).

## Hourglass Command-line Example

```shell
hourglass -n on -a on -g on -c on -w minimized -i left+title -t "Timer 1" 1h23
```

creates

- 1 hour 23 minutes long timer: `1h23`
- named **Timer 1**: `-t "Timer 1"`
- with the notification area icon: `-n on`
- with the timer window:
  - which is always on top: `-a on`
  - shows the reversed progress bar: `-g on`
  - displays time in the digital clock format: `-c on`
  - initially minimized: `-w minimized`
  - has the time left and the timer name displayed in the title: `-i left+title`

Run `hourglass -h` to display the **Hourglass** [command-line reference](https://github.com/i2van/hourglass/blob/develop/Hourglass/Resources/Usage.txt) or select **Command-line usage** from the notification area context menu.

## Prerequisites

- [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) (click [here](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-web-installer) to download the runtime Web Installer)

## Hourglass Changes

### Installer

- Adds the **Hourglass** executable path to the [Windows App Paths](https://learn.microsoft.com/en-us/windows/win32/shell/app-registration#using-the-app-paths-subkey), so the **Hourglass** [command-line](https://github.com/i2van/hourglass/blob/develop/Hourglass/Resources/Usage.txt) is available out of the box.

### UI

- Uses a [Windows Task Dialog](https://learn.microsoft.com/en-us/windows/win32/controls/task-dialogs-overview) instead of a message box ([GitHub](https://github.com/kpreisser/TaskDialog)).

### Notification Area

- The double click shows/hides all the timer windows.
- All the timers are arranged by the time left in the notification area context menu.
- The nearest remaining running timers are shown in the notification area tooltip.
- The **Exit** menu asks to close all the running timer windows if the **Prompt on exit** option is on.
- The new **Command-line usage** menu.

### Timer Windows

- The `Esc` shortcut minimizes the timer window.
- The `F11` shortcut makes the timer window full screen and back.
- The `Ctrl+N` shortcut creates a new timer window.
- The mouse double-click on progress border makes the timer window full screen and back.
- The minimum timer window size is limited by the Windows.
- The timer tooltip is shown if the timer window size is too small.
- All the timer windows are arranged by the time left. The order of timer windows is new, expired, paused, running.
- When the timer window is minimized or closed the next visible non-minimized timer window is activated.
- The **Window title** submenu is available directly from the timer window context menu.
- The **Reset bounds** menu item sets the timer window default position and size.
- The **Restore**, **Minimize** and **Maximize** timer window commands are always present in the timer window context menu.
- All the timer window commands are available in the timer window context menu.
- Shortcuts are displayed in the timer window context menu.
- The progress bar changes direction to vertical when the height is more than the width and vice versa.
- The switching between light and dark themes is improved.
- The **Display time in the digital clock format** menu item toggles the displayed time digital clock time format. It can be found under the **Advanced options** submenu of the timer window context menu. The command-line option is  `--digital-clock-time` / `-c`

### Misc

- The **Hourglass** is built deterministically using the [GitHub Actions](https://github.com/i2van/hourglass/actions).

## Troubleshooting

If the **Hourglass** does not start or fails silently, delete the **Hourglass** settings.

The **Hourglass** settings can be located by the following command (to run it press `Win`+`R` and copy-paste command below):

```shell
cmd /k dir "C:\Users\%USERNAME%\AppData\Local\Chris_Dziemborowicz*"
```

The settings are stored into the corresponding `hourglass.EXE` subdirectories.

The **Hourglass Portable** keeps settings next to the executable.
