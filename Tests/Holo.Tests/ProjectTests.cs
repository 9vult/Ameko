// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using AssCS;
using Holo.Models;
using Shouldly;
using static Holo.Tests.Utilities.TestUtils;

namespace Holo.Tests;

public class ProjectTests
{
    [Fact]
    public void Constructor()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        sln.LoadedWorkspaces.Count.ShouldBe(1);
        sln.WorkingSpace?.Id.ShouldBe(sln.LoadedWorkspaces[0].Id);
    }

    [Fact]
    public void AddWorkspace_New()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var workspaceId = sln.AddWorkspace().Id;

        sln.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        sln.WorkingSpace?.Id.ShouldBe(workspaceId);
    }

    [Fact]
    public void AddWorkspace_Existing()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var workspace = new Workspace(new Document(true), 123);
        var workspaceId = sln.AddWorkspace(workspace).Id;

        sln.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        sln.WorkingSpace?.Id.ShouldBe(workspaceId);
    }

    [Fact]
    public void AddWorkspace_FromExistingDocument()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var document = new Document(true);
        var workspaceId = sln.AddWorkspace(document, MakeTestableUri(fs, "test.ass")).Id;

        sln.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        sln.WorkingSpace?.Id.ShouldBe(workspaceId);
    }

    [Fact]
    public void RemoveWorkspace_Exists()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var workspaceId = sln.AddWorkspace().Id;

        var result = sln.RemoveWorkspace(workspaceId);

        result.ShouldBeTrue();
        sln.LoadedWorkspaces.ShouldNotContain(w => w.Id == workspaceId);
    }

    [Fact]
    public void RemoveWorkspace_NotExists()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var result = sln.RemoveWorkspace(999);

        result.ShouldBeFalse();
    }

    [Fact]
    public void AddDirectory_Root()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var dir = sln.AddDirectory("Directory1");

        sln.ReferencedItems.ShouldContain(dir);
    }

    [Fact]
    public void AddDirectory_Child()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var dir1 = sln.AddDirectory("Directory1");
        var dir2 = sln.AddDirectory("Directory2", dir1.Id);

        sln.ReferencedItems.ShouldContain(dir1);
        dir1.Children.ShouldContain(dir2);
    }

    [Fact]
    public void RemoveDirectory_Root()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
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
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var dir = sln.AddDirectory("Directory1");

        sln.ReferencedItems.ShouldContain(dir);

        sln.RemoveDirectory(dir.Id);
        sln.ReferencedItems.ShouldNotContain(dir);
    }

    [Fact]
    public void AddWorkspace_ToDirectory()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var dir = sln.AddDirectory("Directory1");

        var workspaceId = sln.AddWorkspace(dir.Id).Id;

        dir.Children.ShouldContain(w => w.Id == workspaceId);
        sln.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        sln.WorkingSpace?.Id.ShouldBe(workspaceId);
    }

    [Fact]
    public void OpenDocument_NotExists()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var result = sln.OpenDocument(999);

        result.ShouldBe(-1);
    }

    [Fact]
    public void CloseDocument_Exists()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var docId = sln.AddWorkspace().Id;

        var result = sln.CloseDocument(docId);

        result.ShouldBeTrue();
        sln.LoadedWorkspaces.ShouldNotContain(d => d.Id == docId);
    }

    [Fact]
    public void CloseDocument_NotExists()
    {
        var fs = new MockFileSystem();
        var sln = new Project(fs);
        var result = sln.CloseDocument(999);

        result.ShouldBeFalse();
    }

    [Fact]
    public void Save()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.aproj");

        var sln = new Project(fs) { SavePath = path };

        var result = sln.Save();

        result.ShouldBeTrue();
        fs.FileExists(path.LocalPath).ShouldBeTrue();
    }

    [Fact]
    public void Parse()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.aproj");
        fs.AddFile(path.LocalPath, new MockFileData(ExampleProject));

        var sln = Project.Parse(fs, path);

        sln.Cps.ShouldBe(21);
        sln.UseSoftLinebreaks.ShouldBeNull();
        sln.ReferencedItems.Count.ShouldBe(1);
        sln.StyleManager.Styles.Count.ShouldBe(0);
    }

    [Fact]
    public void LoadDirectory_NotEmpty()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(MakeTestableUri(fs, "test/01").LocalPath);
        fs.AddDirectory(MakeTestableUri(fs, "test/02").LocalPath);
        fs.AddDirectory(MakeTestableUri(fs, "test/03").LocalPath);
        fs.AddFile(MakeTestableUri(fs, "test/a.ass").LocalPath, new MockFileData(string.Empty));
        fs.AddFile(MakeTestableUri(fs, "test/01/b.ass").LocalPath, new MockFileData(string.Empty));
        fs.AddFile(MakeTestableUri(fs, "test/01/d.ass").LocalPath, new MockFileData(string.Empty));
        fs.AddFile(MakeTestableUri(fs, "test/03/d.ass").LocalPath, new MockFileData(string.Empty));

        var sln = Project.LoadDirectory(fs, MakeTestableUri(fs, "test/"));

        sln.ReferencedItems.Count.ShouldBe(3);
        sln.ReferencedItems.First(i => i.Type == ProjectItemType.Directory)
            .Children.Count.ShouldBe(2);
        sln.ReferencedItems.Last(i => i.Type == ProjectItemType.Directory)
            .Children.Count.ShouldBe(1);
    }

    [Fact]
    public void LoadDirectory_Empty()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(MakeTestableUri(fs, "test/01").LocalPath);
        fs.AddDirectory(MakeTestableUri(fs, "test/02").LocalPath);
        fs.AddDirectory(MakeTestableUri(fs, "test/03").LocalPath);

        var sln = Project.LoadDirectory(fs, MakeTestableUri(fs, "test/"));

        sln.ReferencedItems.Count.ShouldBe(1);
        sln.ReferencedItems.First().Type.ShouldBe(ProjectItemType.Document); // default document
    }

    [Fact]
    public void LoadDirectory_NotExists()
    {
        var fs = new MockFileSystem();
        var sln = Project.LoadDirectory(fs, MakeTestableUri(fs, "test/"));

        sln.ReferencedItems.Count.ShouldBe(1);
        sln.ReferencedItems.First().Type.ShouldBe(ProjectItemType.Document); // default document
    }

    private const string ExampleProject = """
         {
             "Version": 1,
             "ReferencedDocuments": [],
             "Styles": [],
             "Cps": 21,
             "CpsIncludesWhitespace": null,
             "CpsIncludesPunctuation": null,
             "UseSoftLinebreaks": null,
             "SpellcheckCulture": null,
             "CustomWords": []
         }
        """;
}
