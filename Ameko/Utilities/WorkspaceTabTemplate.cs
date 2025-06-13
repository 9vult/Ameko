// SPDX-License-Identifier: GPL-3.0-only

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Holo;
using Microsoft.Extensions.DependencyInjection;
using TabItem = Ameko.Views.Controls.TabItem;

namespace Ameko.Utilities;

public class WorkspaceTabTemplate(IServiceProvider provider) : IDataTemplate
{
    private readonly ITabFactory _tabFactory = provider.GetRequiredService<ITabFactory>();

    public Control Build(object? param)
    {
        if (param is not Workspace workspace)
            throw new ArgumentException("Invalid data type");

        var vm = _tabFactory.Create(workspace);
        return new TabItem { DataContext = vm };
    }

    public bool Match(object? data)
    {
        return data is Workspace;
    }
}
