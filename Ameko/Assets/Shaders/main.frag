// Version header will be injected by the shader loader based on the OpenGL version provided by the platform.
#ifdef GL_ES
precision highp float;
precision highp int;
#endif

in vec4 v_Color;
out vec4 fragColor;

void main()
{
    fragColor = v_Color;
}