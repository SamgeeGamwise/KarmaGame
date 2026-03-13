using System.Collections.Generic;

namespace Engine.Dialogue;

public sealed record DialogueStepResult(
    bool Closed,
    IReadOnlyList<DialogueEffect> Effects);
