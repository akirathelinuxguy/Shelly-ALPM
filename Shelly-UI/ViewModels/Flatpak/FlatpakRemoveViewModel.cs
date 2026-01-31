using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using PackageManager.Flatpak;
using Shelly_UI.BaseClasses;
using Shelly_UI.Enums;
using Shelly_UI.Models;
using Shelly_UI.Services;

namespace Shelly_UI.ViewModels.Flatpak;

public class FlatpakRemoveViewModel : ConsoleEnabledViewModelBase, IRoutableViewModel
{
    public IScreen HostScreen { get; }

    private readonly IUnprivilegedOperationService _unprivilegedOperationService;

    private string? _searchText;
    private readonly ObservableAsPropertyHelper<IEnumerable<FlatpakModel>> _filteredPackages;
    private readonly ICredentialManager _credentialManager;

    public FlatpakRemoveViewModel(IScreen screen)
    {
        HostScreen = screen;

        _unprivilegedOperationService = App.Services.GetRequiredService<IUnprivilegedOperationService>();
        AvailablePackages = new ObservableCollection<FlatpakModel>();

        _filteredPackages = this
            .WhenAnyValue(x => x.SearchText, x => x.AvailablePackages.Count, (s, c) => s)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(Search)
            .ToProperty(this, x => x.FilteredPackages);
        
        RefreshCommand = ReactiveCommand.Create(LoadData);
        RemovePackageCommand = ReactiveCommand.CreateFromTask<FlatpakModel>(RemovePackage);

        LoadData();
    }
    
    private async void LoadData()
    {
        try
        {
            var result = await Task.Run(() => _unprivilegedOperationService.ListFlatpakPackages());
            var cleanOutput = result.Output.Replace(System.Environment.NewLine, "");
            var packages = JsonSerializer.Deserialize(
                cleanOutput,
                FlatpakDtoJsonContext.Default.ListFlatpakPackageDto) ?? new List<FlatpakPackageDto>();
            var models = packages.Select(u => new FlatpakModel
            {
                Name = u.Name,
                Version = u.Version,
                Id = u.Id,
                IconPath = $"/var/lib/flatpak/appstream/flathub/x86_64/active/icons/64x64/{u.Id}.png",
                Kind = u.Kind == 0
                ? "App"
                : "Runtime",
            }).ToList();
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                AvailablePackages.Clear();
                foreach (var pkg in models)
                {
                    AvailablePackages.Add(pkg);
                }

                this.RaisePropertyChanged(nameof(AvailablePackages));
            });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load installed packages for removal: {e.Message}");
        }
    }

    private IEnumerable<FlatpakModel> Search(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return AvailablePackages;
        }

        return AvailablePackages.Where(p =>
            p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            p.Version.Contains(searchText, StringComparison.OrdinalIgnoreCase));
    }

    private bool _showConfirmDialog;

    public bool ShowConfirmDialog
    {
        get => _showConfirmDialog;
        set => this.RaiseAndSetIfChanged(ref _showConfirmDialog, value);
    }

    public void ToggleConfirmAction()
    {
        ShowConfirmDialog = !ShowConfirmDialog;
    }

    public async Task RemovePackage(FlatpakModel package)
    {
        MainWindowViewModel? mainWindow = HostScreen as MainWindowViewModel;

        try
        {
            // Set busy
            if (mainWindow != null)
            {
                mainWindow.GlobalProgressValue = 0;
                mainWindow.GlobalProgressText = "0%";
                mainWindow.IsGlobalBusy = true;
                mainWindow.GlobalBusyMessage = "Removing selected package...";
            }

            //do work

            var result = await _unprivilegedOperationService.RemoveFlatpakPackage(package.Id);
            if (!result.Success)
            {
                Console.WriteLine($"Failed to remove packages: {result.Error}");
            }

            LoadData();
        }
        finally
        {
            //always exit globally busy in case of failure
            if (mainWindow != null)
            {
                mainWindow.IsGlobalBusy = false;
            }
        }
    }

    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

    public System.Reactive.Unit Unit => System.Reactive.Unit.Default;
    
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RefreshCommand { get; }
    public ReactiveCommand<FlatpakModel, Unit> RemovePackageCommand { get; }

    public ObservableCollection<FlatpakModel> AvailablePackages { get; set; }

    public IEnumerable<FlatpakModel> FilteredPackages => _filteredPackages.Value;

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public ReactiveCommand<PackageModel, Unit> TogglePackageCheckCommand { get; }
}
