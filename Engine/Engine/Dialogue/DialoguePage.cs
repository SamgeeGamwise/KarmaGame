using System.Collections.Generic;

namespace Engine.Dialogue;

public sealed record DialoguePage(
    string SpeakerName,
    string Text,
    IReadOnlyList<DialogueResponse> Responses);
