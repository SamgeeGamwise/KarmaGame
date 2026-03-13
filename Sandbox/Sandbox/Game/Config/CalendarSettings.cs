using System.Collections.Generic;

namespace Sandbox.Game.Config;

internal sealed class CalendarSettings
{
    public int StartingDayNumber { get; set; } = 1;

    public List<string> WeekdayNames { get; set; } =
    [
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday",
        "Sunday"
    ];

    public List<SeasonSettings> Seasons { get; set; } =
    [
        new("Spring", 28),
        new("Summer", 28),
        new("Autumn", 28),
        new("Winter", 28)
    ];

    public static CalendarSettings CreateDefault() => new();
}

internal sealed class SeasonSettings
{
    public SeasonSettings()
    {
    }

    public SeasonSettings(string name, int dayCount)
    {
        Name = name;
        DayCount = dayCount;
    }

    public string Name { get; set; } = string.Empty;

    public int DayCount { get; set; } = 28;
}
