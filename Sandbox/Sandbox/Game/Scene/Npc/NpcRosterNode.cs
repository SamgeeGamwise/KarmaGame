using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Sandbox.Game.Config;

namespace Sandbox.Game.Scene.Npc;

internal sealed class NpcRosterNode(NpcSystemSettings settings, InteractionSettings interactionSettings)
{
    private readonly List<NpcNode> _npcs = settings.Definitions
            .Select(definition => new NpcNode(definition))
            .ToList();
    private readonly float _defaultInteractionRange = interactionSettings.NpcInteractionRange;

    public void LoadContent(ContentManager content, IReadOnlyDictionary<string, MapNode> mapsByAssetName)
    {
        foreach (NpcNode npc in _npcs)
        {
            npc.LoadContent(content);
            npc.SetFeetPosition(ResolveSpawnPosition(npc, mapsByAssetName));
        }
    }

    public IEnumerable<NpcNode> GetNpcsForMap(string mapAssetName)
    {
        return _npcs.Where(npc => string.Equals(npc.MapAssetName, mapAssetName, StringComparison.Ordinal));
    }

    public bool TryFindInteractableNpc(string mapAssetName, Vector2 playerFeetPosition, out NpcNode? interactableNpc)
    {
        interactableNpc = null;
        float bestDistanceSquared = float.MaxValue;

        foreach (NpcNode npc in GetNpcsForMap(mapAssetName))
        {
            if (!npc.IsInInteractionRange(playerFeetPosition, _defaultInteractionRange))
                continue;

            float distanceSquared = Vector2.DistanceSquared(playerFeetPosition, npc.FeetPosition);
            if (distanceSquared >= bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            interactableNpc = npc;
        }

        return interactableNpc is not null;
    }

    private static Vector2 ResolveSpawnPosition(NpcNode npc, IReadOnlyDictionary<string, MapNode> mapsByAssetName)
    {
        if (!mapsByAssetName.TryGetValue(npc.MapAssetName, out MapNode? mapNode))
            return npc.FallbackFeetPosition;

        if (!string.IsNullOrWhiteSpace(npc.SpawnObjectName) &&
            mapNode.TryGetObjectAnchorPosition(npc.SpawnObjectName, out Vector2 objectSpawn))
        {
            return objectSpawn;
        }

        if (npc.FallbackFeetPosition != Vector2.Zero)
            return npc.FallbackFeetPosition;

        if (mapNode.TryGetPlayerSpawn(out Vector2 playerSpawn))
            return playerSpawn;

        return new Vector2(120f, 120f);
    }
}
