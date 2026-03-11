using System.Collections.Generic;

namespace Sandbox.Game.Config;

internal sealed class SleepSettings
{
    public bool Enabled { get; set; } = true;

    public string SleepActionInputActionName { get; set; } = "action";

    public bool AllowSleepAnytime { get; set; } = false;

    public int EarliestSleepMinutes { get; set; } = 18 * 60;

    public int LatestSleepMinutes { get; set; } = 5 * 60;

    public int WakeMinutes { get; set; } = 6 * 60;

    public List<SleepSpotSettings> Spots { get; set; } =
    [
        new("HouseInterior", "BedSleepSpot", 160f, 140f, 18f, "Sleep in bed")
    ];

    public static SleepSettings CreateDefault() => new();
}

internal sealed class SleepSpotSettings
{
    public SleepSpotSettings()
    {
    }

    public SleepSpotSettings(string mapAssetName, string triggerObjectName, float fallbackX, float fallbackY, float fallbackRadius, string promptText)
    {
        MapAssetName = mapAssetName;
        TriggerObjectName = triggerObjectName;
        FallbackX = fallbackX;
        FallbackY = fallbackY;
        FallbackRadius = fallbackRadius;
        PromptText = promptText;
    }

    public string MapAssetName { get; set; } = string.Empty;

    public string TriggerObjectName { get; set; } = string.Empty;

    public float FallbackX { get; set; }

    public float FallbackY { get; set; }

    public float FallbackRadius { get; set; } = 24f;

    public string PromptText { get; set; } = string.Empty;
}
