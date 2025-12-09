// SPDX-License-Identifier: GPL-3.0-only

using System;
using Ameko.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Ameko.Utilities;

public interface IViewModelFactory
{
    /// <summary>
    /// Create a ViewModel with the arguments
    /// </summary>
    /// <param name="args">Arguments for building the ViewModel</param>
    /// <typeparam name="T">Type of ViewModel to create</typeparam>
    /// <returns>Instantiated ViewModel</returns>
    public T Create<T>(params object[] args)
        where T : ViewModelBase;
}

public class ViewModelFactory(IServiceProvider serviceProvider) : IViewModelFactory
{
    /// <inheritdoc />
    public T Create<T>(params object[] args)
        where T : ViewModelBase
    {
        return ActivatorUtilities.CreateInstance<T>(serviceProvider, args);
    }
}
