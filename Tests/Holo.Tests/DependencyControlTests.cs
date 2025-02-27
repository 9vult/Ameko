// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using Holo.Scripting;
using Shouldly;

namespace Holo.Tests;

public class DependencyControlTests
{
    [Fact]
    public void IsModuleInstalled_True()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { DependencyControl.ModulePath("author.test"), new MockFileData(string.Empty) },
            }
        );

        var dc = new DependencyControl(fileSystem);

        dc.IsModuleInstalled("author.test").ShouldBeTrue();
    }

    [Fact]
    public void IsModuleInstalled_False()
    {
        var fileSystem = new MockFileSystem();

        var dc = new DependencyControl(fileSystem);

        dc.IsModuleInstalled("author.test").ShouldBeFalse();
    }
}
