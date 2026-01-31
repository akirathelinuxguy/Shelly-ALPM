using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Shelly_UI.ViewModels.Flatpak;

namespace Shelly_UI.Views.Flatpak;

public partial class FlatpakInstallWindow : ReactiveUserControl<FlatpakInstallViewModel>
{
    private FlatpakInstallViewModel _viewModel;
    
    public IScreen HostScreen { get; }
    public FlatpakInstallWindow()
    {

        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(disposables =>
        {
            // Bind scroll detection
            this.FindControl<ScrollViewer>("FlatpakScrollViewer")
                ?.GetObservable(ScrollViewer.OffsetProperty)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(offset =>
                {
                    var scrollViewer = this.FindControl<ScrollViewer>("FlatpakScrollViewer");
                    if (scrollViewer != null)
                    {
                        var viewportHeight = scrollViewer.Viewport.Height;
                        var extentHeight = scrollViewer.Extent.Height;
                        var verticalOffset = offset.Y;
                        
                        // Load more when near bottom
                        if (verticalOffset + viewportHeight >= extentHeight-300)
                        {
                            ViewModel?.LoadMoreCommand.Execute().Subscribe();
                        }
                    }
                })
                .DisposeWith(disposables);
        });
    }
    
}