namespace Sandbox.Game.Config;

internal sealed class CameraSettings
{
    public float ZoomSpeed { get; set; } = 0.25f;

    public float MinZoom { get; set; } = 0.5f;

    public float MaxZoom { get; set; } = 1.5f;

    public static CameraSettings CreateDefault() => new();
}
