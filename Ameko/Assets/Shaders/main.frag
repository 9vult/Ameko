// Version header will be injected by the shader loader based on the OpenGL version provided by the platform.
#ifdef GL_ES
precision highp float;
precision highp int;
#endif

in vec2 texCoord;
out vec4 outputColor;

uniform sampler2D texture0;

void main()
{
    outputColor = texture(texture0, texCoord);
}