namespace Engine.Core;

public sealed class TiledMapAuthoringProfile
{
    public string CollisionLayerName { get; init; } = "Collision";

    public string SpawnObjectLayerName { get; init; } = "Spawns";

    public string PlayerSpawnObjectName { get; init; } = "PlayerSpawn";

    public string[] BackgroundLayerNames { get; init; } = ["Ground", "GroundDetails", "Buildings"];

    public string[] ForegroundLayerNames { get; init; } = ["AbovePlayer"];

    public static TiledMapAuthoringProfile Default { get; } = new();
}
