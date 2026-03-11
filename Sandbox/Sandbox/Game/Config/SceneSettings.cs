using System.Collections.Generic;

namespace Sandbox.Game.Config;

internal sealed class SceneSettings
{
    public string StartingMapAssetName { get; set; } = "Maps/GameMap";

    public string ActionInputActionName { get; set; } = "action";

    public string ExitInputActionName { get; set; } = "exit";

    public float CameraZoom { get; set; } = 1f;

    public float PortalTransitionCooldownSeconds { get; set; } = 0.35f;

    public bool FreezeWorldWhileMenuOpen { get; set; } = true;

    public bool FreezeWorldWhileDialogueOpen { get; set; } = true;

    public List<PortalSettings> Portals { get; set; } =
    [
        new("Maps/GameMap", "DoorToHospital", "Maps/HospitalInterior", "HospitalFromTown"),
        new("Maps/HospitalInterior", "DoorToTown", "Maps/GameMap", "TownFromHospital")
    ];

    public List<BuildingSettings> Buildings { get; set; } =
    [
        new(
            "home",
            "Player Home",
            "Maps/GameMap",
            "DoorToHouse",
            "Maps/HospitalInterior",
            "HouseFromTown",
            "DoorToTown",
            "TownFromHouse",
            true),
        new(
            "blacksmith",
            "Placeholder Blacksmith",
            "Maps/GameMap",
            "DoorToBlacksmith",
            "Maps/HospitalInterior",
            "HouseFromTown",
            "DoorToTown",
            "TownFromBlacksmith",
            false),
        new(
            "library",
            "Placeholder Library",
            "Maps/GameMap",
            "DoorToLibrary",
            "Maps/HospitalInterior",
            "HouseFromTown",
            "DoorToTown",
            "TownFromLibrary",
            false)
    ];

    public static SceneSettings CreateDefault() => new();
}

internal sealed class PortalSettings
{
    public PortalSettings()
    {
    }

    public PortalSettings(string sourceMapAssetName, string triggerObjectName, string targetMapAssetName, string targetSpawnObjectName)
    {
        SourceMapAssetName = sourceMapAssetName;
        TriggerObjectName = triggerObjectName;
        TargetMapAssetName = targetMapAssetName;
        TargetSpawnObjectName = targetSpawnObjectName;
    }

    public string SourceMapAssetName { get; set; } = string.Empty;

    public string TriggerObjectName { get; set; } = string.Empty;

    public string TargetMapAssetName { get; set; } = string.Empty;

    public string TargetSpawnObjectName { get; set; } = string.Empty;
}

internal sealed class BuildingSettings
{
    public BuildingSettings()
    {
    }

    public BuildingSettings(
        string buildingId,
        string displayName,
        string exteriorMapAssetName,
        string enterTriggerObjectName,
        string interiorMapAssetName,
        string interiorSpawnObjectName,
        string exitTriggerObjectName,
        string exteriorSpawnObjectName,
        bool isPlayerHome)
    {
        BuildingId = buildingId;
        DisplayName = displayName;
        ExteriorMapAssetName = exteriorMapAssetName;
        EnterTriggerObjectName = enterTriggerObjectName;
        InteriorMapAssetName = interiorMapAssetName;
        InteriorSpawnObjectName = interiorSpawnObjectName;
        ExitTriggerObjectName = exitTriggerObjectName;
        ExteriorSpawnObjectName = exteriorSpawnObjectName;
        IsPlayerHome = isPlayerHome;
    }

    public string BuildingId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ExteriorMapAssetName { get; set; } = string.Empty;

    public string EnterTriggerObjectName { get; set; } = string.Empty;

    public string InteriorMapAssetName { get; set; } = string.Empty;

    public string InteriorSpawnObjectName { get; set; } = string.Empty;

    public string ExitTriggerObjectName { get; set; } = string.Empty;

    public string ExteriorSpawnObjectName { get; set; } = string.Empty;

    public bool IsPlayerHome { get; set; }
}
