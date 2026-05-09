# WpfAndroidInteractor

A proof-of-concept WPF application that demonstrates two-way, real-time
communication between a Windows desktop app and an Android device (or any
browser) using **SignalR** over an **ADB reverse port-forwarding** tunnel —
no Wi-Fi, no public network, just a USB cable.

## What it does

- Hosts a small HTTP server inside the WPF app that serves a single web
  page (`index.html`) plus the jQuery / SignalR client scripts.
- Hosts a SignalR hub (`MessagePropagator`) inside the same WPF process so
  the desktop UI and any connected client can exchange messages in real time.
- Uses `adb reverse` to expose the desktop's `localhost` ports to the USB-
  attached Android device, so the device can browse the page and talk to
  the SignalR hub as if the WPF app were running locally on the phone.
- Pressing **"Say Hello To Customer"** in the WPF window broadcasts a
  message to every connected client; clicking **Hello!!!** in the browser
  pops up a `MessageBox` on the desktop.

## How it works

```
+--------------------------+         USB / adb reverse         +-----------------------+
|         WPF App          |  tcp:3001 -> tcp:3001 (HTTP)      |    Android Device     |
|  (WpfAndroidInteractor)  |  tcp:3002 -> tcp:3002 (SignalR)   |   Browser / WebView   |
|                          | <-------------------------------> |                       |
|  - HTTP server  :3001    |                                   |  index.html           |
|  - SignalR hub  :3002    |                                   |  jQuery + SignalR JS  |
|  - bundled adb.exe       |                                   |                       |
+--------------------------+                                   +-----------------------+
```

On startup, `MainWindow` spins up a background thread that:

1. Starts the SignalR server on `http://localhost:3002` (`SignalRServer.Start`).
2. Calls `ADBInteractor.StartADB`, which restarts the adb server, checks
   for an authorized device, and runs `adb reverse tcp:3001 tcp:3001` and
   `adb reverse tcp:3002 tcp:3002`.
3. Starts a `SimpleHttpServer` on port 3001 that serves three routes:
   `index.html`, `jquery-1.9.1.min.js`, and `jquery.signalR-2.2.3.min.js`
   from `Resources/Pages/`.

When the window is closed, `OnClosed` calls `ADBInteractor.StopADB` to
kill the adb server and then terminates the process.

## Tech stack

- **.NET Framework 4.6.1** WPF application (C#)
- **Microsoft.AspNet.SignalR** (self-hosted via OWIN / Katana)
- **SimpleHttpServer** for serving the static client page
- **adb.exe** (bundled in `Resources/Adb/`) for USB reverse-tunnelling
- **jQuery 1.9.1** + **SignalR JS client 2.2.3** on the browser side

## Prerequisites

- Windows with **Visual Studio 2019+** (or MSBuild) and the .NET Framework
  4.6.1 developer pack installed.
- An Android device with **USB debugging** enabled (only needed for the
  on-device demo; you can also test entirely in a desktop browser).
- The bundled adb binaries under `Resources/Adb/` are used as-is — no
  separate Android SDK install is required.

## Build

```powershell
# Restore NuGet packages and build the solution
nuget restore WpfAndroidInteractor.sln
msbuild WpfAndroidInteractor.sln /p:Configuration=Debug
```

Or open `WpfAndroidInteractor.sln` in Visual Studio and press **F5**.

## Run

1. Launch the WPF app (`WpfAndroidInteractor.exe`).
2. Choose one of the two clients:
   - **Desktop browser:** open <http://localhost:3001/index.html>.
   - **Android device:** plug it into the PC over USB, accept the USB
     debugging prompt on the device, then open
     <http://localhost:3001/index.html> in the device's browser. The
     `adb reverse` tunnel makes `localhost` on the phone resolve to the
     PC.
3. Interact:
   - Click **Say Hello To Customer** in the WPF window — the connected
     client(s) update their page with `"Hello"`.
   - Click **Hello!!!** in the browser — a `MessageBox` shows
     `Customer says hello` on the desktop.

## Ports

| Port | Used by                                  |
| ---- | ---------------------------------------- |
| 3001 | Static HTTP server (serves `index.html`) |
| 3002 | SignalR hub (`/signalr`)                 |

Both are reverse-forwarded to the attached Android device when one is
present and authorized.

## Project layout

```
WpfAndroidInteractor/
├── App.xaml / App.xaml.cs        # WPF application entry point
├── MainWindow.xaml / .xaml.cs    # UI + bootstraps HTTP server, SignalR, ADB
├── SignalRServer.cs              # OWIN-hosted SignalR hub (MessagePropagator)
├── ADBInteractor.cs              # Wraps adb.exe (start/stop, reverse ports)
├── Resources/
│   ├── Adb/                      # Bundled adb.exe + Windows USB DLLs
│   └── Pages/                    # index.html, jQuery, SignalR JS client
├── packages.config               # NuGet dependencies
└── WpfAndroidInteractor.csproj
```

## Troubleshooting

- **"no connected devices" / "no authorized connected devices"** — make
  sure USB debugging is enabled and the *Allow USB debugging?* dialog has
  been accepted on the phone. The reverse-tunnel step is skipped when no
  authorized device is detected, but the desktop-browser flow still works.
- **Port already in use** — another process is bound to 3001 or 3002.
  Stop it or change the ports in `MainWindow.xaml.cs` and
  `SignalRServer.cs` (and update `index.html` accordingly).
- **Browser shows nothing / SignalR not connecting** — verify the WPF app
  is still running and that `http://localhost:3002/signalr/hubs` returns
  JavaScript when opened directly.

## License

Proof-of-concept code; use at your own risk.
