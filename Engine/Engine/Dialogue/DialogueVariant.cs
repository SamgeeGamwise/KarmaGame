using System.Collections.Generic;

namespace Engine.Dialogue;

public sealed record DialogueVariant(
    string VariantId,
    int Priority,
    int Weight,
    string StartNodeId,
    IReadOnlyDictionary<string, DialogueNode> Nodes,
    DialogueCondition? Condition = null,
    string SpeakerName = "");
