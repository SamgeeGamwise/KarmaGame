using System.Collections.Generic;

namespace Sandbox.Game.Config;

internal sealed class ProgressionSettings
{
    public int StartingLevel { get; set; } = 1;

    public List<string> StartingInventory { get; set; } =
    [
        "Bedroll",
        "Copper Key",
        "Starter Pickaxe"
    ];

    public List<SkillSeedSettings> StartingSkills { get; set; } =
    [
        new("Foraging", 1),
        new("Crafting", 1),
        new("Negotiation", 1)
    ];

    public List<string> StartingLore { get; set; } =
    [
        "The settlement was rebuilt after the Silent Storm.",
        "Most buildings share a common interior while the district expands."
    ];

    public static ProgressionSettings CreateDefault() => new();
}

internal sealed class SkillSeedSettings
{
    public SkillSeedSettings()
    {
    }

    public SkillSeedSettings(string name, int level)
    {
        Name = name;
        Level = level;
    }

    public string Name { get; set; } = string.Empty;

    public int Level { get; set; } = 1;
}
