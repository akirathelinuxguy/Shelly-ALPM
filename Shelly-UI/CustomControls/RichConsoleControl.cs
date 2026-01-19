using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;
using System.Linq;
using Avalonia.Threading;

namespace Shelly_UI.CustomControls;

public class RichConsoleControl : Control
{
    public static readonly StyledProperty<ObservableCollection<string>?> LogsProperty =
        AvaloniaProperty.Register<RichConsoleControl, ObservableCollection<string>?>(nameof(Logs));

    public ObservableCollection<string>? Logs
    {
        get => GetValue(LogsProperty);
        set => SetValue(LogsProperty, value);
    }

    private readonly Typeface _typeface = new("Cascadia Code,Consolas,Monospace");
    private const double FontSize = 12;
    private const double LineHeight = 16;
    
    private int _selectionStartLine = -1;
    private int _selectionEndLine = -1;

    public RichConsoleControl()
    {
        ClipToBounds = true;
        Focusable = true;
    }
    
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Logs == null || Logs.Count == 0) return new Size(availableSize.Width, 0);
        return new Size(availableSize.Width, Logs.Count * LineHeight);
    }

    protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
       
        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed) return;
        Focus();

        var pos = e.GetPosition(this);
        var clickedLine = (int)(pos.Y / LineHeight);

        // Constrain to valid log lines
        if (Logs != null && Logs.Count > 0)
        {
            clickedLine = Math.Clamp(clickedLine, 0, Logs.Count - 1);
        }

        _selectionStartLine = clickedLine;
        _selectionEndLine = clickedLine;
        
        e.Pointer.Capture(this);
        InvalidateVisual();
            
        e.Handled = true;
    }

    protected override void OnPointerMoved(Avalonia.Input.PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        
        if (!Equals(e.Pointer.Captured, this)) return;
        var pos = e.GetPosition(this);
        var currentLine = (int)(pos.Y / LineHeight);
            
        if (Logs != null && Logs.Count > 0)
        {
            currentLine = Math.Clamp(currentLine, 0, Logs.Count - 1);
        }

        if (_selectionEndLine != currentLine)
        {
            _selectionEndLine = currentLine;
            InvalidateVisual();
        }
        e.Handled = true;
    }
    protected override void OnPointerReleased(Avalonia.Input.PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (Equals(e.Pointer.Captured, this))
        {
            e.Pointer.Capture(null);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != LogsProperty) return;
        if (change.OldValue is ObservableCollection<string> oldLogs)
        {
            oldLogs.CollectionChanged -= OnLogsChanged;
        }
            
        if (change.NewValue is ObservableCollection<string> newLogs)
        {
            newLogs.CollectionChanged += OnLogsChanged;
        }
            
        InvalidateMeasure();
        InvalidateVisual();
    }
    
    private void OnLogsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Triggered when new logs are added
        Dispatcher.UIThread.Post(() =>
        {
            InvalidateMeasure(); 
            InvalidateVisual();
        }, DispatcherPriority.Background);

        if (this.Parent is not ScrollViewer sv) return;
        var isAtBottom = sv.Offset.Y >= sv.Extent.Height - sv.Viewport.Height - 10;
        
        if (!isAtBottom) return;
        if (Logs != null) sv.Offset = new Vector(sv.Offset.X, Logs.Count * LineHeight);
    }

    public override void Render(DrawingContext context)
    {
        if (Logs == null || Logs.Count == 0) return;

        // Get the current scroll offset from the parent ScrollViewer
        var scrollViewer = this.Parent as ScrollViewer;
        var offset = scrollViewer?.Offset.Y ?? 0;

        
        var accentColor = Colors.RoyalBlue; 
        if (Application.Current?.TryFindResource("SystemAccentColor", out var resource) == true && resource is Color col)
        {
            accentColor = col;
        }
        
        var foreground = Brushes.LightGray;
        var selectionBrush = new SolidColorBrush(accentColor, 0.4);

        // Calculate which lines are visible to prevent rendering off-screen lines
        var firstVisibleLine = (int)(offset / LineHeight);
        var lastVisibleLine = firstVisibleLine + (int)(Bounds.Height / LineHeight) + 1;
        lastVisibleLine = Math.Min(lastVisibleLine, Logs.Count);

        for (var i = firstVisibleLine; i < lastVisibleLine; i++)
        {
            var y = i * LineHeight;

            // Selection highlighting
            var start = Math.Min(_selectionStartLine, _selectionEndLine);
            var end = Math.Max(_selectionStartLine, _selectionEndLine);
            if (i >= start && i <= end && start != -1)
            {
                context.FillRectangle(selectionBrush, new Rect(0, y, Bounds.Width, LineHeight));
            }

            var text = new FormattedText(
                Logs[i],
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _typeface,
                FontSize,
                foreground);

            // Draw the text at the correct absolute position
            context.DrawText(text, new Point(5, y));
        }
    }

    protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key != Avalonia.Input.Key.C || e.KeyModifiers != Avalonia.Input.KeyModifiers.Control) return;
        var start = Math.Min(_selectionStartLine, _selectionEndLine);
        var end = Math.Max(_selectionStartLine, _selectionEndLine);
        if (start == -1 || Logs == null) return;
        var selectedText = string.Join(Environment.NewLine, Logs.Skip(start).Take(end - start + 1));
        TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(selectedText);
    }
}