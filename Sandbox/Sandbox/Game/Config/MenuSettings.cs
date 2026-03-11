namespace Sandbox.Game.Config;

internal sealed class MenuSettings
{
    public string ToggleInputActionName { get; set; } = "menu_toggle";

    public string NextItemInputActionName { get; set; } = "menu_next";

    public string PreviousItemInputActionName { get; set; } = "menu_previous";

    public string ConfirmInputActionName { get; set; } = "menu_confirm";

    public string BackInputActionName { get; set; } = "menu_back";

    public bool PauseWorldWhileOpen { get; set; } = true;

    public bool DrawControlHints { get; set; } = true;

    public static MenuSettings CreateDefault() => new();
}
