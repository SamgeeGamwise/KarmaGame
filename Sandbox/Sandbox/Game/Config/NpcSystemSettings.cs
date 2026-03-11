using System.Collections.Generic;

namespace Sandbox.Game.Config;

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
