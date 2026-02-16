using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox.Game;

internal sealed class DayNightNode
{
    private const int MinutesPerDay = 24 * 60;
    private const int MidnightMinutes = 0;
    private const int DayStartMinutes = 6 * 60;
    private const int DayEndMinutes = 2 * 60;
    private const int MinutesPerTick = 180;
    private const float SecondsPerTick = 5f;

    private static readonly TintKeyframe[] TintTimeline =
    [
        new(MidnightMinutes, new Color(24, 32, 62), 0.44f),
        new(DayEndMinutes, new Color(12, 18, 40), 0.56f),      // 2:00 AM (night starts)
        new(5 * 60, new Color(34, 42, 70), 0.30f),             // 5:00 AM
        new(DayStartMinutes, new Color(255, 214, 170), 0.16f), // 6:00 AM (day starts)
        new(12 * 60, new Color(255, 255, 255), 0.04f),         // Noon
        new(18 * 60, new Color(255, 188, 140), 0.14f),         // 6:00 PM
        new(22 * 60, new Color(42, 50, 86), 0.32f),            // 10:00 PM
    ];

    private Texture2D _pixel = null!;
    private SpriteFont _clockFont = null!;
    private int _currentMinutes = DayStartMinutes;
    private float _tickTimer;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _clockFont = content.Load<SpriteFont>("UIFont");
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void Update(float deltaSeconds)
    {
        _tickTimer += deltaSeconds;
        while (_tickTimer >= SecondsPerTick)
        {
            _tickTimer -= SecondsPerTick;
            _currentMinutes = (_currentMinutes + MinutesPerTick) % MinutesPerDay;
        }
    }

    public void DrawScreen(SpriteBatch spriteBatch, int _virtualWidth, int _virtualHeight)
    {
        Viewport viewport = spriteBatch.GraphicsDevice.Viewport;
        int screenWidth = viewport.Width;
        int screenHeight = viewport.Height;

        TintSample tint = ComputeTint(_currentMinutes);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, screenWidth, screenHeight), tint.Color * tint.Alpha);

        string clockText = FormatClock(_currentMinutes);
        Vector2 textSize = _clockFont.MeasureString(clockText);
        const int panelPadding = 8;
        Rectangle panelRect = new(
            screenWidth - (int)textSize.X - panelPadding * 2 - 16,
            16,
            (int)textSize.X + panelPadding * 2,
            (int)textSize.Y + panelPadding * 2);

        spriteBatch.Draw(_pixel, panelRect, Color.Black * 0.45f);
        spriteBatch.DrawString(_clockFont, clockText, new Vector2(panelRect.X + panelPadding, panelRect.Y + panelPadding), Color.White);
    }

    private static TintSample ComputeTint(int minutes)
    {
        if (TintTimeline.Length == 0)
            return new TintSample(Color.White, 0f);

        for (int i = 0; i < TintTimeline.Length; i++)
        {
            TintKeyframe from = TintTimeline[i];
            TintKeyframe to = i == TintTimeline.Length - 1
                ? new TintKeyframe(TintTimeline[0].Minutes + MinutesPerDay, TintTimeline[0].Color, TintTimeline[0].Alpha)
                : TintTimeline[i + 1];

            int adjustedMinutes = minutes;
            if (to.Minutes > MinutesPerDay && adjustedMinutes < from.Minutes)
                adjustedMinutes += MinutesPerDay;

            if (adjustedMinutes < from.Minutes || adjustedMinutes > to.Minutes)
                continue;

            float t = (adjustedMinutes - from.Minutes) / (float)(to.Minutes - from.Minutes);
            Color color = Color.Lerp(from.Color, to.Color, t);
            float alpha = MathHelper.Lerp(from.Alpha, to.Alpha, t);
            return new TintSample(color, alpha);
        }

        TintKeyframe fallback = TintTimeline[0];
        return new TintSample(fallback.Color, fallback.Alpha);
    }

    private static string FormatClock(int minutes)
    {
        int hour24 = minutes / 60;
        int minute = minutes % 60;
        int hour12 = hour24 % 12;
        if (hour12 == 0)
            hour12 = 12;

        string meridiem = hour24 < 12 ? "AM" : "PM";
        return $"{hour12:00}:{minute:00} {meridiem}";
    }

    private readonly record struct TintKeyframe(int Minutes, Color Color, float Alpha);

    private readonly record struct TintSample(Color Color, float Alpha);
}
