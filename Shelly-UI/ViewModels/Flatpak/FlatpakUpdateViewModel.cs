using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PackageManager.Flatpak;
using ReactiveUI;
using Shelly_UI.BaseClasses;
using Shelly_UI.Models;
using Shelly_UI.Services;
using Shelly_UI.Services.LocalDatabase;

namespace Shelly_UI.ViewModels.Flatpak;

public class FlatpakUpdateViewModel : ConsoleEnabledViewModelBase, IRoutableViewModel
{
    public IScreen HostScreen { get; }

    private readonly IUnprivilegedOperationService _unprivilegedOperationService;

    private string? _searchText;
    private readonly ObservableAsPropertyHelper<IEnumerable<FlatpakModel>> _filteredPackages;

    public FlatpakUpdateViewModel(IScreen screen)
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

        UpdatePackageCommand = ReactiveCommand.CreateFromTask<FlatpakModel>(UpdateCommand);

        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            var result = await Task.Run(() => _unprivilegedOperationService.ListFlatpakUpdates());
            Console.WriteLine($@"[DEBUG_LOG] Loaded {result.Output.Length} installed packages");
            var cleanOutput = result.Output.Replace(System.Environment.NewLine, "");
            var packages = JsonSerializer.Deserialize(
                cleanOutput,
                FlatpakDtoJsonContext.Default.ListFlatpakPackageDto) ?? new List<FlatpakPackageDto>();

            var models = packages.Select(u => new FlatpakModel
            {
                Name = u.Name,
                Version = u.Version,
                IconPath = $"/var/lib/flatpak/appstream/flathub/x86_64/active/icons/64x64/{u.Id}.png",
                Id = u.Id,
                Kind = u.Kind == 0
                    ? "App"
                    : "Runtime",
            }).ToList();
            RxApp.MainThreadScheduler.Schedule(() =>
            {
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

    public async Task UpdateCommand(FlatpakModel package)
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
                mainWindow.GlobalBusyMessage = "Updated selected package...";
            }

            //do work

            var result = await _unprivilegedOperationService.UpdateFlatpakPackage(package.Id);
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

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<FlatpakModel, Unit> UpdatePackageCommand { get; set; }
    public ObservableCollection<FlatpakModel> AvailablePackages { get; set; }
    public IEnumerable<FlatpakModel> FilteredPackages => _filteredPackages.Value;

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }
}