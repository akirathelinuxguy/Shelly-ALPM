using System;
using System.Runtime.InteropServices;

namespace PackageManager.Alpm;

[StructLayout(LayoutKind.Sequential)]
internal struct AlpmPackageOperationEvent
{
    public AlpmEventType Type;
    public int Operation;
    public IntPtr OldPkgPtr;
    public IntPtr NewPkgPtr;
}