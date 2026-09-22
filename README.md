<p align="center">
  <img src="appicon.png" alt="Music File Organizer" width="120" height="120">
</p>

# Music Files Organizer

A lightweight native Windows desktop app (WinForms) that reads the tags on
your music files and uses them to move/copy the files into an
artist/album‑artist/year/album folder structure of your choosing, renaming
them from any combination of track number, artist, album, title, year,
genre, and disc number along the way.

## What it does

- Add source files or whole folders via buttons **or** drag & drop.
- Pick a destination folder via a button **or** drag & drop.
- Choose and re‑order the tags that build the destination folder structure
  (e.g. `Album Artist \ Album`, or `Artist \ Year - Album`, etc. — any
  combination, in any order).
- Choose and re‑order the tags that build the output file name (e.g.
  `Track Number - Title`, or `Artist - Title`), with a customizable
  separator between each tag.
- Live preview of an example output path before you run anything.
- Move (default) or copy, with automatic conflict‑safe renaming
  (`Song (1).mp3`) and a progress bar + log of every file processed.
- Reads tags from basically every common audio format via TagLib# (see
  below).
- **About** button with version info and a clickable author link.

## Why WinForms + TagLib#

"Lightweight native Windows" without an embedded browser (i.e. not
Electron) realistically means either raw Win32/C++ or .NET WinForms.
WinForms draws real native Win32 common controls, has a tiny footprint,
and — critically — the tag‑reading requirement ("preferably all types of
tags") is what actually decides the stack: **TagLib#** (NuGet package
`TagLibSharp`) is the one mature, actively‑used open‑source library that
correctly reads ID3v1/ID3v2, Xiph/Vorbis comments, MP4 atoms, ASF/WMA
objects, APE tags, etc. across formats — re‑implementing all of that by
hand in raw C++ would be an enormous undertaking for a small utility.
WinForms + TagLib# gives you a small, fast, genuinely native‑looking
Windows app with rock‑solid tag support.

## Supported audio formats

`.mp3` `.flac` `.m4a` `.m4b` `.mp4` `.aac` `.ogg` `.oga` `.opus` `.wma`
`.wav` `.aiff` `.aif` `.ape` `.wv` `.mpc` `.tta` `.dsf` `.dff`

## Dependencies

| Dependency | Purpose | How it's obtained |
|---|---|---|
| [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0/runtime) (Windows) | Running the source code or the non-standalone .exe | Manual install |
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (Windows) | Compiler + WinForms runtime | Manual install, see below |
| [TagLibSharp](https://www.nuget.org/packages/TagLibSharp) 2.3.0 | Reads/writes audio tags | Restored automatically from NuGet via the `.csproj` |
| Visual Studio 2022 (optional) | IDE with a debugger/designer | Manual install, see below |

No other third‑party libraries are used — everything else is plain
`System.Windows.Forms` / `System.IO`.

## Project files

```
MusicFilesOrganizer/
├── MusicFilesOrganizer.csproj   # project + NuGet reference
├── Program.cs                   # app entry point
├── MainForm.cs                  # main window, all UI + logic
├── AboutForm.cs                 # About dialog
├── Metadata.cs                  # TagInfo, MetadataToken enum, label/resolve helpers
├── TagReader.cs                 # TagLib# wrapper for reading tags
├── Organizer.cs                 # path building, sanitizing, move/copy
├── appicon.ico                  # App icon (ICO format)
├── appicon.png                  # App icon (PNG format)
└── README.md
```

## Step‑by‑step build instructions

### Option A — Command line (fastest)

1. **Install the .NET 8 SDK** (Windows x64): download and run the
   installer from https://dotnet.microsoft.com/download/dotnet/8.0
   (choose the **SDK**, not just the runtime). Restart your terminal
   after installing.
2. **Verify the install**:
   ```
   dotnet --version
   ```
   should print something starting with `8.`.
3. **Unzip the project** into a folder, e.g. `C:\Projects\MusicFilesOrganizer`.
4. **Open a terminal in that folder** (PowerShell, Command Prompt, or
   Windows Terminal) — `cd C:\Projects\MusicFilesOrganizer`.
5. **Restore the NuGet package** (downloads TagLibSharp automatically):
   ```
   dotnet restore
   ```
6. **Build it**:
   ```
   dotnet build -c Release
   ```
   The compiled app will be at
   `bin\Release\net8.0-windows\MusicFilesOrganizer.exe`.
7. **Run it** straight from the CLI while testing:
   ```
   dotnet run -c Release
   ```

### Option B — Visual Studio 2022 (GUI, with designer/debugger)

1. Install **Visual Studio 2022** (Community edition is free) with the
   **".NET desktop development"** workload selected in the installer.
2. Unzip the project folder, then double‑click
   `MusicFilesOrganizer.csproj` to open it in Visual Studio.
3. Visual Studio will automatically restore the `TagLibSharp` NuGet
   package on load (or right‑click the project → *Restore NuGet
   Packages* if it doesn't).
4. Press **F5** (or **Ctrl+F5** to run without debugging) to build and
   launch the app.

### Producing a distributable EXE

You have two choices for how you hand the app to someone else:

**Framework‑dependent (small, ~1 MB, requires the .NET 8 Desktop Runtime
on the target PC):**
```
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

**Self‑contained single file (bigger, ~70–150 MB, runs on any Windows
10/11 x64 PC with nothing else installed):**
```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Either command outputs a single `MusicFilesOrganizer.exe` under
`bin\Release\net8.0-windows\win-x64\publish\`.

## Notes / things you can tweak

- Default folder structure is `Artist \ Album`; default file naming is
  `Track Number - Title`. Both are just starting points set in
  `MainForm.PopulateDefaults()` — change them there if you want different
  defaults, or just adjust them in the app itself every time.
- Characters that Windows doesn't allow in file/folder names (e.g. the
  `/` in an artist like "AC/DC") are automatically replaced with `_`.
- If a tag is missing, sensible fallbacks are used (e.g. "Unknown Album",
  Album Artist falls back to Artist and vice versa).
- The window is fixed‑size by design to keep the layout code simple; feel
  free to switch `FormBorderStyle` to `Sizable` in `MainForm.cs` and add
  anchors if you want it resizable.

## License

This program is licensed under GNU General Public License v3.0 (GPLv3)
