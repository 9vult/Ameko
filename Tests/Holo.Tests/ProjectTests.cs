// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AssCS;
using Holo.Models;
using Holo.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TestingUtils;
using static TestingUtils.TestableUri;

namespace Holo.Tests;

public class ProjectTests
{
    private readonly IWorkspaceFactory _workspaceFactory = Substitute.For<IWorkspaceFactory>();

    public ProjectTests()
    {
        _workspaceFactory
            .Create(Arg.Any<Document>(), Arg.Any<int>())
            .Returns(args => new Workspace(
                args.Arg<Document>(),
                args.Arg<int>(),
                null,
                NullLogger<Workspace>.Instance,
                new MediaController(new NullSourceProvider(), NullLogger<MediaController>.Instance)
            ));

        _workspaceFactory
            .Create(Arg.Any<Document>(), Arg.Any<int>(), Arg.Any<Uri?>())
            .Returns(args => new Workspace(
                args.Arg<Document>(),
                args.Arg<int>(),
                args.Arg<Uri?>(),
                NullLogger<Workspace>.Instance,
                new MediaController(new NullSourceProvider(), NullLogger<MediaController>.Instance)
            ));
    }

    private Project CreateProject()
    {
        return new Project(new MockFileSystem(), NullLogger<Project>.Instance, _workspaceFactory);
    }

    private Project CreateProject(IFileSystem fs)
    {
        return new Project(fs, NullLogger<Project>.Instance, _workspaceFactory);
    }

    private Project CreateProject(IFileSystem fs, Uri path)
    {
        return new Project(fs, NullLogger<Project>.Instance, _workspaceFactory, path);
    }

    private static Workspace CreateWorkspace(Document document, int id)
    {
        return new Workspace(
            document,
            id,
            null,
            NullLogger<Workspace>.Instance,
            new MediaController(new NullSourceProvider(), NullLogger<MediaController>.Instance)
        );
    }

    [Test]
    public async Task Constructor()
    {
        var prj = CreateProject();
        await Assert.That(prj.LoadedWorkspaces.Count).IsEqualTo(1);
        await Assert.That(prj.WorkingSpace?.Id).IsEqualTo(prj.LoadedWorkspaces[0].Id);
    }

    [Test]
    public async Task AddWorkspace_New()
    {
        var prj = CreateProject();
        var workspaceId = prj.AddWorkspace().Id;

        await Assert.That(prj.LoadedWorkspaces).Contains(w => w.Id == workspaceId);
        await Assert.That(prj.WorkingSpace?.Id).IsEqualTo(workspaceId);
    }

    [Test]
    public async Task AddWorkspace_Existing()
    {
        var prj = CreateProject();
        var workspace = CreateWorkspace(new Document(true), 123);
        var workspaceId = prj.AddWorkspace(workspace).Id;

        await Assert.That(prj.LoadedWorkspaces).Contains(w => w.Id == workspaceId);
        await Assert.That(prj.WorkingSpace?.Id).IsEqualTo(workspaceId);
    }

    [Test]
    public async Task AddWorkspace_FromExistingDocument()
    {
        var fs = new MockFileSystem();
        var prj = CreateProject(fs);
        var document = new Document(true);
        var workspaceId = prj.AddWorkspace(document, MakeTestableUri(fs, "test.ass")).Id;

        await Assert.That(prj.LoadedWorkspaces).Contains(w => w.Id == workspaceId);
        await Assert.That(prj.WorkingSpace?.Id).IsEqualTo(workspaceId);
    }

    [Test]
    public async Task RemoveWorkspace_Exists()
    {
        var prj = CreateProject();
        var workspaceId = prj.AddWorkspace().Id;

        var result = prj.RemoveWorkspace(workspaceId);

        await Assert.That(result).IsTrue();
        await Assert.That(prj.LoadedWorkspaces).DoesNotContain(w => w.Id == workspaceId);
    }

