using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Config;

namespace Sandbox.Game.Scene.Progression;

internal sealed class PlayerProgressState
{
    private readonly Dictionary<string, int> _skills = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _inventory = [];
    private readonly List<string> _loreEntries = [];

    private PlayerProgressState()
    {
    }

    public int Level { get; private set; } = 1;

    public int DayNumber { get; private set; } = 1;

    public int Money { get; private set; }

    public IReadOnlyList<string> Inventory => _inventory;

    public IReadOnlyDictionary<string, int> Skills => _skills;

    public IReadOnlyList<string> LoreEntries => _loreEntries;

    public static PlayerProgressState Create(ProgressionSettings progression, EconomySettings economy)
    {
        var state = new PlayerProgressState
        {
            Level = progression.StartingLevel,
            Money = economy.StartingMoney
        };

        foreach (string inventoryItem in progression.StartingInventory)
            state.AddInventoryItem(inventoryItem);
        foreach (SkillSeedSettings skill in progression.StartingSkills)
            state.SetSkillLevel(skill.Name, skill.Level);
        foreach (string loreEntry in progression.StartingLore)
            state.AddLoreEntry(loreEntry);

        return state;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        Money += amount;
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount <= 0 || amount > Money)
            return false;

        Money -= amount;
        return true;
    }

    public void AddInventoryItem(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            return;

        _inventory.Add(itemName.Trim());
    }

    public void AddLoreEntry(string loreEntry)
    {
        if (string.IsNullOrWhiteSpace(loreEntry))
            return;

        string trimmed = loreEntry.Trim();
        if (_loreEntries.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            return;

        _loreEntries.Add(trimmed);
    }

    public void SetSkillLevel(string skillName, int level)
    {
        if (string.IsNullOrWhiteSpace(skillName))
            return;

        _skills[skillName.Trim()] = Math.Max(1, level);
    }

    public void AdvanceDay()
    {
        DayNumber++;
    }
}
