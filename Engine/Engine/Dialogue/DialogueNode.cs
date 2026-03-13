using System.Collections.Generic;

namespace Engine.Dialogue;

public sealed record DialogueNode(
    string NodeId,
    string Text,
    IReadOnlyList<DialogueResponse> Responses,
    string NextNodeId = "",
    bool CloseAfter = false,
    DialogueCondition? Condition = null,
    string SpeakerName = "");
