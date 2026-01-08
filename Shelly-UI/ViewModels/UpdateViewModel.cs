using System;
using System.Collections.ObjectModel;
using System.Linq;
using PackageManager.Alpm;
using ReactiveUI;
using Shelly_UI.Models;

namespace Shelly_UI.ViewModels;

public class UpdateViewModel : ViewModelBase, IRoutableViewModel
{
    public IScreen HostScreen { get; }

    public UpdateViewModel(IScreen screen)
    {
        var manager = new AlpmManager();
        manager.Initialize();
        manager.Sync();
        HostScreen = screen;
       
        var updates = manager.GetPackagesNeedingUpdate();
        
        PackagesForUpdating = new ObservableCollection<UpdateModel>(
            updates.Select(u => new UpdateModel 
            {
                Name = u.Name,
                CurrentVersion = u.CurrentVersion,
                NewVersion = u.NewVersion,
                DownloadSize = u.DownloadSize,
                IsChecked = false
            })
        );
      
    }
    
    public void CheckAll()
    {
        var targetState = PackagesForUpdating.Any(x => !x.IsChecked);

        foreach (var item in PackagesForUpdating)
        {
            item.IsChecked = targetState;
        }
    }
    
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
    
    public ObservableCollection<UpdateModel> PackagesForUpdating { get; set; }
}