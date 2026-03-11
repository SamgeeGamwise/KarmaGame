using System.Collections.Generic;

namespace Sandbox.Game.Config;

internal sealed class DayNightSettings
{
    public int MinutesPerDay { get; set; } = 24 * 60;

    public int StartMinutes { get; set; } = 6 * 60;

    public int MinutesPerTick { get; set; } = 360;

    public float SecondsPerTick { get; set; } = 5f;

    public int ClockPanelPadding { get; set; } = 8;

    public List<DayNightTintKeyframeSettings> TintTimeline { get; set; } =
    [
        new(0, new RgbColorSettings(24, 32, 62), 0.44f),
        new(2 * 60, new RgbColorSettings(12, 18, 40), 0.56f),
        new(5 * 60, new RgbColorSettings(34, 42, 70), 0.30f),
        new(6 * 60, new RgbColorSettings(255, 214, 170), 0.16f),
        new(12 * 60, new RgbColorSettings(255, 255, 255), 0.04f),
        new(18 * 60, new RgbColorSettings(255, 188, 140), 0.14f),
        new(22 * 60, new RgbColorSettings(42, 50, 86), 0.32f)
    ];

    public static DayNightSettings CreateDefault() => new();
}

internal sealed class DayNightTintKeyframeSettings
{
    public DayNightTintKeyframeSettings()
    {
    }

    public DayNightTintKeyframeSettings(int minutes, RgbColorSettings color, float alpha)
    {
        Minutes = minutes;
        Color = color;
        Alpha = alpha;
    }

    public int Minutes { get; set; }

    public RgbColorSettings Color { get; set; } = new(255, 255, 255);

    public float Alpha { get; set; }
}
