using System;
using System.Collections.Generic;

namespace Engine.Dialogue;

public static class DialogueConditionEvaluator
{
    public static bool Matches(DialogueCondition? condition, DialogueContext context)
    {
        if (condition is null)
            return true;

        if (condition.RandomChance <= 0f)
            return false;

        if (condition.RandomChance < 1f && context.RandomSource.NextDouble() > condition.RandomChance)
            return false;

        if (condition.MinDayNumber.HasValue && context.DayNumber < condition.MinDayNumber.Value)
            return false;
        if (condition.MaxDayNumber.HasValue && context.DayNumber > condition.MaxDayNumber.Value)
            return false;

        if (!MatchesTimeWindow(condition, context.CurrentMinutes))
            return false;

        if (!MatchesRequiredList(condition.AllowedWeekdays, context.WeekdayName))
            return false;
        if (!MatchesRequiredList(condition.AllowedSeasons, context.SeasonName))
            return false;

        if (!ContainsAll(context.Flags, condition.RequiredFlags))
            return false;
        if (ContainsAny(context.Flags, condition.ExcludedFlags))
            return false;

        if (!ContainsAll(context.AcceptedQuestIds, condition.RequiredQuestIds))
            return false;
        if (ContainsAny(context.AcceptedQuestIds, condition.ExcludedQuestIds))
            return false;

        return true;
    }

    private static bool MatchesTimeWindow(DialogueCondition condition, int currentMinutes)
    {
        if (!condition.EarliestMinutes.HasValue && !condition.LatestMinutes.HasValue)
            return true;

        if (condition.EarliestMinutes.HasValue && !condition.LatestMinutes.HasValue)
            return currentMinutes >= condition.EarliestMinutes.Value;

        if (!condition.EarliestMinutes.HasValue && condition.LatestMinutes.HasValue)
            return currentMinutes <= condition.LatestMinutes.Value;

        int earliestMinutes = condition.EarliestMinutes!.Value;
        int latestMinutes = condition.LatestMinutes!.Value;

        if (earliestMinutes <= latestMinutes)
            return currentMinutes >= earliestMinutes && currentMinutes <= latestMinutes;

        return currentMinutes >= earliestMinutes || currentMinutes <= latestMinutes;
    }

    private static bool MatchesRequiredList(IReadOnlyList<string>? allowedValues, string currentValue)
    {
        if (allowedValues is null || allowedValues.Count == 0)
            return true;

        foreach (string allowedValue in allowedValues)
        {
            if (string.Equals(allowedValue, currentValue, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool ContainsAll(IReadOnlySet<string> haystack, IReadOnlyList<string>? needles)
    {
        if (needles is null || needles.Count == 0)
            return true;

        foreach (string needle in needles)
        {
            if (string.IsNullOrWhiteSpace(needle))
                continue;

            if (!haystack.Contains(needle.Trim()))
                return false;
        }

        return true;
    }

    private static bool ContainsAny(IReadOnlySet<string> haystack, IReadOnlyList<string>? needles)
    {
        if (needles is null || needles.Count == 0)
            return false;

        foreach (string needle in needles)
        {
            if (string.IsNullOrWhiteSpace(needle))
                continue;

            if (haystack.Contains(needle.Trim()))
                return true;
        }

        return false;
    }
}
