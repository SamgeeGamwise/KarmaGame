namespace Sandbox.Game.Config;

internal sealed class PlayerSettings
{
    public string SpriteSheetAssetName { get; set; } = "Person2";

    public int TargetHeightInPixels { get; set; } = 64;

    public float WalkFramesPerSecond { get; set; } = 8f;

    public float MoveSpeed { get; set; } = 200f;

    public float RunSpeed { get; set; } = 250f;

    public int CollisionWidth { get; set; } = 10;

    public int CollisionHeight { get; set; } = 9;

    public int CollisionBottomInset { get; set; } = 3;

    public int DoorInteractionWidth { get; set; } = 9;

    public int DoorInteractionHeight { get; set; } = 17;

    public static PlayerSettings CreateDefault() => new();
}
