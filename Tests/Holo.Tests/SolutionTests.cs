// SPDX-License-Identifier: MPL-2.0

using AssCS;
using FluentAssertions;

namespace Holo.Tests;

public class SolutionTests
{
    [Fact]
    public void Constructor()
    {
        var sln = new Solution();
        sln.LoadedWorkspaces.Should().HaveCount(1);
        sln.WorkingSpaceId.Should().Be(sln.LoadedWorkspaces[0].Id);
    }

    [Fact]
    public void AddWorkspace_New()
    {
        var sln = new Solution();
        int workspaceId = sln.AddWorkspace();

        sln.LoadedWorkspaces.Should().ContainSingle(w => w.Id == workspaceId);
        sln.WorkingSpaceId.Should().Be(workspaceId);
    }

    [Fact]
    public void AddWorkspace_Existing()
    {
        var sln = new Solution();
        var workspace = new Workspace(new Document(true), 123);
        int workspaceId = sln.AddWorkspace(workspace);

        sln.LoadedWorkspaces.Should().ContainSingle(w => w.Id == workspaceId);
        sln.WorkingSpaceId.Should().Be(workspaceId);
    }

    [Fact]
    public void RemoveWorkspace_Exists()
    {
        var sln = new Solution();
        var workspaceId = sln.AddWorkspace();

        var result = sln.RemoveWorkspace(workspaceId);

        result.Should().BeTrue();
        sln.LoadedWorkspaces.Should().NotContain(w => w.Id == workspaceId);
    }

    [Fact]
    public void RemoveWorkspace_NotExists()
    {
        var sln = new Solution();
        var result = sln.RemoveWorkspace(999);

        result.Should().BeFalse();
    }

    [Fact]
    public void OpenDocument_NotExists()
    {
        var sln = new Solution();
        int result = sln.OpenDocument(999);

        result.Should().Be(-1);
    }

    [Fact]
    public void CloseDocument_Exists()
    {
        var sln = new Solution();
        int docId = sln.AddWorkspace();

        bool result = sln.CloseDocument(docId);

        result.Should().BeTrue();
        sln.LoadedWorkspaces.Should().NotContain(d => d.Id == docId);
    }

    [Fact]
    public void CloseDocument_NotExists()
    {
        var sln = new Solution();
        bool result = sln.CloseDocument(999);

        result.Should().BeFalse();
    }

    [Fact]
    public void Save()
    {
        var sln = new Solution { SavePath = new Uri("file:///test.asln") };

        var writer = new StringWriter();
        bool result = sln.Save(writer, sln.SavePath);

        result.Should().BeTrue();
        writer.ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Parse()
    {
        const string input =
            "{\"Version\":1,\"ReferencedDocuments\":[],\"Styles\":[],\"Cps\":21,\"UseSoftLinebreaks\":null}";

        var sr = new StringReader(input);
        var sln = Solution.Parse(sr, new Uri("file:///test.asln"));

        sln.Cps.Should().Be(21);
        sln.UseSoftLinebreaks.Should().BeNull();
        sln.ReferencedDocuments.Count.Should().Be(1);
        sln.StyleManager.Styles.Count.Should().Be(0);
    }
}
