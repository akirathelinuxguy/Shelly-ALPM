using System;
using System.Collections.Generic;

namespace PackageManager.Alpm;

public class AlpmReplacesEventArgs : EventArgs
{
    public string PackageName { get; }
    public string Repository { get; }
    public List<string> Replaces { get; }

    public AlpmReplacesEventArgs(string packageName, string repository, List<string> replaces)
    {
        PackageName = packageName;
        Repository = repository;
        Replaces = replaces;
    }
}
