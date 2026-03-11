using Microsoft.Xna.Framework;

namespace Sandbox.Game.Config;

internal sealed class RenderSettings
{
    public RgbColorSettings ClearColor { get; set; } = new(24, 29, 38);

    public static RenderSettings CreateDefault() => new();
}

internal sealed class RgbColorSettings
{
    public RgbColorSettings()
    {
    }

    public RgbColorSettings(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public byte R { get; set; }

    public byte G { get; set; }

    public byte B { get; set; }

    public Color ToColor() => new(R, G, B);
}
