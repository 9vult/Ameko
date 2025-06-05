// SPDX-License-Identifier: MPL-2.0

using AssCS;
using Shouldly;

namespace Holo.Tests;

public class SolutionTests
{
    [Fact]
    public void Constructor()
    {
        var sln = new Solution();
        sln.LoadedWorkspaces.Count.ShouldBe(1);
        sln.WorkingSpaceId.ShouldBe(sln.LoadedWorkspaces[0].Id);
    }

    [Fact]
    public void AddWorkspace_New()
    {
        var sln = new Solution();
        var workspaceId = sln.AddWorkspace().Id;

        sln.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        sln.WorkingSpaceId.ShouldBe(workspaceId);
    }

    [Fact]
    public void AddWorkspace_Existing()
    {
        var sln = new Solution();
        var workspace = new Workspace(new Document(true), 123);
        var workspaceId = sln.AddWorkspace(workspace).Id;

        sln.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        sln.WorkingSpaceId.ShouldBe(workspaceId);
    }

    [Fact]
    public void AddWorkspace_FromExistingDocument()
    {
        var sln = new Solution();
        var document = new Document(true);
        var workspaceId = sln.AddWorkspace(document, new Uri("file:///test.ass")).Id;

        sln.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        sln.WorkingSpaceId.ShouldBe(workspaceId);
    }

    [Fact]
    public void RemoveWorkspace_Exists()
    {
        var sln = new Solution();
        var workspaceId = sln.AddWorkspace().Id;

        var result = sln.RemoveWorkspace(workspaceId);

        result.ShouldBeTrue();
        sln.LoadedWorkspaces.ShouldNotContain(w => w.Id == workspaceId);
    }

    [Fact]
    public void RemoveWorkspace_NotExists()
    {
        var sln = new Solution();
        var result = sln.RemoveWorkspace(999);

        result.ShouldBeFalse();
    }

    [Fact]
    public void AddDirectory_Root()
    {
        var sln = new Solution();
        var dir = sln.AddDirectory("Directory1");

        sln.ReferencedItems.ShouldContain(dir);
    }

    [Fact]
    public void AddDirectory_Child()
    {
        var sln = new Solution();
        var dir1 = sln.AddDirectory("Directory1");
        var dir2 = sln.AddDirectory("Directory2", dir1.Id);

        sln.ReferencedItems.ShouldContain(dir1);
        dir1.Children.ShouldContain(dir2);
    }

    [Fact]
    public void RemoveDirectory_Root()
    {
        var sln = new Solution();
        var dir1 = sln.AddDirectory("Directory1");
        var dir2 = sln.AddDirectory("Directory2", dir1.Id);

        sln.ReferencedItems.ShouldContain(dir1);
        dir1.Children.ShouldContain(dir2);

        sln.RemoveDirectory(dir2.Id);

        sln.ReferencedItems.ShouldContain(dir1);
        dir1.Children.ShouldNotContain(dir2);
    }

    [Fact]
    public void RemoveDirectory_Child()
    {
        var sln = new Solution();
        var dir = sln.AddDirectory("Directory1");

        sln.ReferencedItems.ShouldContain(dir);

        sln.RemoveDirectory(dir.Id);
        sln.ReferencedItems.ShouldNotContain(dir);
    }

    [Fact]
    public void AddWorkspace_ToDirectory()
    {
        var sln = new Solution();
        var dir = sln.AddDirectory("Directory1");

        var workspaceId = sln.AddWorkspace(dir.Id).Id;

        dir.Children.ShouldContain(w => w.Id == workspaceId);
        sln.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        sln.WorkingSpaceId.ShouldBe(workspaceId);
    }

    [Fact]
    public void OpenDocument_NotExists()
    {
        var sln = new Solution();
        var result = sln.OpenDocument(999);

        result.ShouldBe(-1);
    }

    [Fact]
    public void CloseDocument_Exists()
    {
        var sln = new Solution();
        var docId = sln.AddWorkspace().Id;

        var result = sln.CloseDocument(docId);

        result.ShouldBeTrue();
        sln.LoadedWorkspaces.ShouldNotContain(d => d.Id == docId);
    }

    [Fact]
    public void CloseDocument_NotExists()
    {
        var sln = new Solution();
        var result = sln.CloseDocument(999);

        result.ShouldBeFalse();
    }

    [Fact]
    public void Save()
    {
        var sln = new Solution { SavePath = new Uri("file:///test.asln") };

        var writer = new StringWriter();
        var result = sln.Save(writer, sln.SavePath);

        result.ShouldBeTrue();
        writer.ToString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Parse()
    {
        var sr = new StringReader(ExampleSolution);
        var sln = Solution.Parse(sr, new Uri("file:///test.asln"));

        sln.Cps.ShouldBe(21);
        sln.UseSoftLinebreaks.ShouldBeNull();
        sln.ReferencedItems.Count.ShouldBe(1);
        sln.StyleManager.Styles.Count.ShouldBe(0);
    }

    private const string ExampleSolution = """
         {
             "Version": 1,
             "ReferencedDocuments": [],
             "Styles": [],
             "Cps": 21,
             "CpsIncludesWhitespace": null,
             "CpsIncludesPunctuation": null,
             "UseSoftLinebreaks": null
         }
        """;
}
