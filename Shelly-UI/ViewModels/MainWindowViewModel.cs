using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Reactive;
using Shelly_UI.Assets;
using ReactiveUI;
using Material.Icons;
using PackageManager.Alpm;
using Shelly_UI.Services;
using Shelly_UI.Services.AppCache;

namespace Shelly_UI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    
    private PackageViewModel? _cachedPackages;

    public MainWindowViewModel(IConfigService configService, IAppCache appCache, IAlpmManager alpmManager, IScheduler? scheduler = null)
    {
        scheduler ??= RxApp.MainThreadScheduler;

        var packageOperationEvents = Observable.FromEventPattern<AlpmPackageOperationEventArgs>(
            h => alpmManager.PackageOperation += h,
            h => alpmManager.PackageOperation -= h);

        packageOperationEvents.Subscribe(pattern =>
        {
            var args = pattern.EventArgs;
            if (args.EventType == AlpmEventType.PackageOperationStart || args.EventType == AlpmEventType.TransactionStart)
            {
                IsProcessing = true;
                if (!string.IsNullOrEmpty(args.PackageName))
                {
                    ProcessingMessage = $"Completing requested actions: {args.PackageName}";
                }
                else if (args.EventType == AlpmEventType.TransactionStart)
                {
                    ProcessingMessage = "Starting transaction...";
                }
                ProgressValue = 0;
                ProgressIndeterminate = true;
            }
            else if (args.EventType == AlpmEventType.PackageOperationDone || args.EventType == AlpmEventType.TransactionDone)
            {
                if (args.EventType == AlpmEventType.TransactionDone)
                {
                    IsProcessing = false;
                    ProcessingMessage = string.Empty;
                    ProgressValue = 0;
                }
            }
        });

        Observable.FromEventPattern<AlpmProgressEventArgs>(
            h => alpmManager.Progress += h,
            h => alpmManager.Progress -= h)
            .ObserveOn(scheduler)
            .Subscribe(pattern =>
            {
                var args = pattern.EventArgs;
                if (args.Percent.HasValue)
                {
                    ProgressValue = args.Percent.Value;
                    ProgressIndeterminate = false;
                }
                
                if (!string.IsNullOrEmpty(args.PackageName))
                {
                    ProcessingMessage = $"Processing {args.PackageName}...";
                }
            });

        packageOperationEvents
            .Where(e => e.EventArgs.EventType != AlpmEventType.TransactionDone)
            .Throttle(TimeSpan.FromSeconds(30), scheduler)
            .Subscribe(_ =>
            {
                IsProcessing = false;
                ProcessingMessage = string.Empty;
            });

        GoHome = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new HomeViewModel(this)));
        GoPackages = ReactiveCommand.CreateFromObservable(() =>
        {
            _cachedPackages ??= new PackageViewModel(this, appCache);
            return Router.Navigate.Execute(_cachedPackages);
        });
        GoUpdate = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new UpdateViewModel(this)));
        GoRemove = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new RemoveViewModel(this)));
        GoSetting = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new SettingViewModel(this, configService)));

        MenuItems = new()
        {
            new MenuItemViewModel( Resources.Home, MaterialIconKind.Home, "Home page", GoHome),
            new MenuItemViewModel(Resources.Packages,MaterialIconKind.PackageVariantClosed,"View New Packages to Install",
                GoPackages),
            new MenuItemViewModel(Resources.Updates, MaterialIconKind.Update, "Update Existing Packages", GoUpdate),
            new MenuItemViewModel(Resources.Remove, MaterialIconKind.Delete, "Delete Existing Packages", GoRemove),
            new MenuItemViewModel(Resources.Settings, MaterialIconKind.Settings, "Application Settings", GoSetting)
        };
        
        GoHome.Execute(Unit.Default);
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; } 

    private bool _isPaneOpen = false;

    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }

    private bool _isProcessing;
    public bool IsProcessing
    {
        get => _isProcessing;
        set => this.RaiseAndSetIfChanged(ref _isProcessing, value);
    }

    private int _progressValue;
    public int ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }

    private bool _progressIndeterminate = true;
    public bool ProgressIndeterminate
    {
        get => _progressIndeterminate;
        set => this.RaiseAndSetIfChanged(ref _progressIndeterminate, value);
    }

    private string _processingMessage = string.Empty;
    public string ProcessingMessage
    {
        get => _processingMessage;
        set => this.RaiseAndSetIfChanged(ref _processingMessage, value);
    }
    
    public void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public RoutingState Router { get; } = new RoutingState();

    #region ReactiveCommands

    public static ReactiveCommand<Unit, IRoutableViewModel> GoHome { get; set; } = null!;

    public static ReactiveCommand<Unit, IRoutableViewModel> GoUpdate { get; set; } = null!;

    public static ReactiveCommand<Unit, IRoutableViewModel> GoRemove { get; set; } = null!;

    public static ReactiveCommand<Unit, IRoutableViewModel> GoSetting { get; set; } = null!;

    public static ReactiveCommand<Unit, IRoutableViewModel> GoPackages { get; set; } = null!;

    #endregion
   
    #region MenuItemSelectionNav
    private MenuItemViewModel? _selectedMenuItem;

    public MenuItemViewModel? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);

            if (value?.Command != null)
            {
                value.Command.Execute(Unit.Default);
            }
        }
    }
    #endregion
}

public class MenuItemViewModel(
    string label,
    MaterialIconKind icon,
    string toolTip,
    ReactiveCommand<Unit, IRoutableViewModel> command)
{
    public string Label { get; } = label;
    public MaterialIconKind Icon { get; } = icon;

    public string ToolTip { get; } = toolTip;

    public ReactiveCommand<Unit, IRoutableViewModel> Command { get; set; } = command;
}