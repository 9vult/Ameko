// Version header will be injected by the shader loader based on the OpenGL version provided by the platform.
#ifdef GL_ES
precision highp float;
precision highp int;
#endif

in vec2 texCoord;
out vec4 outputColor;

uniform sampler2D texture0;
uniform sampler2D texture1;

void main()
{
    vec4 videoColor = texture(texture0, texCoord);
    vec4 subsColor = texture(texture1, texCoord);

    // Premultiplied alpha blending
    vec3 blendedRGB = subsColor.rgb + videoColor.rgb * (1.0 - subsColor.a);

    // Output fully opaque
    outputColor = vec4(blendedRGB, 1.0);
}