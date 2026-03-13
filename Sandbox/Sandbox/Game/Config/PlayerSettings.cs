namespace Sandbox.Game.Config;

internal sealed class PlayerSettings
{
    public string SpriteSheetAssetName { get; set; } = "Characters/Person2";

    public int TargetHeightInPixels { get; set; } = 72;

    public float WalkFramesPerSecond { get; set; } = 8f;

    public float MoveSpeed { get; set; } = 300f;
    
    public float RunSpeed { get; set; } = 400f;
    
    // public float MoveSpeed { get; set; } = 150f;
    //
    // public float RunSpeed { get; set; } = 200f;

    public int CollisionWidth { get; set; } = 41;

    public int CollisionHeight { get; set; } = 18;

    public int CollisionBottomInset { get; set; } = 1;

    public int DoorInteractionWidth { get; set; } = 9;

    public int DoorInteractionHeight { get; set; } = 17;

    public static PlayerSettings CreateDefault() => new();
}
