namespace Sandbox.Game.Config;

internal sealed class EconomySettings
{
    public int StartingMoney { get; set; } = 125;

    public static EconomySettings CreateDefault() => new();
}
