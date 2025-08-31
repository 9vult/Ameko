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

    /// <summary>
    /// Throw if the <see cref="ALContext"/> instance is in an error state
    /// </summary>
    /// <param name="al">OpenAL Context</param>
    /// <param name="device">OpenAL Device</param>
    /// <exception cref="OpenAlException">If there is an error</exception>
    public static unsafe void ThrowIfError(ALContext al, Device* device)
    {
        var error = al.GetError(device);
        if (error != ContextError.NoError)
        {
            throw new OpenAlException(error.ToString());
        }
    }
}
