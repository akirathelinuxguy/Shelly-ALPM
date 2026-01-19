using Avalonia;
using Avalonia.Controls;

namespace Shelly_UI.CustomControls;

public partial class ConsoleSplitControl : UserControl
{
    public static readonly StyledProperty<object?> MainContentProperty =
        AvaloniaProperty.Register<ConsoleSplitControl, object?>(nameof(MainContent));

    public static readonly StyledProperty<object?> BottomContentProperty =
        AvaloniaProperty.Register<ConsoleSplitControl, object?>(nameof(BottomContent));

    public static readonly StyledProperty<bool> IsBottomPanelCollapsedProperty =
        AvaloniaProperty.Register<ConsoleSplitControl, bool>(nameof(IsBottomPanelCollapsed), true);

    public static readonly StyledProperty<bool> IsBottomPanelVisibleProperty =
        AvaloniaProperty.Register<ConsoleSplitControl, bool>(nameof(IsBottomPanelVisible), true);

    public object? MainContent
    {
        get => GetValue(MainContentProperty);
        set => SetValue(MainContentProperty, value);
    }

    public object? BottomContent
    {
        get => GetValue(BottomContentProperty);
        set => SetValue(BottomContentProperty, value);
    }

    public bool IsBottomPanelCollapsed
    {
        get => GetValue(IsBottomPanelCollapsedProperty);
        set => SetValue(IsBottomPanelCollapsedProperty, value);
    }

    public bool IsBottomPanelVisible
    {
        get => GetValue(IsBottomPanelVisibleProperty);
        set => SetValue(IsBottomPanelVisibleProperty, value);
    }

    public ConsoleSplitControl()
    {
        InitializeComponent();
    }

    public void ToggleBottomPanel()
    {
        IsBottomPanelCollapsed = !IsBottomPanelCollapsed;
    }
}