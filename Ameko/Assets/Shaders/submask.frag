#version 300 es
precision mediump float;

in vec2 TexCoord;
in vec4 Color;
out vec4 FragColor;

uniform sampler2D uTexture;

void main()
{
    vec4 texColor = texture(uTexture, TexCoord);
    FragColor = texColor * Color; // Combine texture color with vertex color
}
