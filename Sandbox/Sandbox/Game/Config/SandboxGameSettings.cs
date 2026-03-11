using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Sandbox.Game.Config;

internal sealed class SandboxGameSettings
{
    public WindowSettings Window { get; set; } = WindowSettings.CreateDefault();

    public RenderSettings Render { get; set; } = RenderSettings.CreateDefault();

    public DebugSettings Debug { get; set; } = DebugSettings.CreateDefault();

    public InputSettings Input { get; set; } = InputSettings.CreateDefault();

    public SceneSettings Scene { get; set; } = SceneSettings.CreateDefault();

    public PlayerSettings Player { get; set; } = PlayerSettings.CreateDefault();

    public CameraSettings Camera { get; set; } = CameraSettings.CreateDefault();

    public DayNightSettings DayNight { get; set; } = DayNightSettings.CreateDefault();

    public NpcSystemSettings Npcs { get; set; } = NpcSystemSettings.CreateDefault();

    public InteractionSettings Interaction { get; set; } = InteractionSettings.CreateDefault();

    public MenuSettings Menu { get; set; } = MenuSettings.CreateDefault();

    public EconomySettings Economy { get; set; } = EconomySettings.CreateDefault();

    public ProgressionSettings Progression { get; set; } = ProgressionSettings.CreateDefault();

    public SleepSettings Sleep { get; set; } = SleepSettings.CreateDefault();

    public static SandboxGameSettings CreateDefault() => new();
}

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

internal sealed class RenderSettings
{
    public RgbColorSettings ClearColor { get; set; } = new(24, 29, 38);

    public static RenderSettings CreateDefault() => new();
}

internal sealed class DebugSettings
{
    public bool SkipMainMenu { get; set; } = true;

    public static DebugSettings CreateDefault() => new();
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
        new("menu_toggle", "Tab"),
        new("menu_next", "S"),
        new("menu_next", "D"),
        new("menu_previous", "W"),
        new("menu_previous", "A"),
        new("menu_confirm", "Enter"),
        new("menu_back", "Escape"),
        new("debug_add_money", "F6"),
        new("exit", "Escape")
    ];

    public static InputSettings CreateDefault() => new();
}

internal sealed class SceneSettings
{
    public string StartingMapAssetName { get; set; } = "GameMap";

    public string ActionInputActionName { get; set; } = "action";

    public string ExitInputActionName { get; set; } = "exit";

    public string DebugToggleInputActionName { get; set; } = "toggle_debug";

    public float CameraZoom { get; set; } = 1f;

    public float PortalTransitionCooldownSeconds { get; set; } = 0.35f;

    public bool DrawPortalDebugOverlay { get; set; } = true;

    public bool FreezeWorldWhileMenuOpen { get; set; } = true;

    public bool FreezeWorldWhileDialogueOpen { get; set; } = true;

    public List<PortalSettings> Portals { get; set; } =
    [
        new("GameMap", "DoorToHospital", "HospitalInterior", "HospitalFromTown"),
        new("HospitalInterior", "DoorToTown", "GameMap", "TownFromHospital")
    ];

    public List<BuildingSettings> Buildings { get; set; } =
    [
        new(
            "home",
            "Player Home",
            "Town",
            "DoorToHouse",
            "HouseInterior",
            "HouseFromTown",
            "DoorToTown",
            "TownFromHouse",
            true),
        new(
            "blacksmith",
            "Placeholder Blacksmith",
            "Town",
            "DoorToBlacksmith",
            "HouseInterior",
            "HouseFromTown",
            "DoorToTown",
            "TownFromBlacksmith",
            false),
        new(
            "library",
            "Placeholder Library",
            "Town",
            "DoorToLibrary",
            "HouseInterior",
            "HouseFromTown",
            "DoorToTown",
            "TownFromLibrary",
            false)
    ];

    public static SceneSettings CreateDefault() => new();
}

internal sealed class PlayerSettings
{
    public string SpriteSheetAssetName { get; set; } = "Person2";

    public int TargetHeightInPixels { get; set; } = 64;

