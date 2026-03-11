namespace Sandbox.Game.Config;

internal sealed class InteractionSettings
{
    public float NpcInteractionRange { get; set; } = 22f;

    public bool ShowInteractionHints { get; set; } = true;

    public float NotificationDurationSeconds { get; set; } = 2.6f;

    public static InteractionSettings CreateDefault() => new();
}
