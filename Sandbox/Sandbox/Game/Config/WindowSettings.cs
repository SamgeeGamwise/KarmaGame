namespace Sandbox.Game.Config;

internal sealed class WindowSettings
{
    public int VirtualWidth { get; set; } = 640;

    public int VirtualHeight { get; set; } = 360;

    public int BackBufferWidth { get; set; } = 1280;

    public int BackBufferHeight { get; set; } = 720;

    public bool AllowUserResizing { get; set; } = true;

    public DisplayMode StartDisplayMode { get; set; } = DisplayMode.Fullscreen;

    public static WindowSettings CreateDefault() => new();
}

internal enum DisplayMode
{
    Windowed,
    Fullscreen,
    Borderless
}