    public float WalkFramesPerSecond { get; set; } = 8f;

    public float MoveSpeed { get; set; } = 200f;

    public float RunSpeed { get; set; } = 250f;

    public int CollisionWidth { get; set; } = 10;

    public int CollisionHeight { get; set; } = 9;

    public int CollisionBottomInset { get; set; } = 3;

    public int DoorInteractionWidth { get; set; } = 9;

    public int DoorInteractionHeight { get; set; } = 17;

    public static PlayerSettings CreateDefault() => new();
}

internal sealed class CameraSettings
{
    public float ZoomSpeed { get; set; } = 0.1f;

    public float MinZoom { get; set; } = 0.8f;

    public float MaxZoom { get; set; } = 1.6f;

    public static CameraSettings CreateDefault() => new();
}

internal sealed class DayNightSettings
{
    public int MinutesPerDay { get; set; } = 24 * 60;

    public int StartMinutes { get; set; } = 6 * 60;

    public int MinutesPerTick { get; set; } = 360;

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

internal sealed class NpcSystemSettings
{
    public List<NpcDefinitionSettings> Definitions { get; set; } =
    [
        new(
            "npc_guard",
            "Town Guard",
            "Town",
            string.Empty,
            520f,
            402f,
            "ShopClerk",
            24,
            22f,
            [
                "Morning. Keep your coin purse close.",
                "Placeholder quest hooks will live here."
            ])
        {
            FrameWidth = 32,
            FrameHeight = 64,
            SourceOffsetX = 0,
            SourceOffsetY = 0
        },
        new(
            "npc_farmer",
            "Field Farmer",
            "Town",
            string.Empty,
            336f,
            270f,
            "Person2",
            24,
            22f,
            [
                "These crops are temporary, but the grind is permanent.",
                "Someday this spot will run farming loops."
            ]),
        new(
            "npc_scholar",
            "Library Scholar",
            "HouseInterior",
            string.Empty,
            176f,
            160f,
            "Person2",
            24,
            24f,
            [
                "This house interior is a stand-in for every building.",
                "Lore and codex pages will connect here."
            ])
    ];

    public static NpcSystemSettings CreateDefault() => new();
}

internal sealed class InteractionSettings
{
    public float NpcInteractionRange { get; set; } = 22f;

    public bool ShowInteractionHints { get; set; } = true;

    public float NotificationDurationSeconds { get; set; } = 2.6f;

    public static InteractionSettings CreateDefault() => new();
}

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

internal sealed class EconomySettings
{
    public int StartingMoney { get; set; } = 125;

    public string DebugAddMoneyActionName { get; set; } = "debug_add_money";

    public int DebugAddMoneyAmount { get; set; } = 10;

    public static EconomySettings CreateDefault() => new();
}

internal sealed class ProgressionSettings
{
    public int StartingLevel { get; set; } = 1;

    public List<string> StartingInventory { get; set; } =
    [
        "Bedroll",
        "Copper Key",
        "Starter Pickaxe"
    ];

    public List<SkillSeedSettings> StartingSkills { get; set; } =
    [
        new("Foraging", 1),
        new("Crafting", 1),
        new("Negotiation", 1)
    ];

    public List<string> StartingLore { get; set; } =
    [
        "The settlement was rebuilt after the Silent Storm.",
        "Most buildings share a common interior while the district expands."
    ];

    public static ProgressionSettings CreateDefault() => new();
}

internal sealed class SleepSettings
{
    public bool Enabled { get; set; } = true;

    public string SleepActionInputActionName { get; set; } = "action";

    public bool AllowSleepAnytime { get; set; } = false;

    public int EarliestSleepMinutes { get; set; } = 18 * 60;

    public int LatestSleepMinutes { get; set; } = 5 * 60;

    public int WakeMinutes { get; set; } = 6 * 60;

    public List<SleepSpotSettings> Spots { get; set; } =
    [
        new("HouseInterior", "BedSleepSpot", 160f, 140f, 18f, "Sleep in bed")
    ];

