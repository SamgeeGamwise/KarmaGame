namespace Sandbox.Game.Config;

internal sealed class DebugSettings
{
    public const string ToggleDebugLinesActionName = "toggle_debug";
    public const string TogglePlayerDebugActionName = "toggle_player_debug";
    public const string AddMoneyActionName = "debug_add_money";

    public bool SkipMainMenu { get; set; } = true;

    public bool StartWithDebugLinesOn { get; set; } = false;

    public bool ShowPlayerCollisionBox { get; set; } = true;

    public bool ShowPlayerOcclusionBox { get; set; } = false;

    public bool ShowPlayerInteractionBox { get; set; } = false;

    public string ToggleDebugLinesInputActionName { get; set; } = ToggleDebugLinesActionName;

    public string TogglePlayerDebugInputActionName { get; set; } = TogglePlayerDebugActionName;

    public string AddMoneyInputActionName { get; set; } = AddMoneyActionName;

    public int AddMoneyAmount { get; set; } = 10;

    public static DebugSettings CreateDefault() => new();
}
