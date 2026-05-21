# Void OS ❖

Void OS is a lightweight, fully self-contained GUI operating system environment built entirely in C# and Batch. 

Instead of shipping a pre-compiled executable, Void OS features a "Ghost Kernel" architecture: it compiles itself instantaneously from source code the moment you boot it, and completely purges itself the millisecond you shut it down.

## Features
- **First-Time Setup**: Securely prompts you to create a custom administrator Username and Password on your very first boot.
- **Void Window Manager**: A completely custom-built taskbar and window manager. Minimized apps detach from your main Windows taskbar and hide natively inside the Void OS environment.
- **Syntax-Highlighted Terminal**: A robust internal terminal featuring real-time color coding (`cyan` commands, `red` errors) and an asynchronous architecture that never freezes. Type `/esscmds` for a list of built-in commands.
- **Virtual Explorer**: A fully contained file system explorer that allows you to create, edit, move, rename, and delete files inside your Void workspace without touching Windows Explorer.
- **Custom Theming Engine**: Switch between Dark Mode, Cyber-Punk, and Classic Linux aesthetics on boot. All internal right-click menus are dynamically rendered to match your theme perfectly.
- **Retro Audio**: Built-in 8-bit boot chords and system error beeps (toggleable via the Start Menu).

## Installation & Usage

1. Download or clone this repository. Ensure all three core files are in the same directory:
   - `void.bat` (The Boot Loader)
   - `void_os.cs` (The OS Kernel Source Code)
   - `void.ico` (The System Icon)
2. Run `void.bat`.
3. Follow the First-Time Setup instructions to create your account.
4. Select your theme and enjoy your isolated environment!

## Architecture
Void OS relies on the native C# compiler (`csc.exe`) bundled with the Windows .NET Framework. When `void.bat` is executed, it locates your compiler, compiles `void_os.cs` with the custom `void.ico` burned in, launches `VoidOS.exe`, and cleans it up upon exit.

---
*Created by hima*
