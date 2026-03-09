using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sandbox.Game.Config;
using Sandbox.Game.Scene.Progression;

namespace Sandbox.Game.Scene.UI;

internal sealed class MenuOverlayNode
{
    private static readonly string[] Tabs =
    [
        "Overview",
        "Inventory",
        "Skills",
        "Lore",
        "Buildings"
    ];

    private readonly MenuSettings _settings;
    private readonly IReadOnlyDictionary<string, string> _keysByAction;
    private SpriteFont _font = null!;
    private Texture2D _pixel = null!;
    private int _tabIndex;

    public MenuOverlayNode(MenuSettings settings, IReadOnlyDictionary<string, string> keysByAction)
    {
        _settings = settings;
        _keysByAction = keysByAction;
    }

    public bool IsOpen { get; private set; }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _font = content.Load<SpriteFont>("UIFont");
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void Toggle()
    {
        IsOpen = !IsOpen;
    }

    public void Close()
    {
        IsOpen = false;
    }

    public void UpdateInput(Engine.Core.EngineFrameContext context)
    {
        if (!IsOpen)
            return;

        if (context.Input.Pressed(_settings.BackInputActionName))
        {
            Close();
            return;
        }

        if (context.Input.Pressed(_settings.NextItemInputActionName))
            _tabIndex = (_tabIndex + 1) % Tabs.Length;
        if (context.Input.Pressed(_settings.PreviousItemInputActionName))
            _tabIndex = (_tabIndex - 1 + Tabs.Length) % Tabs.Length;
    }

