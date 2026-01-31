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

public class FlatpakInstallViewModel : ConsoleEnabledViewModelBase, IRoutableViewModel
{
    public IScreen HostScreen { get; }

    private readonly IUnprivilegedOperationService _unprivilegedOperationService;

    private string? _searchText;
    
    private Database _db = new Database();
    public ObservableCollection<FlatpakModel> Flatpaks { get; set; } = new();
    private int _currentPage = 0;
    private bool _isLoading = false;
    
    public ReactiveCommand<Unit, Unit> LoadInitialDataCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    

    public FlatpakInstallViewModel(IScreen screen)
    {
        HostScreen = screen;

        _unprivilegedOperationService = App.Services.GetRequiredService<IUnprivilegedOperationService>();
        AvailablePackages = new ObservableCollection<FlatpakModel>();

        LoadInitialDataCommand = ReactiveCommand.CreateFromTask(LoadInitialDataAsync);
        LoadMoreCommand = ReactiveCommand.CreateFromTask(LoadMoreAsync);
        SearchCommand = ReactiveCommand.CreateFromTask(PerformSearchAsync);

        RemovePackagesCommand = ReactiveCommand.CreateFromTask(RemovePackages);
        RefreshCommand = ReactiveCommand.CreateFromTask(Refresh);
        RemovePackageCommand = ReactiveCommand.Create<FlatpakModel>(RemovePackage);
        
        //LoadData();
    }

    private async Task Refresh()
    {
        try
        {
            await new Database().AddToDatabase(AvailablePackages.ToList());
          
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to refresh installed packages: {e.Message}");
        }
    }

    private async Task PerformSearchAsync()
    {
        Flatpaks.Clear();
        _currentPage = 0;
        
        var items = await Task.Run(() => _db.GetNextPage(_currentPage, SearchText));
        
        foreach (var item in items)
        {
            Flatpaks.Add(item);
        }
    }
    
    private async Task LoadInitialDataAsync()
    {
        Flatpaks.Clear();
        _currentPage = 0;
        
        var items = await Task.Run(() => _db.GetNextPage(_currentPage));
        
        foreach (var item in items)
        {
            Flatpaks.Add(item);
        }
    }
    
    private async Task LoadMoreAsync()
    {
        if (_isLoading) return;
        
        _isLoading = true;
        _currentPage++;
        
        try
        {
            var items = await Task.Run(() => _db.GetNextPage(_currentPage));
            
            foreach (var item in items)
            {
                Flatpaks.Add(item);
            }
        }
        finally
        {
            _isLoading = false;
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

    private async Task RemovePackages()
    {
        var selectedPackages = AvailablePackages.Select(x => x.Name).ToList();
        if (selectedPackages.Any())
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
                    mainWindow.GlobalBusyMessage = "Removing selected packages...";
                }

                //do work

                var result = await _unprivilegedOperationService.RemoveFlatpakPackage(selectedPackages);
                if (!result.Success)
                {
                    Console.WriteLine($"Failed to remove packages: {result.Error}");
                }

                await Refresh();
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
        else
        {
            ShowConfirmDialog = false;
        }
    }

    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

    public System.Reactive.Unit Unit => System.Reactive.Unit.Default;

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RemovePackagesCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RefreshCommand { get; }
    public ReactiveCommand<FlatpakModel, Unit> RemovePackageCommand { get; }

    public ObservableCollection<FlatpakModel> AvailablePackages { get; set; }
    

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    private void RemovePackage(FlatpakModel package)
    {
        AvailablePackages.Remove(package);
    }

    public ReactiveCommand<PackageModel, Unit> TogglePackageCheckCommand { get; }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            AvailablePackages?.Clear();
            Flatpaks?.Clear();
        }
        base.Dispose(disposing);
    }
}