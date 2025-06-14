# Mizuki

This library is intended to serve as a high-performance interface between Ameko and rendering libraries (ffms, libass,
etc.)

Mizuki is intended to do the compositing workload and potentially playback control / frame-timing if it makes sense to
do so. Compositing in this case means requesting a frame from the video source provider (currently ffms, will probably
be expanded in the future), drawing the subtitle data provided by libass onto it, and serving the completed frame to
Ameko.

If playback control is included, then Ameko will send the command to Mizuki (e.g. "Play"), and Mizuki will handle the
frame timer and related duties.