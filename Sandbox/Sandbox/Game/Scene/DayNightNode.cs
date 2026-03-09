using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sandbox.Game.Config;

namespace Sandbox.Game;

internal sealed class DayNightNode(DayNightSettings settings)
{
    private readonly DayNightSettings _settings = settings;
    private readonly TintKeyframe[] _tintTimeline = settings.TintTimeline
            .Select(t => new TintKeyframe(t.Minutes, t.Color.ToColor(), t.Alpha))
            .ToArray();
    private Texture2D _pixel = null!;
    private SpriteFont _clockFont = null!;
    private int _currentMinutes = settings.StartMinutes;
    private float _tickTimer;

    public int CurrentMinutes => _currentMinutes;

    public string CurrentClockText => FormatClock(_currentMinutes);

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _clockFont = content.Load<SpriteFont>("UIFont");
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void Update(float deltaSeconds, bool isPaused = false)
    {
        if (isPaused)
            return;

        _tickTimer += deltaSeconds;
        while (_tickTimer >= _settings.SecondsPerTick)
        {
            _tickTimer -= _settings.SecondsPerTick;
            _currentMinutes = (_currentMinutes + _settings.MinutesPerTick) % _settings.MinutesPerDay;
        }
    }

    public void SetCurrentMinutes(int minutes)
    {
        _currentMinutes = Math.Clamp(minutes, 0, _settings.MinutesPerDay - 1);
        _tickTimer = 0f;
    }

    public bool IsWithinRange(int startMinutes, int endMinutes)
    {
        if (startMinutes == endMinutes)
            return true;

        if (startMinutes < endMinutes)
            return _currentMinutes >= startMinutes && _currentMinutes <= endMinutes;

        return _currentMinutes >= startMinutes || _currentMinutes <= endMinutes;
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
        int panelPadding = _settings.ClockPanelPadding;
        Rectangle panelRect = new(
            screenWidth - (int)textSize.X - panelPadding * 2 - 16,
            16,
            (int)textSize.X + panelPadding * 2,
            (int)textSize.Y + panelPadding * 2);

        spriteBatch.Draw(_pixel, panelRect, Color.Black * 0.45f);
        spriteBatch.DrawString(_clockFont, clockText, new Vector2(panelRect.X + panelPadding, panelRect.Y + panelPadding), Color.White);
    }

    private TintSample ComputeTint(int minutes)
    {
        if (_tintTimeline.Length == 0)
            return new TintSample(Color.White, 0f);

        for (int i = 0; i < _tintTimeline.Length; i++)
        {
            TintKeyframe from = _tintTimeline[i];
            TintKeyframe to = i == _tintTimeline.Length - 1
                ? new TintKeyframe(_tintTimeline[0].Minutes + _settings.MinutesPerDay, _tintTimeline[0].Color, _tintTimeline[0].Alpha)
                : _tintTimeline[i + 1];

            int adjustedMinutes = minutes;
            if (to.Minutes > _settings.MinutesPerDay && adjustedMinutes < from.Minutes)
                adjustedMinutes += _settings.MinutesPerDay;

            if (adjustedMinutes < from.Minutes || adjustedMinutes > to.Minutes)
                continue;

            float t = (adjustedMinutes - from.Minutes) / (float)(to.Minutes - from.Minutes);
            Color color = Color.Lerp(from.Color, to.Color, t);
            float alpha = MathHelper.Lerp(from.Alpha, to.Alpha, t);
            return new TintSample(color, alpha);
        }

        TintKeyframe fallback = _tintTimeline[0];
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
