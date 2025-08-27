// SPDX-License-Identifier: GPL-3.0-only

using System;
using Silk.NET.OpenAL;

namespace Ameko.DataModels.OpenAl;

public class OpenAlException : Exception
{
    public OpenAlException() { }

    public OpenAlException(string message)
        : base(message) { }

    /// <summary>
    /// Throw if the <see cref="AL"/> instance is in an error state
    /// </summary>
    /// <param name="al">OpenAL instance</param>
    /// <exception cref="OpenAlException">If there is an error</exception>
    public static void ThrowIfError(AL al)
    {
        var error = al.GetError();
        if (error != AudioError.NoError)
        {
            throw new OpenAlException(error.ToString());
        }
    }
}
