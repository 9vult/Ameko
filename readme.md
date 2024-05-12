<h1 align="center">Ameko</h1>

<img alt="Ameko Banner Logo" src="https://files.catbox.moe/aylq3a.jpg" />

<br />

Ameko is a subtitle editing suite for Advanced Substation Alpha (ASS) subtitles.

<h2 align="center">Roadmap</h1>

### Milestone 1 - MVP

The goal for Milestone 1 is to have a Minimum  Viable Product (MVP). In Ameko's case, the MVP is delivering a feature-rich *subtitle editor*. Note that Ameko intends to be an editing *suite* in its final form. Thus, the MVP is missing some pretty major features one might consider invaluable to the subtitle workflow, such as audio, video, graphical tools, etc.

However, as Milestone 1 is serving mostly as a proof-of-concept for the viability of the Ameko-Holo-AssCS pipeline, a fully-featured subtitle editor will play the role of the minimuim product. The full list of MVP features can be found under the MVP milestone issue filter, but the gist is that it should include all major editing features that don't require audio or video as part of the workflow.

### Future Milestones

As Ameko is still early in the development cycle, the exact nature of post-MVP milestones is still unclear. Potential milestones include Audio, Video, and Tooling, but these could be merged or split into fewer or more milestones as the scope discovery process continues.

<h2 align="center">Development</h1>

I would strongly recommend using either Visual Studio or Jetbrains Rider for development. You _can_ use VSCode and other text editors, but you will miss out on the visual designer and other goodies that come with the full-fledged IDEs.

- As a prerequisite, ensure all NuGet packages are installed.
- To build, either click the `Build` button in your IDE, or run `dotnet build` in the project root directory.
- The final output for debugging and running is the `Ameko.Desktop` project
- Holo and Ameko follow XDG directory guidelines

- Ameko and Ameko.Desktop use `.NET 8.0`
- AssCS and Holo use `.NET Standard 2.1`