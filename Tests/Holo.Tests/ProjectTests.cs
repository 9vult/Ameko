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
        var prj = new Project(fs);
        prj.LoadedWorkspaces.Count.ShouldBe(1);
        prj.WorkingSpace?.Id.ShouldBe(prj.LoadedWorkspaces[0].Id);
    }

    [Fact]
    public void AddWorkspace_New()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var workspaceId = prj.AddWorkspace().Id;

        prj.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        prj.WorkingSpace?.Id.ShouldBe(workspaceId);
    }

    [Fact]
    public void AddWorkspace_Existing()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var workspace = new Workspace(new Document(true), 123);
        var workspaceId = prj.AddWorkspace(workspace).Id;

        prj.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        prj.WorkingSpace?.Id.ShouldBe(workspaceId);
    }

    [Fact]
    public void AddWorkspace_FromExistingDocument()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var document = new Document(true);
        var workspaceId = prj.AddWorkspace(document, MakeTestableUri(fs, "test.ass")).Id;

        prj.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        prj.WorkingSpace?.Id.ShouldBe(workspaceId);
    }

    [Fact]
    public void RemoveWorkspace_Exists()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var workspaceId = prj.AddWorkspace().Id;

        var result = prj.RemoveWorkspace(workspaceId);

        result.ShouldBeTrue();
        prj.LoadedWorkspaces.ShouldNotContain(w => w.Id == workspaceId);
    }

    [Fact]
    public void RemoveWorkspace_NotExists()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var result = prj.RemoveWorkspace(999);

        result.ShouldBeFalse();
    }

    [Fact]
    public void AddDirectory_Root()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var dir = prj.AddDirectory("Directory1");

        prj.ReferencedItems.ShouldContain(dir);
    }

    [Fact]
    public void AddDirectory_Child()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var dir1 = prj.AddDirectory("Directory1");
        var dir2 = prj.AddDirectory("Directory2", dir1.Id);

        prj.ReferencedItems.ShouldContain(dir1);
        dir1.Children.ShouldContain(dir2);
    }

    [Fact]
    public void RemoveDirectory_Root()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var dir1 = prj.AddDirectory("Directory1");
        var dir2 = prj.AddDirectory("Directory2", dir1.Id);

        prj.ReferencedItems.ShouldContain(dir1);
        dir1.Children.ShouldContain(dir2);

        prj.RemoveDirectory(dir2.Id);

        prj.ReferencedItems.ShouldContain(dir1);
        dir1.Children.ShouldNotContain(dir2);
    }

    [Fact]
    public void RemoveDirectory_Child()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var dir = prj.AddDirectory("Directory1");

        prj.ReferencedItems.ShouldContain(dir);

        prj.RemoveDirectory(dir.Id);
        prj.ReferencedItems.ShouldNotContain(dir);
    }

    [Fact]
    public void AddWorkspace_ToDirectory()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var dir = prj.AddDirectory("Directory1");

        var workspaceId = prj.AddWorkspace(dir.Id).Id;

        dir.Children.ShouldContain(w => w.Id == workspaceId);
        prj.LoadedWorkspaces.ShouldContain(w => w.Id == workspaceId);
        prj.WorkingSpace?.Id.ShouldBe(workspaceId);
    }

    [Fact]
    public void OpenDocument_NotExists()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var result = prj.OpenDocument(999);

        result.ShouldBe(-1);
    }

    [Fact]
    public void CloseDocument_Exists()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var docId = prj.AddWorkspace().Id;

        var result = prj.CloseDocument(docId);

        result.ShouldBeTrue();
        prj.LoadedWorkspaces.ShouldNotContain(d => d.Id == docId);
    }

    [Fact]
    public void CloseDocument_NotExists()
    {
        var fs = new MockFileSystem();
        var prj = new Project(fs);
        var result = prj.CloseDocument(999);

        result.ShouldBeFalse();
    }

    [Fact]
    public void Save()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.aproj");

        var prj = new Project(fs) { SavePath = path };

        var result = prj.Save();

        result.ShouldBeTrue();
        fs.FileExists(path.LocalPath).ShouldBeTrue();
    }

    [Fact]
    public void Parse()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.aproj");
        fs.AddFile(path.LocalPath, new MockFileData(ExampleProject));

        var prj = Project.Parse(fs, path);

        prj.Cps.ShouldBe<uint?>(21);
        prj.UseSoftLinebreaks.ShouldBeNull();
        prj.ReferencedItems.Count.ShouldBe(1);
        prj.StyleManager.Styles.Count.ShouldBe(0);
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

        var prj = Project.LoadDirectory(fs, MakeTestableUri(fs, "test/"));

        prj.ReferencedItems.Count.ShouldBe(3);
        prj.ReferencedItems.First(i => i.Type == ProjectItemType.Directory)
            .Children.Count.ShouldBe(2);
        prj.ReferencedItems.Last(i => i.Type == ProjectItemType.Directory)
            .Children.Count.ShouldBe(1);
    }

    [Fact]
    public void LoadDirectory_Empty()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(MakeTestableUri(fs, "test/01").LocalPath);
        fs.AddDirectory(MakeTestableUri(fs, "test/02").LocalPath);
        fs.AddDirectory(MakeTestableUri(fs, "test/03").LocalPath);

        var prj = Project.LoadDirectory(fs, MakeTestableUri(fs, "test/"));

        prj.ReferencedItems.Count.ShouldBe(1);
        prj.ReferencedItems.First().Type.ShouldBe(ProjectItemType.Document); // default document
    }

    [Fact]
    public void LoadDirectory_NotExists()
    {
        var fs = new MockFileSystem();
        var prj = Project.LoadDirectory(fs, MakeTestableUri(fs, "test/"));

        prj.ReferencedItems.Count.ShouldBe(1);
        prj.ReferencedItems.First().Type.ShouldBe(ProjectItemType.Document); // default document
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
