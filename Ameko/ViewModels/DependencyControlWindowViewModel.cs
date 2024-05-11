using Ameko.Services;
using DynamicData;
using Holo;
using Holo.DC;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.ViewModels
{
    public class DependencyControlWindowViewModel : ViewModelBase
    {
        public ObservableCollection<ScriptEntity> InstalledScripts { get; }
        public ObservableCollection<Repository> Repositories { get; }
        public ObservableCollection<ScriptEntity> RepoScripts { get; }
        public ObservableCollection<ScriptEntity> UpdateCandidates { get; }

        public List<ScriptEntity> SelectedRepoScripts { get; }
        public List<ScriptEntity> SelectedInstalledScripts { get; }
        public List<Repository> SelectedRepos { get; }

        private string _repoTextBoxText;
        public string RepoTextBoxText
        {
            get => _repoTextBoxText;
            set => this.RaiseAndSetIfChanged(ref _repoTextBoxText, value);
        }

        private string _impExpTextBoxText;
        public string ImpExptextBoxText
        {
            get => _impExpTextBoxText;
            set => this.RaiseAndSetIfChanged(ref _impExpTextBoxText, value);
        }

        public Interaction<DependencyControlWindowViewModel, Unit> DisplayRepoManager { get; }
        public Interaction<DependencyControlWindowViewModel, Unit> DisplayImportExport { get; }

        public ICommand InstallScriptCommand { get; }
        public ICommand UninstallScriptCommand { get; }
        public ICommand UpdateScriptCommand { get; }
        public ICommand UpdateAllCommand { get; }
        public ICommand ExportScriptsCommand { get; }
        public ICommand ImportScriptsCommand { get; }

        public ICommand AddRepositoryCommand { get; }
        public ICommand RemoveRepositoryCommand { get; }
        public ICommand DisplayRepoManagerCommand { get; }
        public ICommand DisplayImportExportCommand { get; }

        private void PopulateInstalledScriptsList()
        {
            var scriptEntities = ScriptService.Instance.LoadedScripts.Select(s =>
            {
                var realScript = ScriptService.Instance.Get(s.Item1);
                return new ScriptEntity
                {
                    Name = realScript?.Name,
                    QualifiedName = realScript?.QualifiedName,
                    Author = realScript?.Author,
                    CurrentVersion = realScript?.Version ?? 0,
                    Description = realScript?.Description
                };
            }).ToList();

            InstalledScripts.Clear();
            InstalledScripts.AddRange(scriptEntities);

            PopulateRepoScriptsList();
            PopulateUpdateCandidatesList();
        }

        private void PopulateUpdateCandidatesList()
        {
            UpdateCandidates.Clear();
            var candidates = DCScriptManager.GetUpdateCandidates(HoloContext.Instance.RepositoryManager.RepoScripts.ToList(), InstalledScripts.ToList());
            UpdateCandidates.AddRange(candidates);
        }

        private void PopulateRepoScriptsList()
        {
            RepoScripts.Clear();
            RepoScripts.AddRange(
                HoloContext.Instance.RepositoryManager.RepoScripts.Where(s => !DCScriptManager.IsDCScriptInstalled(s.QualifiedName ?? ""))
            );
        }

        private async Task<bool> TryUpdate(ScriptEntity script)
        {
            if (script.QualifiedName == null) return false;

            // Check if the script is an update candidate
            var candidates = UpdateCandidates.Where(s => s.QualifiedName!.Equals(script.QualifiedName));
            if (candidates.Any())
            {
                var serverScript = candidates.First();
                if (serverScript.Url == null) return false;
                return await DCScriptManager.UpdateDCScript(script);
            }
            return false;
        }

        public DependencyControlWindowViewModel()
        {
            _repoTextBoxText = string.Empty;
            DisplayRepoManager = new Interaction<DependencyControlWindowViewModel, Unit>();
            DisplayImportExport = new Interaction<DependencyControlWindowViewModel, Unit>();

            SelectedRepoScripts = new List<ScriptEntity>();
            SelectedInstalledScripts = new List<ScriptEntity>();
            SelectedRepos = new List<Repository>();

            Repositories = new ObservableCollection<Repository>(HoloContext.Instance.RepositoryManager.Repositories);

            RepoScripts = new ObservableCollection<ScriptEntity>();
            InstalledScripts = new ObservableCollection<ScriptEntity>();
            UpdateCandidates = new ObservableCollection<ScriptEntity>();
            PopulateInstalledScriptsList();

            InstallScriptCommand = ReactiveCommand.Create(async () =>
            {
                foreach (var script in SelectedRepoScripts)
                {
                    if (script.QualifiedName != null && script.Url != null)
                        await DCScriptManager.InstallDCScript(script);
                }
                ScriptService.Instance.Reload(false);
                PopulateInstalledScriptsList();
            });

            UninstallScriptCommand = ReactiveCommand.Create(() =>
            {
                foreach (var script in SelectedInstalledScripts)
                {
                    if (script.QualifiedName != null)
                        DCScriptManager.UninstallDCScript(script);
                }
                ScriptService.Instance.Reload(false);
                PopulateInstalledScriptsList();
            });

            UpdateScriptCommand = ReactiveCommand.Create(async () =>
            {
                foreach (var script in SelectedInstalledScripts)
                {
                    await TryUpdate(script);
                }
                ScriptService.Instance.Reload(false);
                PopulateInstalledScriptsList();
            });

            UpdateAllCommand = ReactiveCommand.Create(async () =>
            {
                foreach (var script in InstalledScripts)
                {
                    await TryUpdate(script);
                }
                ScriptService.Instance.Reload(false);
                PopulateInstalledScriptsList();
            });

            AddRepositoryCommand = ReactiveCommand.Create(async () =>
            {
                var input = RepoTextBoxText.Trim();
                if (input.Equals(string.Empty)) return;

                var repo = await Repository.Build(input);
                if (repo == null) return;

                HoloContext.Instance.ConfigurationManager.AddRepository(repo.Url ?? string.Empty);
                HoloContext.Instance.RepositoryManager.GatherRepositories(repo);
                HoloContext.Instance.RepositoryManager.GatherRepoScripts();

                Repositories.Clear();
                Repositories.AddRange(HoloContext.Instance.RepositoryManager.Repositories);
                HoloContext.Instance.RepositoryManager.GatherRepoScripts();
                PopulateRepoScriptsList();
            });

            RemoveRepositoryCommand = ReactiveCommand.Create(() =>
            {
                foreach (var repo in SelectedRepos.ToArray())
                {
                    HoloContext.Instance.RepositoryManager.RemoveRepository(repo);
                    HoloContext.Instance.ConfigurationManager.RemoveRepository(repo.Url ?? string.Empty);
                }
                Repositories.Clear();
                Repositories.AddRange(HoloContext.Instance.RepositoryManager.Repositories);
                HoloContext.Instance.RepositoryManager.GatherRepoScripts();
                PopulateRepoScriptsList();
            });

            DisplayRepoManagerCommand = ReactiveCommand.Create(async () => {
                await DisplayRepoManager.Handle(this);
            });

            DisplayImportExportCommand = ReactiveCommand.Create(async () => {
                await DisplayImportExport.Handle(this);
            });

            ExportScriptsCommand = ReactiveCommand.Create(() =>
            {
                ImpExptextBoxText = JsonSerializer.Serialize(HoloContext.Instance.ConfigurationManager.InstalledScriptsMap);
            });

            ImportScriptsCommand = ReactiveCommand.Create(async () =>
            {
                try
                {
                    var map = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(ImpExptextBoxText);
                    if (map == null) return;
                    HoloContext.Instance.ConfigurationManager.ImportScriptList(map);
                    foreach (var pair in map)
                    {
                        var repo = await Repository.Build(pair.Key);
                        if (repo == null) continue;
                        HoloContext.Instance.RepositoryManager.GatherRepositories(repo);
                        HoloContext.Instance.RepositoryManager.GatherRepoScripts();
                        
                        foreach (var scriptQname in pair.Value)
                        {
                            var scripts = HoloContext.Instance.RepositoryManager.RepoScripts.Where(s => s.QualifiedName == scriptQname);
                            if (!scripts.Any()) continue;
                            var script = scripts.First();
                            await DCScriptManager.InstallDCScript(script);
                        }
                    }
                }
                catch (Exception ex)
                {
                    HoloContext.Logger.Error(ex.Message, "DC ImportScripts");
                }
                finally
                {
                    Repositories.Clear();
                    Repositories.AddRange(HoloContext.Instance.RepositoryManager.Repositories);
                    PopulateRepoScriptsList();
                    PopulateInstalledScriptsList();
                    ScriptService.Instance.Reload(false);
                    ImpExptextBoxText = string.Empty;
                }
            });

        }
    }
}
