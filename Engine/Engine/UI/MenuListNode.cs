using Engine.Core;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI;

/// <summary>
/// Simple vertical menu node with keyboard and mouse navigation.
/// </summary>
/// <remarks>
/// Requires a font and 1x1 pixel texture to be registered in <see cref="Core.EngineContext.Assets"/>.
/// </remarks>
public sealed class MenuListNode : Node
{
    private readonly List<MenuEntry> _entries = [];

    /// <summary>
    /// Creates a new menu list rendered in screen space.
    /// </summary>
    public MenuListNode()
    {
        RenderSpace = RenderSpace.Screen;
    }

    /// <summary>
    /// Gets entries.
    /// </summary>
    public IReadOnlyList<MenuEntry> Entries => _entries;

    /// <summary>
    /// Gets or sets top-left menu position in virtual coordinates.
    /// </summary>
    public Point Position { get; set; } = new(200, 80);

    /// <summary>
    /// Gets or sets entry width in pixels.
    /// </summary>
    public int EntryWidth { get; set; } = 240;

    /// <summary>
    /// Gets or sets entry height in pixels.
    /// </summary>
    public int EntryHeight { get; set; } = 40;

    /// <summary>
    /// Gets or sets spacing between entries in pixels.
    /// </summary>
    public int EntryGap { get; set; } = 10;

    /// <summary>
    /// Gets or sets selected entry index.
    /// </summary>
    public int SelectedIndex { get; private set; }

    /// <summary>
    /// Gets or sets action name used for moving selection up.
    /// </summary>
    public string UpAction { get; set; } = "ui_up";

    /// <summary>
    /// Gets or sets action name used for moving selection down.
    /// </summary>
    public string DownAction { get; set; } = "ui_down";

    /// <summary>
    /// Gets or sets action name used for confirming selection.
    /// </summary>
    public string AcceptAction { get; set; } = "ui_accept";

    /// <summary>
    /// Gets or sets action name used for cancel/back.
    /// </summary>
    public string CancelAction { get; set; } = "ui_cancel";

    /// <summary>
    /// Gets or sets font key from <see cref="Engine.Assets.AssetSet"/>.
    /// </summary>
    public string FontKey { get; set; } = "menu";

    /// <summary>
    /// Gets or sets pixel texture key from <see cref="Engine.Assets.AssetSet"/>.
    /// </summary>
    public string PixelKey { get; set; } = "pixel";

    /// <summary>
    /// Gets or sets default button color.
    /// </summary>
    public Color ButtonColor { get; set; } = Color.DimGray;

    /// <summary>
    /// Gets or sets hover/selected color.
    /// </summary>
    public Color ButtonHoverColor { get; set; } = Color.DarkSlateGray;

    /// <summary>
    /// Gets or sets border color.
    /// </summary>
    public Color BorderColor { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets text color.
    /// </summary>
    public Color TextColor { get; set; } = Color.White;

    /// <summary>
    /// Raised when selected index changes.
    /// </summary>
    public event Action<int>? SelectionChanged;

    /// <summary>
    /// Raised when cancel is requested.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Sets menu entries.
    /// </summary>
    public void SetEntries(IEnumerable<MenuEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        _entries.Clear();
        _entries.AddRange(entries);
        SelectedIndex = Math.Clamp(SelectedIndex, 0, Math.Max(0, _entries.Count - 1));
    }

    /// <summary>
    /// Adds a menu entry.
    /// </summary>
    public void AddEntry(string text, Action action)
    {
        _entries.Add(new MenuEntry(text, action));
        SelectedIndex = Math.Clamp(SelectedIndex, 0, Math.Max(0, _entries.Count - 1));
    }

    /// <inheritdoc />
    protected override void OnUpdate(EngineContext context)
    {
        if (_entries.Count == 0)
            return;

        bool moved = false;

        if (context.Input.Pressed(DownAction))
        {
            SelectedIndex = (SelectedIndex + 1) % _entries.Count;
            moved = true;
        }

        if (context.Input.Pressed(UpAction))
        {
            SelectedIndex = (SelectedIndex - 1 + _entries.Count) % _entries.Count;
            moved = true;
        }

        if (moved)
            SelectionChanged?.Invoke(SelectedIndex);

        Point? mouseVirtual = context.RawInput.GetMouseVirtualPos(
            context.VirtualDestination,
            context.VirtualWidth,
            context.VirtualHeight);

        if (mouseVirtual.HasValue)
        {
            int index = EntryIndexAt(mouseVirtual.Value);
            if (index >= 0 && index < _entries.Count && index != SelectedIndex)
            {
                SelectedIndex = index;
                SelectionChanged?.Invoke(SelectedIndex);
            }

            if (index >= 0 && context.RawInput.MousePressed(Input.MouseButton.Left))
                _entries[index].Action();
        }

        if (context.Input.Pressed(AcceptAction))
            _entries[SelectedIndex].Action();

        if (context.Input.Pressed(CancelAction))
            Cancelled?.Invoke();
    }

    /// <inheritdoc />
    protected override void OnDraw(EngineContext context)
    {
        if (_entries.Count == 0)
            return;

        SpriteFont font = context.Assets.Font(FontKey);
        Texture2D pixel = context.Assets.Texture(PixelKey);

        for (int i = 0; i < _entries.Count; i++)
        {
            Rectangle bounds = EntryBounds(i);
            bool selected = i == SelectedIndex;
            Color fill = selected ? ButtonHoverColor : ButtonColor;

            context.SpriteBatch.Draw(pixel, bounds, fill);
            context.SpriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), BorderColor);
            context.SpriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - 1, bounds.Width, 1), BorderColor);
            context.SpriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height), BorderColor);
            context.SpriteBatch.Draw(pixel, new Rectangle(bounds.Right - 1, bounds.Y, 1, bounds.Height), BorderColor);

            var textSize = font.MeasureString(_entries[i].Text);
            var textPos = new Vector2(
                bounds.X + (bounds.Width - textSize.X) / 2f,
                bounds.Y + (bounds.Height - textSize.Y) / 2f);

            context.SpriteBatch.DrawString(font, _entries[i].Text, textPos, TextColor);
        }
    }

    private Rectangle EntryBounds(int index)
    {
        int x = Position.X;
        int y = Position.Y + index * (EntryHeight + EntryGap);
        return new Rectangle(x, y, EntryWidth, EntryHeight);
    }

    private int EntryIndexAt(Point virtualMouse)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (EntryBounds(i).Contains(virtualMouse))
                return i;
        }

        return -1;
    }
}
