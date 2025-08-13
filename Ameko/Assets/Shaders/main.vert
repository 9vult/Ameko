// Version header will be injected by the shader loader based on the OpenGL version provided by the platform.
#ifdef GL_ES
precision highp float;
precision highp int;
#endif

layout(location = 0) in vec3 a_Position;
layout(location = 1) in vec4 a_Color;

out vec4 v_Color;

void main()
{
    gl_Position = vec4(a_Position, 1.0);
    v_Color = a_Color;
}