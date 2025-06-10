// SPDX-License-Identifier: MPL-2.0

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

namespace Ameko.ViewModel.Tests;

public class AutoNSubstituteDataAttribute()
    : AutoDataAttribute(() =>
    {
        var fixture = new Fixture().Customize(
            new AutoNSubstituteCustomization { ConfigureMembers = true }
        );
        return fixture;
    });
