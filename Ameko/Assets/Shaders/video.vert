#version 300 es
in vec3 aPosition;
in vec2 aTexCoord;

out vec2 texCoord;

void main(void)
{
    texCoord = aTexCoord;
    texCoord.y = 1.0 - texCoord.y; // Flip image
    gl_Position = vec4(aPosition, 1.0);
}