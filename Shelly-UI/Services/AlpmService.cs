using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PackageManager.Alpm;
using PackageManager.User;
using Avalonia.Threading;
using Shelly_UI.ViewModels;
using Shelly_UI.Views;

namespace Shelly_UI.Services;

public static class AlpmService
{
    private static IAlpmManager? _instance;

    public static IAlpmManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AlpmManager();
            }
            return _instance;
        }
    }
}
