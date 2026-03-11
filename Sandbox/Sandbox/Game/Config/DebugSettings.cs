namespace Sandbox.Game.Config;

internal sealed class DebugSettings
{
    public const string ToggleDebugLinesActionName = "toggle_debug";
    public const string AddMoneyActionName = "debug_add_money";

    public bool SkipMainMenu { get; set; } = true;

    public bool StartWithDebugLinesOn { get; set; } = true;

    public string ToggleDebugLinesInputActionName { get; set; } = ToggleDebugLinesActionName;

    public string AddMoneyInputActionName { get; set; } = AddMoneyActionName;

    public int AddMoneyAmount { get; set; } = 10;

    public static DebugSettings CreateDefault() => new();
}
