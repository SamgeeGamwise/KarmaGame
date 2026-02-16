# KarmaGame

High-level 2D game workspace built with MonoGame and MonoGame.Extended.

This repo currently includes:
1. `Engine` shared runtime utilities (game host, input bridge, tiled map runtime, Y-sort helpers).
2. `Sandbox` playable testbed where new systems are prototyped first.
3. `Karma` game project (separate executable target).

## Repo layout
1. `Engine/Engine` shared engine code (`Engine.Core.*`).
2. `Sandbox/Sandbox` sandbox game and content pipeline.
3. `Karma/Karma` main game executable project.
4. `KarmaGame/KarmaGame.slnx` solution file that includes all projects.

## Helpful docs in this repo
1. `Engine/Engine/README.md` engine runtime surface and migration notes.
2. `Engine/Engine/TILED_WORKFLOW.md` Tiled authoring contract and map pipeline.
3. `Engine/Engine/EXTENDED_MIGRATION.md` old-vs-new architecture details.
4. `Engine/Engine/ROADMAP.md` medium/long-term direction.
5. `Sandbox/Sandbox/Content/Maps/README.md` map asset placement notes.
6. `Codex.md` internal continuation notes for Codex-assisted sessions.

## Windows first-time setup
Follow these steps in order.

### 1) Install Visual Studio 2026 Community
1. Go to the official Visual Studio download page and install `Visual Studio 2026 Community`.
2. In the Visual Studio Installer, select workloads:
   - `.NET desktop development`
   - `Game development with C++`
3. In the installer, ensure `.NET 9 SDK` (or newer SDK that can target .NET 9/8) is installed.
4. Finish installation and reboot Windows if prompted.

### 2) Install Git (required for cloning)
1. Download and install `Git for Windows` from the official Git site.
2. Keep default installer options if you are unsure.

### 3) Clone the repository in Visual Studio
1. Open Visual Studio.
2. Click `Clone a repository`.
3. Paste your GitHub repo URL.
4. Choose a local folder, for example: `C:\Dev\KarmaGame`.
5. Click `Clone`.
6. After cloning, open `KarmaGame\KarmaGame.slnx` if Visual Studio does not open it automatically.

### 4) Restore/build once
1. Wait while Visual Studio restores NuGet packages (bottom status bar).
2. Build once: `Build` -> `Build Solution`.
3. First build may take a few minutes because content pipeline tools are restored.

### 5) Run Sandbox (recommended first run)
1. In Solution Explorer, right-click `Sandbox` project.
2. Click `Set as Startup Project`.
3. Press `F5` (Run with debugger) or `Ctrl+F5` (Run without debugger).
4. Controls:
   - `W A S D` move
   - `Left Shift` run
   - `Esc` exit

### 6) Run Karma project (optional)
1. Right-click `Karma` project.
2. Click `Set as Startup Project`.
3. Press `F5` or `Ctrl+F5`.

## If something fails
1. Close any running game window before rebuilding (locks `Sandbox.exe`).
2. If build says SDK missing, install the requested `.NET SDK` and restart Visual Studio.
3. If content build fails, run `Build -> Rebuild Solution` once.
4. If cloning/auth fails, verify your GitHub account has access to the private repo URL.

## Command-line alternative (optional)
If you prefer terminal:
1. `git clone <your-repo-url>`
2. `cd KarmaGame`
3. `dotnet build KarmaGame/KarmaGame.slnx`
4. `dotnet run --project Sandbox/Sandbox/Sandbox.csproj`
