using System.Collections.Generic;

namespace Sandbox.Game.Config;

internal sealed class NpcSystemSettings
{
    public List<NpcDefinitionSettings> Definitions { get; set; } =
    [
        new(
            "npc_guard",
            "Crackhead",
            "Maps/GameMap",
            string.Empty,
            2650f,
            675f,
            "Characters/NPCs/ShopClerk",
            72,
            22f,
            [
                "Morning. Keep your coin purse close."
            ])
        {
            DialogueConversationId = "npc_guard_conversation",
            FrameWidth = 32,
            FrameHeight = 64,
            SourceOffsetX = 0,
            SourceOffsetY = 0
        },
        new(
            "npc_farmer",
            "Shop Keeper",
            "Maps/GameMap",
            string.Empty,
            336f,
            1000f,
            "Characters/Person2",
            72,
            22f,
            [
                "Need anything? I am here for you!"
            ])
        {
            DialogueConversationId = "npc_shop_keeper_conversation"
        },
        new(
            "npc_scholar",
            "Doctor",
            "Maps/HospitalInterior",
            string.Empty,
            96f,
            96f,
            "Characters/Person2",
            72,
            24f,
            [
                "Things are not looking good.",
                "You should go in and be with her before it is too late."
            ])
        {
            DialogueConversationId = "npc_doctor_conversation"
        }
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

    public string DialogueConversationId { get; set; } = string.Empty;

    public float FallbackX { get; set; }

    public float FallbackY { get; set; }

    public string SpriteSheetAssetName { get; set; } = "Characters/Person2";

    public int FrameWidth { get; set; } = 23;

    public int FrameHeight { get; set; } = 36;

    public int SourceOffsetX { get; set; } = 1;

    public int SourceOffsetY { get; set; } = 1;

    public int FeetBottomInset { get; set; } = 3;

    public int TargetHeightInPixels { get; set; } = 24;

    public float InteractionRange { get; set; } = 20f;

    public List<string> DialogueLines { get; set; } = [];

    public NpcQuestOfferSettings? QuestOffer { get; set; }
}

internal sealed class NpcQuestOfferSettings
{
    public string QuestId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string OfferText { get; set; } = string.Empty;

    public string AcceptedText { get; set; } = "Quest accepted.";

    public string DeclinedText { get; set; } = "Maybe later.";

    public string AlreadyAcceptedText { get; set; } = "You already accepted that quest.";
}
