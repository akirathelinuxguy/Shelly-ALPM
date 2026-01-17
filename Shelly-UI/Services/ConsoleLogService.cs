using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Concurrency;
using System.Text;
using ReactiveUI;

namespace Shelly_UI.Services;

public class ConsoleLogService : TextWriter
{
    private static readonly Lazy<ConsoleLogService> _instance = new(() => new ConsoleLogService());
    public static ConsoleLogService Instance => _instance.Value;

    private readonly TextWriter _originalOut;
    private readonly TextWriter _originalError;
    private StringBuilder _lineBuffer = new();
    public ObservableCollection<string> Logs { get; } = new();

    private ConsoleLogService()
    {
        _originalOut = Console.Out;
        _originalError = Console.Error;
        
        Console.SetError(this);
    }

    public override void WriteLine(string? value)
    {
        if (value != null)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                Logs.Add($"[{DateTime.Now:HH:mm:ss}] {value}");
                if (Logs.Count > 500) Logs.RemoveAt(0);
            });
        }
        _originalError.WriteLine(value);
    }
    
    public override void WriteLine(object? value) => WriteLine(value?.ToString());
    
    public override void Write(string? value) 
    {
        _originalError.Write(value);
    }

    public override Encoding Encoding => Encoding.UTF8;
}