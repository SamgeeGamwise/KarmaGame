using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Sandbox.Game.Config;

internal sealed class SandboxGameSettings
{
    public WindowSettings Window { get; set; } = WindowSettings.CreateDefault();

    public RenderSettings Render { get; set; } = RenderSettings.CreateDefault();

    public InputSettings Input { get; set; } = InputSettings.CreateDefault();

    public SceneSettings Scene { get; set; } = SceneSettings.CreateDefault();

    public PlayerSettings Player { get; set; } = PlayerSettings.CreateDefault();

    public DayNightSettings DayNight { get; set; } = DayNightSettings.CreateDefault();

    public static SandboxGameSettings CreateDefault() => new();
}

internal sealed class WindowSettings
{
    public int VirtualWidth { get; set; } = 640;

    public int VirtualHeight { get; set; } = 360;

    public int BackBufferWidth { get; set; } = 1280;

    public int BackBufferHeight { get; set; } = 720;

    public bool AllowUserResizing { get; set; } = true;

    public static WindowSettings CreateDefault() => new();
}

internal sealed class RenderSettings
{
    public RgbColorSettings ClearColor { get; set; } = new(24, 29, 38);

    public static RenderSettings CreateDefault() => new();
}

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
        new("toggle_debug", "F3"),
        new("exit", "Escape")
    ];

    public static InputSettings CreateDefault() => new();
}

internal sealed class SceneSettings
{
    public string StartingMapAssetName { get; set; } = "Town";

    public string ActionInputActionName { get; set; } = "action";

    public string ExitInputActionName { get; set; } = "exit";

    public string DebugToggleInputActionName { get; set; } = "toggle_debug";

    public float CameraZoom { get; set; } = 1.25f;

    public float PortalTransitionCooldownSeconds { get; set; } = 0.35f;

    public bool DrawPortalDebugOverlay { get; set; } = true;

    public List<PortalSettings> Portals { get; set; } =
    [
        new("Town", "DoorToHouse", "HouseInterior", "HouseFromTown"),
        new("HouseInterior", "DoorToTown", "Town", "TownFromHouse")
    ];

    public static SceneSettings CreateDefault() => new();
}

internal sealed class PlayerSettings
{
    public string SpriteSheetAssetName { get; set; } = "Person2";

    public int TargetHeightInPixels { get; set; } = 24;

    public float WalkFramesPerSecond { get; set; } = 8f;

    public float MoveSpeed { get; set; } = 92f;

    public float RunSpeed { get; set; } = 150f;

    public int CollisionWidth { get; set; } = 10;

    public int CollisionHeight { get; set; } = 9;

    public int CollisionBottomInset { get; set; } = 3;

    public int DoorInteractionWidth { get; set; } = 9;

    public int DoorInteractionHeight { get; set; } = 17;

    public static PlayerSettings CreateDefault() => new();
}

internal sealed class DayNightSettings
{
    public int MinutesPerDay { get; set; } = 24 * 60;

    public int StartMinutes { get; set; } = 6 * 60;

    public int MinutesPerTick { get; set; } = 180;

    public float SecondsPerTick { get; set; } = 5f;

    public int ClockPanelPadding { get; set; } = 8;

    public List<DayNightTintKeyframeSettings> TintTimeline { get; set; } =
    [
        new(0, new RgbColorSettings(24, 32, 62), 0.44f),
        new(2 * 60, new RgbColorSettings(12, 18, 40), 0.56f),
        new(5 * 60, new RgbColorSettings(34, 42, 70), 0.30f),
        new(6 * 60, new RgbColorSettings(255, 214, 170), 0.16f),
        new(12 * 60, new RgbColorSettings(255, 255, 255), 0.04f),
        new(18 * 60, new RgbColorSettings(255, 188, 140), 0.14f),
        new(22 * 60, new RgbColorSettings(42, 50, 86), 0.32f)
    ];

    public static DayNightSettings CreateDefault() => new();
}

internal sealed class PortalSettings
{
    public PortalSettings()
    {
    }

    public PortalSettings(string sourceMapAssetName, string triggerObjectName, string targetMapAssetName, string targetSpawnObjectName)
    {
        SourceMapAssetName = sourceMapAssetName;
        TriggerObjectName = triggerObjectName;
        TargetMapAssetName = targetMapAssetName;
        TargetSpawnObjectName = targetSpawnObjectName;
    }

    public string SourceMapAssetName { get; set; } = string.Empty;

    public string TriggerObjectName { get; set; } = string.Empty;

    public string TargetMapAssetName { get; set; } = string.Empty;

    public string TargetSpawnObjectName { get; set; } = string.Empty;
}

internal sealed class DayNightTintKeyframeSettings
{
    public DayNightTintKeyframeSettings()
    {
    }

    public DayNightTintKeyframeSettings(int minutes, RgbColorSettings color, float alpha)
    {
        Minutes = minutes;
        Color = color;
        Alpha = alpha;
    }

    public int Minutes { get; set; }

    public RgbColorSettings Color { get; set; } = new(255, 255, 255);

    public float Alpha { get; set; }
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

internal sealed class RgbColorSettings
{
    public RgbColorSettings()
    {
    }

    public RgbColorSettings(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public byte R { get; set; }

    public byte G { get; set; }

    public byte B { get; set; }

    public Color ToColor() => new(R, G, B);
}
