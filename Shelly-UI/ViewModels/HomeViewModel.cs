using System;
using System.Collections.ObjectModel;
using PackageManager.Alpm;
using ReactiveUI;

namespace Shelly_UI.ViewModels;

public class HomeViewModel : ViewModelBase, IRoutableViewModel
{
    public HomeViewModel(IScreen screen)
    {
        HostScreen = screen;
        InstalledPackages = new ObservableCollection<AlpmPackage>(new AlpmManager().GetInstalledPackages());
    }

    // Reference to IScreen that owns the routable view model.
    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

    public ObservableCollection<AlpmPackage> InstalledPackages { get; set; }
}