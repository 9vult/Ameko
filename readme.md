<h1 align="center">Ameko</h1>

<img alt="Ameko Banner Logo" src="https://files.catbox.moe/aylq3a.jpg" />

<br />

Ameko is a subtitle editing suite for Advanced Substation Alpha (ASS) subtitles.

<h2 align="center">Roadmap</h1>

### Milestone 1 - MVP

The goal for Milestone 1 is to have a Minimum  Viable Product (MVP). In Ameko's case, the MVP is delivering a feature-rich *subtitle editor*. Note that Ameko intends to be an editing *suite* in its final form. Thus, the MVP is missing some pretty major features one might consider invaluable to the subtitle workflow, such as audio, video, graphical tools, etc.

### Milestone 2 - Video

Video is clearly the second most important feature for a subtitle editor (behind only subtitle editing features). Video loading will be provided via Plugins so Holo/Ameko is not dependent on any one source. The first video provider will be [FFMS2](https://github.com/FFMS/ffms2). The first subtitle provider will be [libass](https://github.com/libass/libass/).

### Future Milestones

As Ameko is still early in the development cycle, the exact nature of upcoming  milestones are still unclear. Potential milestones include Audio and Tooling, but these could be merged or split into fewer or more milestones as the scope discovery process continues.

<h2 align="center">Development</h1>

I would strongly recommend using either Visual Studio or Jetbrains Rider for development. 

- As a prerequisite, ensure all NuGet packages are installed.
- To build, either click the `Build` button in your IDE, or run `dotnet build` in the project root directory.
- The final output for debugging and running is the `Ameko` project
- Holo and Ameko follow XDG directory guidelines

- Ameko uses `.NET 9.0`

<h2 align="center">Licensing</h1>

- The Ameko application is licensed under the GNU GPL v3 license.
- Libraries developed for Ameko are licensed under the Mozilla Public License 2.0.
- For more information, see the LICENSE files.