using System;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Shelly_UI.Services;
using Shelly_UI.ViewModels;

namespace Shelly_UI.BaseClasses;

public abstract class ConsoleEnabledViewModelBase : ViewModelBase
{
    private readonly ConfigService _configService = new();
    private readonly ObservableAsPropertyHelper<string> _fullLogText;
    public string FullLogText => _fullLogText.Value;

    private bool _isBottomPanelCollapsed = true;
    public bool IsBottomPanelCollapsed
    {
        get => _isBottomPanelCollapsed;
        set => this.RaiseAndSetIfChanged(ref _isBottomPanelCollapsed, value);
    }

    private bool _isBottomPanelVisible;
    public bool IsBottomPanelVisible
    {
        get => _isBottomPanelVisible;
        set => this.RaiseAndSetIfChanged(ref _isBottomPanelVisible, value);
    }

    protected ConsoleEnabledViewModelBase()
    {
        var consoleEnabled = _configService.LoadConfig().ConsoleEnabled;
        _isBottomPanelVisible = consoleEnabled;

        // Shared logic for the log stream
        _fullLogText = consoleEnabled ? ConsoleLogService.Instance.Logs
            .ToObservableChangeSet()
            .QueryWhenChanged(items => string.Join(Environment.NewLine, items))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.FullLogText) : Observable.Return(string.Empty).ToProperty(this, x => x.FullLogText);
    }

    public void ToggleBottomPanel()
    {
        IsBottomPanelCollapsed = !IsBottomPanelCollapsed;
    }
}
