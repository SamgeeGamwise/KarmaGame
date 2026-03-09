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
        loaded.Camera ??= CameraSettings.CreateDefault();
        loaded.DayNight ??= DayNightSettings.CreateDefault();
        loaded.Npcs ??= NpcSystemSettings.CreateDefault();
        loaded.Interaction ??= InteractionSettings.CreateDefault();
        loaded.Menu ??= MenuSettings.CreateDefault();
        loaded.Economy ??= EconomySettings.CreateDefault();
        loaded.Progression ??= ProgressionSettings.CreateDefault();
        loaded.Sleep ??= SleepSettings.CreateDefault();

        loaded.Window.VirtualWidth = EnsurePositive(loaded.Window.VirtualWidth, defaults.Window.VirtualWidth);
        loaded.Window.VirtualHeight = EnsurePositive(loaded.Window.VirtualHeight, defaults.Window.VirtualHeight);
        loaded.Window.BackBufferWidth = EnsurePositive(loaded.Window.BackBufferWidth, defaults.Window.BackBufferWidth);
        loaded.Window.BackBufferHeight = EnsurePositive(loaded.Window.BackBufferHeight, defaults.Window.BackBufferHeight);

        loaded.Scene.CameraZoom = EnsurePositive(loaded.Scene.CameraZoom, defaults.Scene.CameraZoom);
        loaded.Scene.PortalTransitionCooldownSeconds = EnsurePositive(
            loaded.Scene.PortalTransitionCooldownSeconds,
            defaults.Scene.PortalTransitionCooldownSeconds);
        loaded.Scene.StartingMapAssetName = EnsureNotBlank(loaded.Scene.StartingMapAssetName, defaults.Scene.StartingMapAssetName);
        loaded.Scene.ActionInputActionName = EnsureNotBlank(loaded.Scene.ActionInputActionName, defaults.Scene.ActionInputActionName);
        loaded.Scene.ExitInputActionName = EnsureNotBlank(loaded.Scene.ExitInputActionName, defaults.Scene.ExitInputActionName);
        loaded.Scene.DebugToggleInputActionName = EnsureNotBlank(loaded.Scene.DebugToggleInputActionName, defaults.Scene.DebugToggleInputActionName);

        loaded.Player.TargetHeightInPixels = EnsurePositive(loaded.Player.TargetHeightInPixels, defaults.Player.TargetHeightInPixels);
        loaded.Player.WalkFramesPerSecond = EnsurePositive(loaded.Player.WalkFramesPerSecond, defaults.Player.WalkFramesPerSecond);
        loaded.Player.MoveSpeed = EnsurePositive(loaded.Player.MoveSpeed, defaults.Player.MoveSpeed);
        loaded.Player.RunSpeed = EnsurePositive(loaded.Player.RunSpeed, defaults.Player.RunSpeed);
        loaded.Player.CollisionWidth = EnsurePositive(loaded.Player.CollisionWidth, defaults.Player.CollisionWidth);
        loaded.Player.CollisionHeight = EnsurePositive(loaded.Player.CollisionHeight, defaults.Player.CollisionHeight);
        loaded.Player.CollisionBottomInset = EnsurePositive(loaded.Player.CollisionBottomInset, defaults.Player.CollisionBottomInset);
        loaded.Player.DoorInteractionWidth = EnsurePositive(loaded.Player.DoorInteractionWidth, defaults.Player.DoorInteractionWidth);
        loaded.Player.DoorInteractionHeight = EnsurePositive(loaded.Player.DoorInteractionHeight, defaults.Player.DoorInteractionHeight);
        loaded.Player.SpriteSheetAssetName = EnsureNotBlank(loaded.Player.SpriteSheetAssetName, defaults.Player.SpriteSheetAssetName);

        loaded.Camera.ZoomSpeed = EnsurePositive(loaded.Camera.ZoomSpeed, defaults.Camera.ZoomSpeed);

        loaded.DayNight.MinutesPerDay = EnsurePositive(loaded.DayNight.MinutesPerDay, defaults.DayNight.MinutesPerDay);
        loaded.DayNight.MinutesPerTick = EnsurePositive(loaded.DayNight.MinutesPerTick, defaults.DayNight.MinutesPerTick);
        loaded.DayNight.SecondsPerTick = EnsurePositive(loaded.DayNight.SecondsPerTick, defaults.DayNight.SecondsPerTick);
        loaded.DayNight.ClockPanelPadding = EnsurePositive(loaded.DayNight.ClockPanelPadding, defaults.DayNight.ClockPanelPadding);
        loaded.DayNight.StartMinutes = Math.Clamp(loaded.DayNight.StartMinutes, 0, loaded.DayNight.MinutesPerDay - 1);

        loaded.Interaction.NpcInteractionRange = EnsurePositive(loaded.Interaction.NpcInteractionRange, defaults.Interaction.NpcInteractionRange);
        loaded.Interaction.NotificationDurationSeconds = EnsurePositive(
            loaded.Interaction.NotificationDurationSeconds,
            defaults.Interaction.NotificationDurationSeconds);

        loaded.Menu.ToggleInputActionName = EnsureNotBlank(loaded.Menu.ToggleInputActionName, defaults.Menu.ToggleInputActionName);
        loaded.Menu.NextItemInputActionName = EnsureNotBlank(loaded.Menu.NextItemInputActionName, defaults.Menu.NextItemInputActionName);
        loaded.Menu.PreviousItemInputActionName = EnsureNotBlank(loaded.Menu.PreviousItemInputActionName, defaults.Menu.PreviousItemInputActionName);
        loaded.Menu.ConfirmInputActionName = EnsureNotBlank(loaded.Menu.ConfirmInputActionName, defaults.Menu.ConfirmInputActionName);
        loaded.Menu.BackInputActionName = EnsureNotBlank(loaded.Menu.BackInputActionName, defaults.Menu.BackInputActionName);

        loaded.Economy.StartingMoney = EnsureNonNegative(loaded.Economy.StartingMoney, defaults.Economy.StartingMoney);
        loaded.Economy.DebugAddMoneyActionName = EnsureNotBlank(loaded.Economy.DebugAddMoneyActionName, defaults.Economy.DebugAddMoneyActionName);
        loaded.Economy.DebugAddMoneyAmount = EnsurePositive(loaded.Economy.DebugAddMoneyAmount, defaults.Economy.DebugAddMoneyAmount);

        loaded.Progression.StartingLevel = EnsurePositive(loaded.Progression.StartingLevel, defaults.Progression.StartingLevel);

        loaded.Sleep.SleepActionInputActionName = EnsureNotBlank(loaded.Sleep.SleepActionInputActionName, defaults.Sleep.SleepActionInputActionName);
        loaded.Sleep.EarliestSleepMinutes = Math.Clamp(loaded.Sleep.EarliestSleepMinutes, 0, loaded.DayNight.MinutesPerDay - 1);
        loaded.Sleep.LatestSleepMinutes = Math.Clamp(loaded.Sleep.LatestSleepMinutes, 0, loaded.DayNight.MinutesPerDay - 1);
        loaded.Sleep.WakeMinutes = Math.Clamp(loaded.Sleep.WakeMinutes, 0, loaded.DayNight.MinutesPerDay - 1);

        loaded.Input.Bindings ??= [];
        loaded.Input.Bindings = loaded.Input.Bindings
            .Where(binding => binding is not null &&
                              !string.IsNullOrWhiteSpace(binding.Action) &&
                              !string.IsNullOrWhiteSpace(binding.Key))
            .Select(binding => binding!)
            .ToList();
        if (loaded.Input.Bindings.Count == 0)
            loaded.Input.Bindings = InputSettings.CreateDefault().Bindings;

        loaded.Scene.Portals ??= [];
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

        loaded.Scene.Buildings ??= [];
        loaded.Scene.Buildings = loaded.Scene.Buildings
            .Where(building => building is not null &&
                               !string.IsNullOrWhiteSpace(building.BuildingId) &&
                               !string.IsNullOrWhiteSpace(building.DisplayName) &&
                               !string.IsNullOrWhiteSpace(building.ExteriorMapAssetName) &&
                               !string.IsNullOrWhiteSpace(building.EnterTriggerObjectName) &&
                               !string.IsNullOrWhiteSpace(building.InteriorMapAssetName))
            .Select(building => building!)
            .ToList();

        loaded.DayNight.TintTimeline ??= [];
        loaded.DayNight.TintTimeline = loaded.DayNight.TintTimeline
            .Where(keyframe => keyframe is not null)
            .Select(keyframe => keyframe!)
            .OrderBy(keyframe => keyframe.Minutes)
            .ToList();
        if (loaded.DayNight.TintTimeline.Count == 0)
            loaded.DayNight.TintTimeline = DayNightSettings.CreateDefault().TintTimeline;

        foreach (DayNightTintKeyframeSettings keyframe in loaded.DayNight.TintTimeline)
        {
            keyframe.Minutes = Math.Clamp(keyframe.Minutes, 0, loaded.DayNight.MinutesPerDay - 1);
            keyframe.Alpha = Math.Clamp(keyframe.Alpha, 0f, 1f);
            keyframe.Color ??= new RgbColorSettings(255, 255, 255);
        }

        loaded.Npcs.Definitions ??= [];
        loaded.Npcs.Definitions = loaded.Npcs.Definitions
            .Where(npc => npc is not null &&
                          !string.IsNullOrWhiteSpace(npc.NpcId) &&
                          !string.IsNullOrWhiteSpace(npc.DisplayName) &&
                          !string.IsNullOrWhiteSpace(npc.MapAssetName))
            .Select(npc => npc!)
            .ToList();
        if (loaded.Npcs.Definitions.Count == 0)
            loaded.Npcs.Definitions = NpcSystemSettings.CreateDefault().Definitions;

        foreach (NpcDefinitionSettings npc in loaded.Npcs.Definitions)
        {
            npc.SpriteSheetAssetName = EnsureNotBlank(npc.SpriteSheetAssetName, "Person2");
            npc.FrameWidth = EnsurePositive(npc.FrameWidth, 23);
            npc.FrameHeight = EnsurePositive(npc.FrameHeight, 36);
            npc.SourceOffsetX = EnsureNonNegative(npc.SourceOffsetX, 1);
            npc.SourceOffsetY = EnsureNonNegative(npc.SourceOffsetY, 1);
            npc.FeetBottomInset = EnsureNonNegative(npc.FeetBottomInset, 3);
            npc.TargetHeightInPixels = EnsurePositive(npc.TargetHeightInPixels, 24);
            npc.InteractionRange = EnsurePositive(npc.InteractionRange, loaded.Interaction.NpcInteractionRange);
            npc.DialogueLines ??= [];
            npc.DialogueLines = npc.DialogueLines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();
            if (npc.DialogueLines.Count == 0)
                npc.DialogueLines.Add($"{npc.DisplayName}: placeholder dialogue.");
        }

        loaded.Progression.StartingInventory ??= [];
        loaded.Progression.StartingInventory = loaded.Progression.StartingInventory
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
        loaded.Progression.StartingLore ??= [];
        loaded.Progression.StartingLore = loaded.Progression.StartingLore
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();

        loaded.Progression.StartingSkills ??= [];
        loaded.Progression.StartingSkills = loaded.Progression.StartingSkills
            .Where(skill => skill is not null && !string.IsNullOrWhiteSpace(skill.Name))
            .Select(skill => skill!)
            .ToList();
        if (loaded.Progression.StartingSkills.Count == 0)
            loaded.Progression.StartingSkills = ProgressionSettings.CreateDefault().StartingSkills;
        foreach (SkillSeedSettings skill in loaded.Progression.StartingSkills)
            skill.Level = EnsurePositive(skill.Level, 1);

        loaded.Sleep.Spots ??= [];
        loaded.Sleep.Spots = loaded.Sleep.Spots
            .Where(spot => spot is not null && !string.IsNullOrWhiteSpace(spot.MapAssetName))
            .Select(spot => spot!)
            .ToList();
        if (loaded.Sleep.Spots.Count == 0)
            loaded.Sleep.Spots = SleepSettings.CreateDefault().Spots;
        foreach (SleepSpotSettings spot in loaded.Sleep.Spots)
            spot.FallbackRadius = EnsurePositive(spot.FallbackRadius, 24f);

        return loaded;
    }

    private static string EnsureNotBlank(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static int EnsurePositive(int value, int fallback)
    {
        return value > 0 ? value : fallback;
    }

    private static int EnsureNonNegative(int value, int fallback)
    {
        return value >= 0 ? value : fallback;
    }

    private static float EnsurePositive(float value, float fallback)
    {
        return value > 0f ? value : fallback;
    }
}
