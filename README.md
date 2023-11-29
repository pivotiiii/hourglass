# Hourglass

This project is the modified [simple countdown timer for Windows](https://github.com/dziemborowicz/hourglass). The changes were made to the original Hourglass can be found [here](#hourglass-changes). The latest Hourglass installer or portable distribution can be downloaded [here](https://github.com/i2van/hourglass/releases/latest).

Visit [chris.dziemborowicz.com](http://chris.dziemborowicz.com/apps/hourglass/) to learn more.

## Hourglass Command-line Example

```shell
hourglass -n on -a on -g on -w minimized -i left+title -t "Timer 1" 1h23
```

creates

- 1 hour 23 minutes long timer: `1h23`
- named **Timer 1**: `-t "Timer 1"`
- with notification area icon: `-n on`
- with timer window:
  - which is always on top: `-a on`
  - shows reversed progress bar: `-g on`
  - initially minimized: `-w minimized`
  - has the time left and timer name displayed in the title: `-i left+title`

Run `hourglass -h` to display Hourglass command-line reference or select **Command-line usage** from notification area context menu.

## Prerequisites

- [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) (click [here](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-web-installer) to download runtime Web Installer)

## Hourglass Changes

### Notification Area

- Double click shows/hides all timer windows.
- Timers are arranged by the time left in the notification area context menu.
- Nearest remaining running timers are shown in the notification area tooltip.
- **Exit** menu asks to close all the running timer windows if **Prompt on exit** option is on.
- New **Command-line usage** menu.

### Timer Windows

- `Ecs` key minimizes timer window.
- `F11` key makes timer window full screen and back.
- Timer windows are arranged by the time left where nearest remaining running timers are followed by expired ones if any.
- When timer window is minimized or closed next visible non-minimized timer window is activated.
- New close timer window message.

### Mics

- Hourglass is built deterministically using [GitHub Actions](https://github.com/i2van/hourglass/actions).

## Troubleshooting

If Hourglass does not start or fails silently, delete Hourglass settings.

Hourglass settings can be located by the following command (to run it press `Win`+`R` and copy-paste command below):

```shell
cmd /k dir "C:\Users\%USERNAME%\AppData\Local\Chris_Dziemborowicz*"
```

Settings are stored into corresponding `hourglass.EXE` subdirectories.

Hourglass Portable keeps settings next to the executable.
