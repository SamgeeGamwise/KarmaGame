using System;
using System.Collections.Generic;

namespace Engine.Dialogue;

public sealed record DialogueContext(
    int CurrentMinutes,
    int DayNumber,
    string WeekdayName,
    string SeasonName,
    IReadOnlySet<string> AcceptedQuestIds,
    IReadOnlySet<string> Flags,
    Random RandomSource);
