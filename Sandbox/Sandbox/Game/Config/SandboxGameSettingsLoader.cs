using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Sandbox.Game.Config;

internal static class SandboxGameSettingsLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static SandboxGameSettings Load()
    {
        SandboxGameSettings defaults = SandboxGameSettings.CreateDefault();
        string settingsPath = ResolveSettingsPath();
        if (!File.Exists(settingsPath))
        {
            Console.WriteLine($"[Sandbox] gameplay settings not found at '{settingsPath}'. Using defaults.");
            return defaults;
        }

        try
        {
            string json = File.ReadAllText(settingsPath);
            SandboxGameSettings? loaded = JsonSerializer.Deserialize<SandboxGameSettings>(json, JsonOptions);
            return Sanitize(loaded ?? defaults, defaults);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Sandbox] failed to load gameplay settings from '{settingsPath}'. Using defaults. {ex.Message}");
            return defaults;
        }
    }

    private static string ResolveSettingsPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Game", "Content", "gameplay.settings.json");
    }

    private static SandboxGameSettings Sanitize(SandboxGameSettings loaded, SandboxGameSettings defaults)
    {
        loaded.Window ??= WindowSettings.CreateDefault();
        loaded.Render ??= RenderSettings.CreateDefault();
        loaded.Input ??= InputSettings.CreateDefault();
        loaded.Scene ??= SceneSettings.CreateDefault();
        loaded.Player ??= PlayerSettings.CreateDefault();
        loaded.DayNight ??= DayNightSettings.CreateDefault();

        loaded.Input.Bindings ??= [];
        if (loaded.Input.Bindings.Count == 0)
            loaded.Input.Bindings = InputSettings.CreateDefault().Bindings;

        loaded.Scene.Portals ??= [];
        if (loaded.Scene.Portals.Count == 0)
            loaded.Scene.Portals = SceneSettings.CreateDefault().Portals;

        loaded.DayNight.TintTimeline ??= [];
        if (loaded.DayNight.TintTimeline.Count == 0)
            loaded.DayNight.TintTimeline = DayNightSettings.CreateDefault().TintTimeline;

        loaded.Window.VirtualWidth = EnsurePositive(loaded.Window.VirtualWidth, defaults.Window.VirtualWidth);
        loaded.Window.VirtualHeight = EnsurePositive(loaded.Window.VirtualHeight, defaults.Window.VirtualHeight);
        loaded.Window.BackBufferWidth = EnsurePositive(loaded.Window.BackBufferWidth, defaults.Window.BackBufferWidth);
        loaded.Window.BackBufferHeight = EnsurePositive(loaded.Window.BackBufferHeight, defaults.Window.BackBufferHeight);

        loaded.Scene.PortalTransitionCooldownSeconds = EnsurePositive(loaded.Scene.PortalTransitionCooldownSeconds, defaults.Scene.PortalTransitionCooldownSeconds);
        loaded.Scene.CameraZoom = EnsurePositive(loaded.Scene.CameraZoom, defaults.Scene.CameraZoom);
        if (string.IsNullOrWhiteSpace(loaded.Scene.StartingMapAssetName))
            loaded.Scene.StartingMapAssetName = defaults.Scene.StartingMapAssetName;
        if (string.IsNullOrWhiteSpace(loaded.Scene.ActionInputActionName))
            loaded.Scene.ActionInputActionName = defaults.Scene.ActionInputActionName;
        if (string.IsNullOrWhiteSpace(loaded.Scene.ExitInputActionName))
            loaded.Scene.ExitInputActionName = defaults.Scene.ExitInputActionName;
        if (string.IsNullOrWhiteSpace(loaded.Scene.DebugToggleInputActionName))
            loaded.Scene.DebugToggleInputActionName = defaults.Scene.DebugToggleInputActionName;

        loaded.Player.TargetHeightInPixels = EnsurePositive(loaded.Player.TargetHeightInPixels, defaults.Player.TargetHeightInPixels);
        loaded.Player.WalkFramesPerSecond = EnsurePositive(loaded.Player.WalkFramesPerSecond, defaults.Player.WalkFramesPerSecond);
        loaded.Player.MoveSpeed = EnsurePositive(loaded.Player.MoveSpeed, defaults.Player.MoveSpeed);
        loaded.Player.RunSpeed = EnsurePositive(loaded.Player.RunSpeed, defaults.Player.RunSpeed);
        loaded.Player.CollisionWidth = EnsurePositive(loaded.Player.CollisionWidth, defaults.Player.CollisionWidth);
        loaded.Player.CollisionHeight = EnsurePositive(loaded.Player.CollisionHeight, defaults.Player.CollisionHeight);
        loaded.Player.CollisionBottomInset = EnsurePositive(loaded.Player.CollisionBottomInset, defaults.Player.CollisionBottomInset);
        loaded.Player.DoorInteractionWidth = EnsurePositive(loaded.Player.DoorInteractionWidth, defaults.Player.DoorInteractionWidth);
        loaded.Player.DoorInteractionHeight = EnsurePositive(loaded.Player.DoorInteractionHeight, defaults.Player.DoorInteractionHeight);
        if (string.IsNullOrWhiteSpace(loaded.Player.SpriteSheetAssetName))
            loaded.Player.SpriteSheetAssetName = defaults.Player.SpriteSheetAssetName;

        loaded.DayNight.MinutesPerDay = EnsurePositive(loaded.DayNight.MinutesPerDay, defaults.DayNight.MinutesPerDay);
        loaded.DayNight.MinutesPerTick = EnsurePositive(loaded.DayNight.MinutesPerTick, defaults.DayNight.MinutesPerTick);
        loaded.DayNight.SecondsPerTick = EnsurePositive(loaded.DayNight.SecondsPerTick, defaults.DayNight.SecondsPerTick);
        loaded.DayNight.ClockPanelPadding = EnsurePositive(loaded.DayNight.ClockPanelPadding, defaults.DayNight.ClockPanelPadding);

        for (int i = 0; i < loaded.Input.Bindings.Count; i++)
        {
            InputBindingSettings? binding = loaded.Input.Bindings[i];
            if (binding is null)
            {
                Console.WriteLine($"[Sandbox] ignoring null input binding at index {i}.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(binding.Action) || string.IsNullOrWhiteSpace(binding.Key))
            {
                Console.WriteLine($"[Sandbox] ignoring invalid input binding at index {i}.");
            }
        }

        loaded.Input.Bindings = loaded.Input.Bindings
            .Where(binding => binding is not null &&
                              !string.IsNullOrWhiteSpace(binding.Action) &&
                              !string.IsNullOrWhiteSpace(binding.Key))
            .Select(binding => binding!)
            .ToList();

        if (loaded.Input.Bindings.Count == 0)
            loaded.Input.Bindings = InputSettings.CreateDefault().Bindings;

        loaded.Scene.Portals = loaded.Scene.Portals
            .Where(portal => portal is not null &&
                !string.IsNullOrWhiteSpace(portal.SourceMapAssetName) &&
                !string.IsNullOrWhiteSpace(portal.TriggerObjectName) &&
                !string.IsNullOrWhiteSpace(portal.TargetMapAssetName) &&
                !string.IsNullOrWhiteSpace(portal.TargetSpawnObjectName))
            .Select(portal => portal!)
            .ToList();

        if (loaded.Scene.Portals.Count == 0)
            loaded.Scene.Portals = SceneSettings.CreateDefault().Portals;

        loaded.DayNight.TintTimeline = loaded.DayNight.TintTimeline
            .Where(keyframe => keyframe is not null)
            .Select(keyframe => keyframe!)
            .OrderBy(keyframe => keyframe.Minutes)
            .ToList();
        if (loaded.DayNight.TintTimeline.Count == 0)
            loaded.DayNight.TintTimeline = DayNightSettings.CreateDefault().TintTimeline;

        loaded.DayNight.StartMinutes = Math.Clamp(loaded.DayNight.StartMinutes, 0, loaded.DayNight.MinutesPerDay - 1);
        foreach (DayNightTintKeyframeSettings keyframe in loaded.DayNight.TintTimeline)
        {
            keyframe.Minutes = Math.Clamp(keyframe.Minutes, 0, loaded.DayNight.MinutesPerDay - 1);
            keyframe.Alpha = Math.Clamp(keyframe.Alpha, 0f, 1f);
            keyframe.Color ??= new RgbColorSettings(255, 255, 255);
        }

        return loaded;
    }

    private static int EnsurePositive(int value, int fallback)
    {
        return value > 0 ? value : fallback;
    }

    private static float EnsurePositive(float value, float fallback)
    {
        return value > 0f ? value : fallback;
    }
}
