// Version header will be injected by the shader loader based on the OpenGL version provided by the platform.
#ifdef GL_ES
precision highp float;
precision highp int;
#endif

in vec3 aPosition;
in vec2 aTexCoord;

out vec2 texCoord;

void main(void)
{
    texCoord = aTexCoord;
    texCoord.y = 1.0 - texCoord.y; // Flip image
    gl_Position = vec4(aPosition, 1.0);
}