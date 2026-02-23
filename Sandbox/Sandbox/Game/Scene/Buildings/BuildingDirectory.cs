using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Config;

namespace Sandbox.Game.Scene.Buildings;

internal sealed class BuildingDirectory
{
    private readonly List<BuildingSettings> _buildings;
    private readonly Dictionary<string, BuildingSettings> _byEnterKey;
    private readonly Dictionary<string, BuildingSettings> _byInteriorMap;

    public BuildingDirectory(IEnumerable<BuildingSettings> buildings)
    {
        _buildings = buildings.ToList();
        _byEnterKey = new Dictionary<string, BuildingSettings>(StringComparer.Ordinal);
        _byInteriorMap = new Dictionary<string, BuildingSettings>(StringComparer.Ordinal);

        foreach (BuildingSettings building in _buildings)
        {
            string enterKey = BuildPortalKey(building.ExteriorMapAssetName, building.EnterTriggerObjectName);
            _byEnterKey[enterKey] = building;
            if (!_byInteriorMap.ContainsKey(building.InteriorMapAssetName))
                _byInteriorMap.Add(building.InteriorMapAssetName, building);
        }
    }

    public IReadOnlyList<BuildingSettings> Buildings => _buildings;

    public bool TryGetBuildingByEnterTrigger(string sourceMapAssetName, string triggerObjectName, out BuildingSettings? building)
    {
        return _byEnterKey.TryGetValue(BuildPortalKey(sourceMapAssetName, triggerObjectName), out building);
    }

    public bool TryGetBuildingByInteriorMap(string mapAssetName, out BuildingSettings? building)
    {
        return _byInteriorMap.TryGetValue(mapAssetName, out building);
    }

    private static string BuildPortalKey(string sourceMapAssetName, string triggerObjectName)
    {
        return $"{sourceMapAssetName}::{triggerObjectName}";
    }
}
