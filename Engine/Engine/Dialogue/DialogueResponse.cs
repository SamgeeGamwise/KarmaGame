using System.Collections.Generic;

namespace Engine.Dialogue;

public sealed record DialogueResponse(
    string ResponseId,
    string Text,
    string NextNodeId = "",
    bool CloseDialogue = false,
    IReadOnlyList<DialogueEffect>? Effects = null,
    DialogueCondition? Condition = null);
