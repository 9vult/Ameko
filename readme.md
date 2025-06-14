<h1 align="center">Ameko</h1>

<img alt="Ameko Banner Logo" src="https://files.catbox.moe/aylq3a.jpg" />

<div align="center">

![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blueviolet?logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0naHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmcnIHZpZXdCb3g9JzAgMCAxMjggMTI4Jz48cGF0aCBmaWxsPScjOUI0Rjk2JyBkPSdNMTE1LjQgMzAuN0w2Ny4xIDIuOWMtLjgtLjUtMS45LS43LTMuMS0uNy0xLjIgMC0yLjMuMy0zLjEuN2wtNDggMjcuOWMtMS43IDEtMi45IDMuNS0yLjkgNS40djU1LjdjMCAxLjEuMiAyLjQgMSAzLjVsMTA2LjgtNjJjLS42LTEuMi0xLjUtMi4xLTIuNC0yLjd6Jy8+PHBhdGggZmlsbD0nIzY4MjE3QScgZD0nTTEwLjcgOTUuM2MuNS44IDEuMiAxLjUgMS45IDEuOWw0OC4yIDI3LjljLjguNSAxLjkuNyAzLjEuNyAxLjIgMCAyLjMtLjMgMy4xLS43bDQ4LTI3LjljMS43LTEgMi45LTMuNSAyLjktNS40VjM2LjFjMC0uOS0uMS0xLjktLjYtMi44bC0xMDYuNiA2MnonLz48cGF0aCBmaWxsPScjZmZmJyBkPSdNODUuMyA3Ni4xQzgxLjEgODMuNSA3My4xIDg4LjUgNjQgODguNWMtMTMuNSAwLTI0LjUtMTEtMjQuNS0yNC41czExLTI0LjUgMjQuNS0yNC41YzkuMSAwIDE3LjEgNSAyMS4zIDEyLjVsMTMtNy41Yy02LjgtMTEuOS0xOS42LTIwLTM0LjMtMjAtMjEuOCAwLTM5LjUgMTcuNy0zOS41IDM5LjVzMTcuNyAzOS41IDM5LjUgMzkuNWMxNC42IDAgMjcuNC04IDM0LjItMTkuOGwtMTIuOS03LjZ6TTk3IDY2LjJsLjktNC4zaC00LjJ2LTQuN2g1LjFMMTAwIDUxaDQuOWwtMS4yIDYuMWgzLjhsMS4yLTYuMWg0LjhsLTEuMiA2LjFoMi40djQuN2gtMy4zbC0uOSA0LjNoNC4ydjQuN2gtNS4xbC0xLjIgNmgtNC45bDEuMi02aC0zLjhsLTEuMiA2aC00LjhsMS4yLTZoLTIuNHYtNC43SDk3em00LjggMGgzLjhsLjktNC4zaC0zLjhsLS45IDQuM3onLz48L3N2Zz4=&logoColor=white)
![Zig 0.14](https://img.shields.io/badge/Zig-0.14-f7a41d?logo=zig&logoColor=white)

</div>

Ameko is a subtitle editing suite for Advanced Substation Alpha (ASS) subtitles.

<h2 align="center">Roadmap</h2>

### Milestone 1 - MVP

The primary goal for Milestone 1 is to deliver a **Minimum Viable Product (MVP)**. For Ameko, this MVP will deliver a
feature-rich **subtitle editor** with several key features to provide a robust foundation for future extensibility.

- **Core Subtitle Editing:** This forms the heart of the MVP, providing essential tools for creating, manipulating, and
  styling subtitles.
- **Integrated Video Support:** I've chosen to include support in the MVP as it's an indispensable part of the
  subtitling workflow.
  Rendering is handled via a plugin-based system, ensuring flexibility and independence from any single backend. The
  initial video provider will be [FFMS2](https://github.com/FFMS/ffms2), and [libass](https://github.com/libass/libass/)
  will be the first subtitle renderer.
- **Tabs and Solutions:** Ameko will feature tabs, allowing users to easily switch between multiple open subtitle files.
  Alongside this, Ameko is introducing **Solution Files**. Solutions enable logical grouping and organization of
  subtitle files
  independent of the physical filesystem structure. Furthermore, Solutions provide a centralized place for common
  project configuration, styles, colors, and more, offering a significant improvement for teams using automated build
  processes and version control-based workflows.
- **C# Scripting Support:** Recognizing that many existing workflows rely heavily on scripting for automation and
  advanced tasks, the MVP will include robust support for C# scripts and libraries. While existing Lua/Moonscript
  scripts are not supported by Ameko, the C# scripting platform is highly versatile and supports development of both
  simple and complex scripts.

### Future Milestones

Ameko is envisioned as a comprehensive editing suite, and while the exact path is still unclear, future development will
focus on significantly expanding its capabilities. Potential upcoming milestones include:

- **Audio Integration:** Implementing audio playback and waveform and spectrum visualizations.
- **Graphical Tools:** Tools for visually manipulating subtitles on the video.

These are initial ideas — milestones will be added or removed based on user feedback, community contributions, and
the natural progression of Ameko's development.

<h2 align="center">Development</h2>

I would strongly recommend using Jetbrains Rider or Visual Studio for development.

- As a prerequisite, ensure all NuGet packages are installed.
- To build, either click the `Build` button in your IDE, or run `dotnet build` in the project root directory.
- The final output for debugging and running is the `Ameko` project
- Ameko follows XDG directory guidelines

<h2 align="center">Contributing</h2>

Thank you for your interest in contributing to Ameko! Whether you're reporting bugs, fixing bugs, adding features, or
starting a discussion, all contributions are welcome and appreciated.

Before submitting a pull request, please make sure your code is properly formatted:

- **C# code** is automatically formatted using [CSharpier](https://github.com/belav/csharpier) as part of the build
  process.
- **Zig code** should be formatted using `zig fmt`.

Additionally, there are some testing guidelines:

- **Unit tests are required** for contributions to the Holo and AssCS projects.
- **Tests are optional but highly appreciated** for Ameko's ViewModels and for Mizuki.

<h2 align="center">Licensing</h2>

- The Ameko application is licensed under the GNU GPL v3 license.
- Libraries developed for Ameko are licensed under the Mozilla Public License 2.0.
- For more information, see the LICENSE files.