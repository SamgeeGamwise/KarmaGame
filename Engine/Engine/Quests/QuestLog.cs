using System;
using System.Collections.Generic;

namespace Engine.Quests;

public sealed class QuestLog
{
    private readonly HashSet<string> _acceptedQuestIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<QuestLogEntry> _entries = [];

    public int AcceptedCount => _acceptedQuestIds.Count;

    public string LastAcceptedQuestId { get; private set; } = "none";

    public IReadOnlyList<QuestLogEntry> Entries => _entries;

    public IReadOnlyCollection<string> AcceptedQuestIds => _acceptedQuestIds;

    public bool HasAccepted(string questId)
    {
        return !string.IsNullOrWhiteSpace(questId) && _acceptedQuestIds.Contains(questId.Trim());
    }

    public bool TryAccept(string questId)
    {
        if (string.IsNullOrWhiteSpace(questId))
            return false;

        return TryAccept(new QuestLogEntry(questId.Trim(), questId.Trim(), string.Empty, string.Empty));
    }

    public bool TryAccept(QuestLogEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.QuestId))
            return false;

        string normalizedQuestId = entry.QuestId.Trim();
        if (!_acceptedQuestIds.Add(normalizedQuestId))
            return false;

        _entries.Add(entry with { QuestId = normalizedQuestId });
        LastAcceptedQuestId = normalizedQuestId;
        return true;
    }
}