    [Test]
    public async Task RemoveWorkspace_NotExists()
    {
        var prj = CreateProject();
        var result = prj.RemoveWorkspace(999);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task AddDirectory_Root()
    {
        var prj = CreateProject();
        var dir = prj.AddDirectory("Directory1");

        await Assert.That(prj.ReferencedItems).Contains(dir);
    }

    [Test]
    public async Task AddDirectory_Child()
    {
        var prj = CreateProject();
        var dir1 = prj.AddDirectory("Directory1");
        var dir2 = prj.AddDirectory("Directory2", dir1.Id);

        await Assert.That(prj.ReferencedItems).Contains(dir1);
        await Assert.That(dir1.Children).Contains(dir2);
    }

    [Test]
    public async Task RemoveDirectory_Root()
    {
        var prj = CreateProject();
        var dir1 = prj.AddDirectory("Directory1");
        var dir2 = prj.AddDirectory("Directory2", dir1.Id);

        await Assert.That(prj.ReferencedItems).Contains(dir1);
        await Assert.That(dir1.Children).Contains(dir2);

        prj.RemoveDirectory(dir2.Id);

        await Assert.That(prj.ReferencedItems).Contains(dir1);
        await Assert.That(dir1.Children).DoesNotContain(dir2);
    }

    [Test]
    public async Task RemoveDirectory_Child()
    {
        var prj = CreateProject();
        var dir = prj.AddDirectory("Directory1");

        await Assert.That(prj.ReferencedItems).Contains(dir);

        prj.RemoveDirectory(dir.Id);
        await Assert.That(prj.ReferencedItems).DoesNotContain(dir);
    }

    [Test]
    public async Task AddWorkspace_ToDirectory()
    {
        var prj = CreateProject();
        var dir = prj.AddDirectory("Directory1");

        var workspaceId = prj.AddWorkspace(dir.Id).Id;

        await Assert.That(dir.Children).Contains(w => w.Id == workspaceId);
        await Assert.That(prj.LoadedWorkspaces).Contains(w => w.Id == workspaceId);
        await Assert.That(prj.WorkingSpace?.Id).IsEqualTo(workspaceId);
    }

    [Test]
    public async Task OpenDocument_NotExists()
    {
        var prj = CreateProject();
        var result = prj.OpenDocument(999);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task CloseDocument_Exists()
    {
        var prj = CreateProject();
        var docId = prj.AddWorkspace().Id;

        var result = prj.CloseDocument(docId);

        await Assert.That(result).IsTrue();
        await Assert.That(prj.LoadedWorkspaces).DoesNotContain(d => d.Id == docId);
    }

    [Test]
    public async Task CloseDocument_NotExists()
    {
        var prj = CreateProject();
        var result = prj.CloseDocument(999);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task Save()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.aproj");

        var prj = CreateProject(fs);
        prj.SavePath = path;

        var result = prj.Save();

        await Assert.That(result).IsTrue();
        await Assert.That(fs.FileExists(path.LocalPath)).IsTrue();
    }

    [Test]
    public async Task Parse()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.aproj");
        fs.AddFile(path.LocalPath, new MockFileData(ExampleProject));

        var prj = CreateProject(fs, path);

        await Assert.That(prj.Cps).IsEqualTo<uint?>(21);
        await Assert.That(prj.UseSoftLinebreaks).IsNull();
        await Assert.That(prj.ReferencedItems.Count).IsEqualTo(1);
        await Assert.That(prj.StyleManager.Styles.Count).IsEqualTo(0);
    }

    [Test]
    public async Task LoadDirectory_NotEmpty()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(MakeTestableUri(fs, "test/01").LocalPath);
        fs.AddDirectory(MakeTestableUri(fs, "test/02").LocalPath);
        fs.AddDirectory(MakeTestableUri(fs, "test/03").LocalPath);
        fs.AddFile(MakeTestableUri(fs, "test/a.ass").LocalPath, new MockFileData(string.Empty));
        fs.AddFile(MakeTestableUri(fs, "test/01/b.ass").LocalPath, new MockFileData(string.Empty));
        fs.AddFile(MakeTestableUri(fs, "test/01/d.ass").LocalPath, new MockFileData(string.Empty));
        fs.AddFile(MakeTestableUri(fs, "test/03/d.ass").LocalPath, new MockFileData(string.Empty));

        var prj = CreateProject(fs, MakeTestableUri(fs, "test/"));

        await Assert.That(prj.ReferencedItems.Count).IsEqualTo(3);
        await Assert
            .That(
                prj.ReferencedItems.First(i => i.Type == ProjectItemType.Directory).Children.Count
            )
            .IsEqualTo(2);
        await Assert
            .That(prj.ReferencedItems.Last(i => i.Type == ProjectItemType.Directory).Children.Count)
            .IsEqualTo(1);
    }

    [Test]
    public async Task LoadDirectory_Empty()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(MakeTestableUri(fs, "test/01").LocalPath);
        fs.AddDirectory(MakeTestableUri(fs, "test/02").LocalPath);
        fs.AddDirectory(MakeTestableUri(fs, "test/03").LocalPath);

        var prj = CreateProject(fs, MakeTestableUri(fs, "test/"));

        await Assert.That(prj.ReferencedItems.Count).IsEqualTo(1);
        await Assert.That(prj.ReferencedItems.First().Type).IsEqualTo(ProjectItemType.Document); // default document
    }

    [Test]
    public async Task LoadDirectory_NotExists()
    {
        var fs = new MockFileSystem();
        var prj = CreateProject(fs, MakeTestableUri(fs, "test/"));

        await Assert.That(prj.ReferencedItems.Count).IsEqualTo(1);
        await Assert.That(prj.ReferencedItems.First().Type).IsEqualTo(ProjectItemType.Document); // default document
    }

    private const string ExampleProject = """
         {
             "Version": 1,
             "ReferencedDocuments": [],
             "Styles": [],
             "Cps": 21,
             "DefaultLayer": 0,
             "CpsIncludesWhitespace": null,
             "CpsIncludesPunctuation": null,
             "UseSoftLinebreaks": null,
             "SpellcheckCulture": null,
             "CustomWords": []
         }
        """;
}