    public void DrawScreen(
        SpriteBatch spriteBatch,
        int virtualWidth,
        int virtualHeight,
        PlayerProgressState progression,
        IReadOnlyList<BuildingSettings> buildings,
        string activeMapAssetName)
    {
        if (!IsOpen)
            return;

        spriteBatch.Draw(_pixel, new Rectangle(0, 0, virtualWidth, virtualHeight), new Color(8, 11, 16, 198));

        int panelWidth = MathHelper.Clamp((int)(virtualWidth * 0.9f), 520, virtualWidth - 24);
        int panelHeight = MathHelper.Clamp((int)(virtualHeight * 0.86f), 300, virtualHeight - 20);
        Rectangle panel = new(
            (virtualWidth - panelWidth) / 2,
            (virtualHeight - panelHeight) / 2,
            panelWidth,
            panelHeight);
        Rectangle shadow = new(panel.X + 3, panel.Y + 3, panel.Width, panel.Height);
        int sidebarWidth = MathHelper.Clamp((int)(panel.Width * 0.24f), 180, 240);
        Rectangle sidebar = new(panel.X, panel.Y, sidebarWidth, panel.Height);
        Rectangle content = new(sidebar.Right + 1, panel.Y, panel.Width - sidebar.Width - 1, panel.Height);

        spriteBatch.Draw(_pixel, shadow, new Color(0, 0, 0, 90));
        spriteBatch.Draw(_pixel, panel, new Color(20, 24, 33, 246));
        spriteBatch.Draw(_pixel, sidebar, new Color(14, 18, 27, 255));
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Y, panel.Width, 2), new Color(211, 177, 103));
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Bottom - 1, panel.Width, 1), new Color(74, 90, 118));
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Y, 1, panel.Height), new Color(74, 90, 118));
        spriteBatch.Draw(_pixel, new Rectangle(panel.Right - 1, panel.Y, 1, panel.Height), new Color(74, 90, 118));
        spriteBatch.Draw(_pixel, new Rectangle(sidebar.Right, panel.Y, 1, panel.Height), new Color(96, 112, 142));

        spriteBatch.DrawString(_font, "Menu", new Vector2(sidebar.X + 12, sidebar.Y + 10), new Color(221, 231, 252));

        for (int i = 0; i < Tabs.Length; i++)
        {
            bool selected = i == _tabIndex;
            Rectangle tabRect = new(sidebar.X + 10, sidebar.Y + 36 + i * 30, sidebar.Width - 20, 24);
            spriteBatch.Draw(_pixel, tabRect, selected ? new Color(85, 106, 145, 220) : new Color(25, 30, 44, 90));
            spriteBatch.DrawString(
                _font,
                Tabs[i],
                new Vector2(tabRect.X + 9, tabRect.Y + 3),
                selected ? new Color(248, 251, 255) : new Color(176, 189, 217));
        }

        spriteBatch.DrawString(_font, Tabs[_tabIndex], new Vector2(content.X + 16, content.Y + 10), new Color(241, 245, 255));
        spriteBatch.Draw(_pixel, new Rectangle(content.X + 14, content.Y + 30, content.Width - 28, 1), new Color(88, 102, 130));
        DrawTabContent(spriteBatch, content, progression, buildings, activeMapAssetName);

        if (_settings.DrawControlHints)
        {
            string previousKey = ResolveKey(_settings.PreviousItemInputActionName);
            string nextKey = ResolveKey(_settings.NextItemInputActionName);
            string backKey = ResolveKey(_settings.BackInputActionName);
            string hints = $"[{previousKey}] / [{nextKey}] switch tabs, [{backKey}] close";
            spriteBatch.DrawString(_font, hints, new Vector2(panel.X + 14, panel.Bottom - 26), new Color(168, 182, 210));
        }
    }

    private string ResolveKey(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return "?";

        if (_keysByAction.TryGetValue(actionName, out string? keyLabel) &&
            !string.IsNullOrWhiteSpace(keyLabel))
        {
            return keyLabel;
        }

        return actionName;
    }

    private void DrawTabContent(
        SpriteBatch spriteBatch,
        Rectangle content,
        PlayerProgressState progression,
        IReadOnlyList<BuildingSettings> buildings,
        string activeMapAssetName)
    {
        Vector2 cursor = new(content.X + 16, content.Y + 40);
        int lineHeight = 19;

        foreach (string line in BuildLines(progression, buildings, activeMapAssetName))
        {
            spriteBatch.DrawString(_font, line, cursor, Color.White);
            cursor.Y += lineHeight;
            if (cursor.Y > content.Bottom - 32)
                break;
        }
    }

    private IEnumerable<string> BuildLines(PlayerProgressState progression, IReadOnlyList<BuildingSettings> buildings, string activeMapAssetName)
    {
        switch (_tabIndex)
        {
            case 0:
                yield return $"Level: {progression.Level}";
                yield return $"Money: {progression.Money}";
                yield return $"Current Map: {activeMapAssetName}";
                yield return $"Known Buildings: {buildings.Count}";
                yield return $"Inventory Slots Used: {progression.Inventory.Count}";
                yield break;

            case 1:
                if (progression.Inventory.Count == 0)
                {
                    yield return "No items yet.";
                    yield break;
                }

                foreach (string item in progression.Inventory)
                    yield return $"- {item}";
                yield break;

            case 2:
                if (progression.Skills.Count == 0)
                {
                    yield return "No skills yet.";
                    yield break;
                }

                foreach ((string skillName, int level) in progression.Skills)
                    yield return $"- {skillName}: Lv {level}";
                yield break;

            case 3:
                if (progression.LoreEntries.Count == 0)
                {
                    yield return "No lore unlocked.";
                    yield break;
                }

                foreach (string lore in progression.LoreEntries)
                    yield return $"- {lore}";
                yield break;

            case 4:
                if (buildings.Count == 0)
                {
                    yield return "No building metadata yet.";
                    yield break;
                }

                foreach (BuildingSettings building in buildings)
                {
                    string homeMarker = building.IsPlayerHome ? " (Home)" : string.Empty;
                    yield return $"- {building.DisplayName}{homeMarker}";
                    yield return $"  Exterior: {building.ExteriorMapAssetName}, Interior: {building.InteriorMapAssetName}";
                }

                yield break;
        }
    }
}
