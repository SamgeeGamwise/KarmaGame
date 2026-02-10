using System;

namespace Engine.Collision;

/// <summary>
/// Bitmask layers for collision filtering.
/// </summary>
[Flags]
public enum CollisionLayer
{
    /// <summary>No layer.</summary>
    None = 0,

    /// <summary>Static world geometry.</summary>
    World = 1 << 0,

    /// <summary>General actor.</summary>
    Actor = 1 << 1,

    /// <summary>Projectiles.</summary>
    Projectile = 1 << 2,

    /// <summary>Player-controlled entities.</summary>
    Player = 1 << 3,

    /// <summary>NPC entities.</summary>
    Npc = 1 << 4,

    /// <summary>Trigger volumes.</summary>
    Trigger = 1 << 5,

    /// <summary>All layers.</summary>
    All = ~0
}
