namespace Engine.Dialogue;

public sealed record DialogueEffect(
    DialogueActionType ActionType,
    string Value,
    string Extra = "");
