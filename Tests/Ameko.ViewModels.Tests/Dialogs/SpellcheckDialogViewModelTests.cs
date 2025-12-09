// SPDX-License-Identifier: MPL-2.0

using Ameko.Utilities;
using AssCS;
using Holo;
using Holo.Models;
using Holo.Providers;
using NSubstitute;
using TestingUtils;

namespace Ameko.ViewModels.Dialogs;

public class SpellcheckDialogViewModelTests
{
    private readonly ISpellcheckService _spellcheckService = Substitute.For<ISpellcheckService>();
    private readonly ITabFactory _tabFactory = Substitute.For<ITabFactory>();

    private readonly IWorkspaceFactory _workspaceFactory = new TestWorkspaceFactory();
    private readonly IProjectProvider _projectFactory = new TestProjectFactory();

    private Document _document = null!;
    private Workspace _workspace = null!;
    private Project _project = null!;
    private SpellcheckDialogViewModel _sut = null!;

    private void CreateSut(
        SpellcheckSuggestion[] items1,
        SpellcheckSuggestion[]? items2 = null,
        SpellcheckSuggestion[]? items3 = null
    )
    {
        _spellcheckService.CheckSpelling().Returns(items1, items2 ?? [], items3 ?? []);
        _sut = new SpellcheckDialogViewModel(_projectFactory, _spellcheckService, _tabFactory);
    }

    [Before(Test)]
    public void Setup()
    {
        _document = new Document(true);
        _workspace = _workspaceFactory.Create(_document, 1);
        _project = _projectFactory.Create();

        _project.WorkingSpace = _workspace;
        _projectFactory.Current = _project;

        _spellcheckService.CurrentLanguage.Returns(SpellcheckLanguage.AvailableLanguages[0]);
    }

    [Test]
    public async Task Loads_First_Suggestion_On_Startup()
    {
        CreateSut(
            [
                new SpellcheckSuggestion
                {
                    Word = "teh",
                    Suggestions = ["the"],
                    EventId = 5,
                },
            ]
        );

        await Assert.That(_sut.MisspelledWord).IsEqualTo("teh");
        await Assert.That(_sut.SelectedSuggestion).IsEqualTo("the");
        await Assert.That(_sut.EventId).IsEqualTo(5);
        await Assert.That(_sut.HaveSuggestions).IsTrue();
    }

    [Test]
    public async Task ChangeCommand_Replaces_Text_And_Moves_To_Next()
    {
        CreateSut(
            [
                new SpellcheckSuggestion
                {
                    Word = "teh",
                    Suggestions = ["the"],
                    EventId = 5,
                },
                new SpellcheckSuggestion
                {
                    Word = "wierd",
                    Suggestions = ["weird"],
                    EventId = 9,
                },
            ]
        );

        // Setup
        var evt1 = new Event(5) { Text = "this is teh test" };
        var evt2 = new Event(9) { Text = "so wierd!" };
        _document.EventManager.AddLast([evt1, evt2]);

        // Execute
        _sut.ChangeCommand.Execute(null);

        // The first event should have been modified
        await Assert.That(evt1.Text).IsEqualTo("this is the test");

        // We should have moved to the next suggestion
        await Assert.That(_sut.MisspelledWord).IsEqualTo("wierd");
        await Assert.That(_sut.SelectedSuggestion).IsEqualTo("weird");
        await Assert.That(_sut.EventId).IsEqualTo(9);
    }

    [Test]
    public async Task AddCommands_Call_Services_And_Suggestion_Is_Removed()
    {
        CreateSut(
            [
                new SpellcheckSuggestion
                {
                    Word = "teh",
                    Suggestions = ["the"],
                    EventId = 7,
                },
            ],
            [
                new SpellcheckSuggestion
                {
                    Word = "wierd",
                    Suggestions = ["weird"],
                    EventId = 8,
                },
            ]
        );

        // Setup
        var evt1 = new Event(7) { Text = "this is teh test" };
        var evt2 = new Event(8) { Text = "so wierd!" };
        _document.EventManager.AddLast([evt1, evt2]);

        _sut.AddToProjectCommand.Execute(null);
        _sut.AddToGlobalsCommand.Execute(null);

        _spellcheckService.Received().AddWordToProject("teh");
        _spellcheckService.Received().AddWordToGlobals("wierd");
        _spellcheckService.Received(3).CheckSpelling(); // Initial + 2

        await Assert.That(_sut.HaveSuggestions).IsFalse();
    }

    [Test]
    public async Task When_No_Suggestions_Remain_HaveSuggestions_Is_False()
    {
        CreateSut(
            [
                new SpellcheckSuggestion
                {
                    Word = "abc",
                    Suggestions = [],
                    EventId = 10,
                },
            ]
        );

        // Move to end
        _sut.FindNextCommand.Execute(null);

        await Assert.That(_sut.HaveSuggestions).IsFalse();
    }
}
