<h1 align="center">Ameko</h1>

<img alt="Ameko Banner Logo" src="https://files.catbox.moe/aylq3a.jpg" />

<div align="center">

![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blueviolet?logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0naHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmcnIHZpZXdCb3g9JzAgMCAxMjggMTI4Jz48cGF0aCBmaWxsPScjOUI0Rjk2JyBkPSdNMTE1LjQgMzAuN0w2Ny4xIDIuOWMtLjgtLjUtMS45LS43LTMuMS0uNy0xLjIgMC0yLjMuMy0zLjEuN2wtNDggMjcuOWMtMS43IDEtMi45IDMuNS0yLjkgNS40djU1LjdjMCAxLjEuMiAyLjQgMSAzLjVsMTA2LjgtNjJjLS42LTEuMi0xLjUtMi4xLTIuNC0yLjd6Jy8+PHBhdGggZmlsbD0nIzY4MjE3QScgZD0nTTEwLjcgOTUuM2MuNS44IDEuMiAxLjUgMS45IDEuOWw0OC4yIDI3LjljLjguNSAxLjkuNyAzLjEuNyAxLjIgMCAyLjMtLjMgMy4xLS43bDQ4LTI3LjljMS43LTEgMi45LTMuNSAyLjktNS40VjM2LjFjMC0uOS0uMS0xLjktLjYtMi44bC0xMDYuNiA2MnonLz48cGF0aCBmaWxsPScjZmZmJyBkPSdNODUuMyA3Ni4xQzgxLjEgODMuNSA3My4xIDg4LjUgNjQgODguNWMtMTMuNSAwLTI0LjUtMTEtMjQuNS0yNC41czExLTI0LjUgMjQuNS0yNC41YzkuMSAwIDE3LjEgNSAyMS4zIDEyLjVsMTMtNy41Yy02LjgtMTEuOS0xOS42LTIwLTM0LjMtMjAtMjEuOCAwLTM5LjUgMTcuNy0zOS41IDM5LjVzMTcuNyAzOS41IDM5LjUgMzkuNWMxNC42IDAgMjcuNC04IDM0LjItMTkuOGwtMTIuOS03LjZ6TTk3IDY2LjJsLjktNC4zaC00LjJ2LTQuN2g1LjFMMTAwIDUxaDQuOWwtMS4yIDYuMWgzLjhsMS4yLTYuMWg0LjhsLTEuMiA2LjFoMi40djQuN2gtMy4zbC0uOSA0LjNoNC4ydjQuN2gtNS4xbC0xLjIgNmgtNC45bDEuMi02aC0zLjhsLTEuMiA2aC00LjhsMS4yLTZoLTIuNHYtNC43SDk3em00LjggMGgzLjhsLjktNC4zaC0zLjhsLS45IDQuM3onLz48L3N2Zz4=&logoColor=white)
![Zig 0.15.1](https://img.shields.io/badge/Zig-0.15.1-f7a41d?logo=zig&logoColor=white)

</div>

Ameko is a subtitle editing suite for Advanced Substation Alpha (ASS) subtitles.

<h2 align="center">Roadmap</h2>

### Milestone 1 - MVP

The primary goal for Milestone 1 is to deliver a Minimum Viable Product (MVP).

- **Core Subtitle Editing:** Creating, manipulating, and style subtitles.
- **Audio and Video Playback:** Initially [FFMS2](https://github.com/FFMS/ffms2)
  and [libass](https://github.com/libass/libass/), rendered using OpenGL and OpenALSoft. A plugin architecture will
  allow for this to be extended in the future.
- **Tabs and Projects:** Ameko features tabs, allowing users to easily switch between multiple open subtitle files.
  Additionally, Ameko is introducing *Project Files*. Projects enable logical grouping and organization of
  subtitle files
  independent of the physical filesystem structure and provide a centralized place for common
  project configuration, styles, colors, and more.
- **Integrated Git Support:** Ameko features basic Git features, like commiting, pulling, pushing, and blaming.
- **Scripting:** Ameko includes robust support for C# scripts and libraries. Ameko also features a scripting playground
  and support for simple JavaScript scriptlets.

### Future Milestones

Ameko is envisioned as a comprehensive editing suite, and while the exact path is still unclear, future development will
focus on significantly expanding its capabilities. Potential upcoming milestones include:

- **Audio Integration:** Implementing audio waveform and spectrum visualizations, related audio tooling.
- **Graphical Tools:** Tools for visually manipulating subtitles on the video.

<h2 align="center">Development</h2>

### Bob the Builder

I would strongly recommend using Jetbrains Rider or Visual Studio for development.

#### C#

- Make sure you have the .NET SDK installed.
- Run `dotnet restore` to collect required NuGet packages.
- To build, either click the `Build` button in your IDE, or run `dotnet build`.
- To test, either click the `Run Tests` button in your IDE, or run `dotnet test`.
- To build a release binary, use `dotnet publish`.
- The final output for debugging and running is the `Ameko` project.

#### Zig

- Make sure you have Zig installed.
- You may need to build FFMS2 and libass yourself.
- To build, run `zig build`. To run, use `zig build run`.
- To test, run `zig test`.
- To build a release binary, use `zig build --release=safe`.

### Project Components

I wasn't sure where to put this section, so under "development" it goes! The Ameko project is currently comprised of 4
components working in tandem.

- **AssCS:** The backbone of the operation. AssCS is responsible for everything involving the subtitle document itself.
  Managing events and styles, parsing tags, and reading/writing files are just part of what AssCS does. Eventually,
  AssCS will likely be split into its own project so anyone can use it for their C# projects.
- **Holo:** The middleware layer, primarily linking the GUI to AssCS and Mizuki. It also manages the Package Manager,
  projects, configuration, and pretty much everything that's not immediately GUI-related.
- **Mizuki:** A high-performance interop library and Holo's first A/V plugin. Mizuki facilitates communication between
  Holo and A/V libraries like FFMS and libass. By doing most of the work in a low-level language like Zig, Mizuki is
  able to reduce the amount of calls across the managed-unmanaged border.
- **Ameko:** Despite being the namesake of the project, effort has been made to make Ameko a thin GUI. Theoretically,
  one should be able to build their own GUI and plug it right into Holo. Ameko's primary purpose is to facilitate data
  transfer between the user and Holo.

<h2 align="center">Contributing</h2>

Thank you for your interest in contributing to Ameko! Whether you're reporting bugs, fixing bugs, adding features, or
starting a discussion, all contributions are welcome and appreciated.

### Code

Before submitting a pull request, please make sure your code is properly formatted:

- **C# code** is automatically formatted using [CSharpier](https://github.com/belav/csharpier) as part of the build
  process.
- **Zig code** should be formatted using `zig fmt`.

Additionally, there are some testing guidelines:

- Unit tests are required for contributions to the Holo and AssCS projects.
- Tests are optional, but highly appreciated, for Ameko's ViewModels and for Mizuki.

### Localization

If you are interested in localizing Ameko into your language (Thank you!), please see
the [Crowdin project](https://crowdin.com/project/ameko). Note: Ameko is localized via the ResX format,
which is quite limited, such as lacking support for plurals.

<h2 align="center">Licensing</h2>

- The Ameko application is licensed under the GNU GPL v3 license.
- Libraries developed for Ameko are licensed under the Mozilla Public License 2.0.
- For more information, see the LICENSE files.