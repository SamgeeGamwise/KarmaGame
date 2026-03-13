using System.Collections.Generic;

namespace Sandbox.Game.Config;

internal sealed class DialogueSettings
{
    public List<DialogueConversationSettings> Conversations { get; set; } = [];

    public List<DialogueTriggerSettings> Triggers { get; set; } = [];

    public static DialogueSettings CreateDefault() => new();
}

internal sealed class DialogueConversationSettings
{
    public string ConversationId { get; set; } = string.Empty;

    public string SpeakerName { get; set; } = string.Empty;

    public List<DialogueVariantSettings> Variants { get; set; } = [];
}

internal sealed class DialogueVariantSettings
{
    public string VariantId { get; set; } = string.Empty;

    public int Priority { get; set; }

    public int Weight { get; set; } = 1;

    public string SpeakerName { get; set; } = string.Empty;

    public string StartNodeId { get; set; } = string.Empty;

    public DialogueConditionSettings Conditions { get; set; } = new();

    public List<DialogueNodeSettings> Nodes { get; set; } = [];
}

internal sealed class DialogueNodeSettings
{
    public string NodeId { get; set; } = string.Empty;

    public string SpeakerName { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string NextNodeId { get; set; } = string.Empty;

    public bool CloseAfter { get; set; }

    public DialogueConditionSettings Conditions { get; set; } = new();

    public List<DialogueResponseSettings> Responses { get; set; } = [];
}

internal sealed class DialogueResponseSettings
{
    public string ResponseId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string NextNodeId { get; set; } = string.Empty;

    public bool CloseDialogue { get; set; }

    public DialogueConditionSettings Conditions { get; set; } = new();

    public List<DialogueEffectSettings> Effects { get; set; } = [];
}

internal sealed class DialogueEffectSettings
{
    public string EffectType { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Extra { get; set; } = string.Empty;
}

internal sealed class DialogueTriggerSettings
{
    public string TriggerId { get; set; } = string.Empty;

    public int Priority { get; set; }

    public string MapAssetName { get; set; } = string.Empty;

    public string ConversationId { get; set; } = string.Empty;

    public string SpeakerName { get; set; } = string.Empty;

    public string TriggerObjectName { get; set; } = string.Empty;

    public float FallbackX { get; set; }

    public float FallbackY { get; set; }

    public float InteractionRadius { get; set; } = 32f;
}

internal sealed class DialogueConditionSettings
{
    public int? EarliestMinutes { get; set; }

    public int? LatestMinutes { get; set; }

    public int? MinDayNumber { get; set; }

    public int? MaxDayNumber { get; set; }

    public float RandomChance { get; set; } = 1f;

    public List<string> AllowedWeekdays { get; set; } = [];

    public List<string> AllowedSeasons { get; set; } = [];

    public List<string> RequiredFlags { get; set; } = [];

    public List<string> ExcludedFlags { get; set; } = [];

    public List<string> RequiredQuestIds { get; set; } = [];

    public List<string> ExcludedQuestIds { get; set; } = [];
}
