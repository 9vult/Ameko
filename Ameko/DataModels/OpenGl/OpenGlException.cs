// SPDX-License-Identifier: GPL-3.0-only

using System;
using Silk.NET.OpenGLES;

namespace Ameko.DataModels.OpenGl;

public class OpenGlException : Exception
{
    public OpenGlException() { }

    public OpenGlException(string message)
        : base(message) { }

    /// <summary>
    /// Throw if the <see cref="GL"/> instance is in an error state
    /// </summary>
    /// <param name="gl">OpenGL instance</param>
    /// <exception cref="OpenGlException">If there is an error</exception>
    public static void ThrowIfError(GL gl)
    {
        var error = gl.GetError();
        if (error != GLEnum.NoError)
        {
            throw new OpenGlException(error.ToString());
        }
    }
}
