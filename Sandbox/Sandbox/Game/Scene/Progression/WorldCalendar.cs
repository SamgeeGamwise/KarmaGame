using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Dialogue;
using Sandbox.Game.Config;

namespace Sandbox.Game.Scene.Progression;

internal sealed class WorldCalendar
{
    private readonly CalendarSettings _settings;
    private readonly string[] _weekdayNames;
    private readonly SeasonSettings[] _seasons;
    private readonly int _daysPerYear;

    public WorldCalendar(CalendarSettings settings)
    {
        _settings = settings;
        _weekdayNames = settings.WeekdayNames.Count == 0
            ? ["Monday"]
            : settings.WeekdayNames.ToArray();
        _seasons = settings.Seasons.Count == 0
            ? [new SeasonSettings("Unknown", 1)]
            : settings.Seasons.ToArray();

        int totalDays = 0;
        foreach (SeasonSettings season in _seasons)
            totalDays += Math.Max(1, season.DayCount);
        _daysPerYear = Math.Max(1, totalDays);
    }

    public int StartingDayNumber => Math.Max(1, _settings.StartingDayNumber);

    public string GetWeekdayName(int dayNumber)
    {
        int normalizedDay = Math.Max(1, dayNumber);
        int index = (normalizedDay - 1) % _weekdayNames.Length;
        return _weekdayNames[index];
    }

    public string GetSeasonName(int dayNumber)
    {
        int normalizedDay = Math.Max(1, dayNumber);
        int dayOfYear = (normalizedDay - 1) % _daysPerYear;
        int runningDay = 0;

        foreach (SeasonSettings season in _seasons)
        {
            int length = Math.Max(1, season.DayCount);
            if (dayOfYear < runningDay + length)
                return season.Name;

            runningDay += length;
        }

        return _seasons[0].Name;
    }

    public DialogueContext BuildDialogueContext(int currentMinutes, PlayerProgressState progressState, Random randomSource)
    {
        var acceptedQuestIds = new HashSet<string>(progressState.Quests.AcceptedQuestIds, StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(progressState.Flags, StringComparer.OrdinalIgnoreCase);

        return new DialogueContext(
            currentMinutes,
            progressState.DayNumber,
            GetWeekdayName(progressState.DayNumber),
            GetSeasonName(progressState.DayNumber),
            acceptedQuestIds,
            flags,
            randomSource);
    }
}
