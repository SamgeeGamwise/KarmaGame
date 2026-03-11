using System.Collections.Generic;

namespace Sandbox.Game.Config;

internal sealed class InputSettings
{
    public List<InputBindingSettings> Bindings { get; set; } =
    [
        new("move_left", "A"),
        new("move_right", "D"),
        new("move_up", "W"),
        new("move_down", "S"),
        new("run", "LeftShift"),
        new("action", "E"),
        new(DebugSettings.ToggleDebugLinesActionName, "F3"),
        new(DebugSettings.TogglePlayerDebugActionName, "F4"),
        new("menu_toggle", "Tab"),
        new("menu_next", "S"),
        new("menu_next", "D"),
        new("menu_previous", "W"),
        new("menu_previous", "A"),
        new("menu_confirm", "Enter"),
        new("menu_back", "Escape"),
        new(DebugSettings.AddMoneyActionName, "F6"),
        new("exit", "Escape")
    ];

    public static InputSettings CreateDefault() => new();
}

internal sealed class InputBindingSettings
{
    public InputBindingSettings()
    {
    }

    public InputBindingSettings(string action, string key)
    {
        Action = action;
        Key = key;
    }

    public string Action { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;
}
