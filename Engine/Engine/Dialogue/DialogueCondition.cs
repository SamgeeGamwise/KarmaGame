using System.Collections.Generic;

namespace Engine.Dialogue;

public sealed record DialogueCondition(
    int? EarliestMinutes = null,
    int? LatestMinutes = null,
    int? MinDayNumber = null,
    int? MaxDayNumber = null,
    IReadOnlyList<string>? AllowedWeekdays = null,
    IReadOnlyList<string>? AllowedSeasons = null,
    IReadOnlyList<string>? RequiredFlags = null,
    IReadOnlyList<string>? ExcludedFlags = null,
    IReadOnlyList<string>? RequiredQuestIds = null,
    IReadOnlyList<string>? ExcludedQuestIds = null,
    float RandomChance = 1f);