    public static SleepSettings CreateDefault() => new();
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

internal sealed class BuildingSettings
{
    public BuildingSettings()
    {
    }

    public BuildingSettings(
        string buildingId,
        string displayName,
        string exteriorMapAssetName,
        string enterTriggerObjectName,
        string interiorMapAssetName,
        string interiorSpawnObjectName,
        string exitTriggerObjectName,
        string exteriorSpawnObjectName,
        bool isPlayerHome)
    {
        BuildingId = buildingId;
        DisplayName = displayName;
        ExteriorMapAssetName = exteriorMapAssetName;
        EnterTriggerObjectName = enterTriggerObjectName;
        InteriorMapAssetName = interiorMapAssetName;
        InteriorSpawnObjectName = interiorSpawnObjectName;
        ExitTriggerObjectName = exitTriggerObjectName;
        ExteriorSpawnObjectName = exteriorSpawnObjectName;
        IsPlayerHome = isPlayerHome;
    }

    public string BuildingId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ExteriorMapAssetName { get; set; } = string.Empty;

    public string EnterTriggerObjectName { get; set; } = string.Empty;

    public string InteriorMapAssetName { get; set; } = string.Empty;

    public string InteriorSpawnObjectName { get; set; } = string.Empty;

    public string ExitTriggerObjectName { get; set; } = string.Empty;

    public string ExteriorSpawnObjectName { get; set; } = string.Empty;

    public bool IsPlayerHome { get; set; }
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

internal sealed class NpcDefinitionSettings
{
    public NpcDefinitionSettings()
    {
    }

    public NpcDefinitionSettings(
        string npcId,
        string displayName,
        string mapAssetName,
        string spawnObjectName,
        float fallbackX,
        float fallbackY,
        string spriteSheetAssetName,
        int targetHeightInPixels,
        float interactionRange,
        List<string> dialogueLines)
    {
        NpcId = npcId;
        DisplayName = displayName;
        MapAssetName = mapAssetName;
        SpawnObjectName = spawnObjectName;
        FallbackX = fallbackX;
        FallbackY = fallbackY;
        SpriteSheetAssetName = spriteSheetAssetName;
        TargetHeightInPixels = targetHeightInPixels;
        InteractionRange = interactionRange;
        DialogueLines = dialogueLines;
    }

    public string NpcId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string MapAssetName { get; set; } = string.Empty;

    public string SpawnObjectName { get; set; } = string.Empty;

    public float FallbackX { get; set; }

    public float FallbackY { get; set; }

    public string SpriteSheetAssetName { get; set; } = "Person2";

    public int FrameWidth { get; set; } = 23;

    public int FrameHeight { get; set; } = 36;

    public int SourceOffsetX { get; set; } = 1;

    public int SourceOffsetY { get; set; } = 1;

    public int FeetBottomInset { get; set; } = 3;

    public int TargetHeightInPixels { get; set; } = 24;

    public float InteractionRange { get; set; } = 20f;

    public List<string> DialogueLines { get; set; } = [];
}

internal sealed class SkillSeedSettings
{
    public SkillSeedSettings()
    {
    }

    public SkillSeedSettings(string name, int level)
    {
        Name = name;
        Level = level;
    }

    public string Name { get; set; } = string.Empty;

    public int Level { get; set; } = 1;
}

internal sealed class SleepSpotSettings
{
    public SleepSpotSettings()
    {
    }

    public SleepSpotSettings(string mapAssetName, string triggerObjectName, float fallbackX, float fallbackY, float fallbackRadius, string promptText)
    {
        MapAssetName = mapAssetName;
        TriggerObjectName = triggerObjectName;
        FallbackX = fallbackX;
        FallbackY = fallbackY;
        FallbackRadius = fallbackRadius;
        PromptText = promptText;
    }

    public string MapAssetName { get; set; } = string.Empty;

    public string TriggerObjectName { get; set; } = string.Empty;

    public float FallbackX { get; set; }

    public float FallbackY { get; set; }

    public float FallbackRadius { get; set; } = 24f;

    public string PromptText { get; set; } = string.Empty;
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
