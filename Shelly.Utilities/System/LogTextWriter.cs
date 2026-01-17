using System.Text;

namespace Shelly.Utilities.System;

public class LogTextWriter : TextWriter, IDisposable
{
    private readonly string _filePath;
    private readonly StreamWriter _file;
    private readonly TextWriter _original;
    public override Encoding Encoding => _original.Encoding;
    
    public LogTextWriter(TextWriter original,string logPath)
    {
        if (string.IsNullOrWhiteSpace(logPath))
            throw new ArgumentException("Log path cannot be null or whitespace", nameof(logPath));
        _filePath = Path.Combine(logPath, $"{DateTime.Now.Ticks}-log.txt");
        _file = new StreamWriter(_filePath, true) { AutoFlush = true };
        _original = original;
    }

    public override void Write(char value)
    {
        _original.Write(value);
        _file.Write(value);
    }

    public override void WriteLine(string? value)
    {
        var timestamped = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss zzz} {value}";
        _original.WriteLine(timestamped);
        _file.WriteLine(timestamped);
    }

    public void DeleteLog()
    {
        _file.Close();
        if (File.Exists(_filePath)) File.Delete(_filePath);
    }
}