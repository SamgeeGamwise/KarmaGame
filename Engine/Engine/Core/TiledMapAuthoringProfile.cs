namespace Engine.Core;

public sealed class TiledMapAuthoringProfile
{
    public string CollisionLayerName { get; init; } = "Collision";

    public string SpawnObjectLayerName { get; init; } = "Spawns";

    public string PlayerSpawnObjectName { get; init; } = "PlayerSpawn";

    public string[] BackgroundLayerNames { get; init; } = ["Ground", "Collision", "GroundDetails", "Buildings"];

    public string[] YSortForegroundLayerNames { get; init; } = ["AbovePlayer"];

    public string[] ForegroundLayerNames { get; init; } = [];

    public static TiledMapAuthoringProfile Default { get; } = new();
}
