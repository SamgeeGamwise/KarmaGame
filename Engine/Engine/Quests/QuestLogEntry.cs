namespace Engine.Quests;

public sealed record QuestLogEntry(
    string QuestId,
    string Title,
    string SourceName,
    string Summary);
