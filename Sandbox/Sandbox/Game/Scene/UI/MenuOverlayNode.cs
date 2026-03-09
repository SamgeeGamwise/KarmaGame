using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sandbox.Game.Config;
using Sandbox.Game.Scene.Progression;

namespace Sandbox.Game.Scene.UI;

internal sealed class MenuOverlayNode(MenuSettings settings)
{
    private static readonly string[] Tabs =
    [
        "Overview",
        "Inventory",
        "Skills",
        "Lore",
        "Buildings"
    ];

    private readonly MenuSettings _settings = settings;
    private SpriteFont _font = null!;
    private Texture2D _pixel = null!;
    private int _tabIndex;

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

        spriteBatch.Draw(_pixel, new Rectangle(0, 0, virtualWidth, virtualHeight), Color.Black * 0.58f);

        Rectangle panel = new(48, 40, virtualWidth - 96, virtualHeight - 80);
        Rectangle sidebar = new(panel.X, panel.Y, 170, panel.Height);
        Rectangle content = new(sidebar.Right + 1, panel.Y, panel.Width - sidebar.Width - 1, panel.Height);

        spriteBatch.Draw(_pixel, panel, new Color(24, 26, 33, 240));
        spriteBatch.Draw(_pixel, sidebar, new Color(17, 19, 25, 255));
        spriteBatch.Draw(_pixel, new Rectangle(sidebar.Right, panel.Y, 1, panel.Height), new Color(92, 98, 120));

        for (int i = 0; i < Tabs.Length; i++)
        {
            bool selected = i == _tabIndex;
            Rectangle tabRect = new(sidebar.X + 8, sidebar.Y + 16 + i * 28, sidebar.Width - 16, 22);
            spriteBatch.Draw(_pixel, tabRect, selected ? new Color(84, 100, 132) : Color.Transparent);
            spriteBatch.DrawString(_font, Tabs[i], new Vector2(tabRect.X + 8, tabRect.Y + 3), selected ? Color.White : Color.LightGray);
        }

        DrawTabContent(spriteBatch, content, progression, buildings, activeMapAssetName);

        if (_settings.DrawControlHints)
        {
            string hints = $"[{_settings.PreviousItemInputActionName}] / [{_settings.NextItemInputActionName}] switch tab, [{_settings.BackInputActionName}] close";
            spriteBatch.DrawString(_font, hints, new Vector2(panel.X + 12, panel.Bottom - 28), Color.LightGray);
        }
    }

    private void DrawTabContent(
        SpriteBatch spriteBatch,
        Rectangle content,
        PlayerProgressState progression,
        IReadOnlyList<BuildingSettings> buildings,
        string activeMapAssetName)
    {
        Vector2 cursor = new(content.X + 14, content.Y + 16);
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
                yield return "Overview";
                yield return $"Level: {progression.Level}";
                yield return $"Money: {progression.Money}";
                yield return $"Current Map: {activeMapAssetName}";
                yield return $"Known Buildings: {buildings.Count}";
                yield return $"Inventory Slots Used: {progression.Inventory.Count}";
                yield break;

            case 1:
                yield return "Inventory";
                if (progression.Inventory.Count == 0)
                {
                    yield return "No items yet.";
                    yield break;
                }

                foreach (string item in progression.Inventory)
                    yield return $"- {item}";
                yield break;

            case 2:
                yield return "Skills";
                if (progression.Skills.Count == 0)
                {
                    yield return "No skills yet.";
                    yield break;
                }

                foreach ((string skillName, int level) in progression.Skills)
                    yield return $"- {skillName}: Lv {level}";
                yield break;

            case 3:
                yield return "Lore";
                if (progression.LoreEntries.Count == 0)
                {
                    yield return "No lore unlocked.";
                    yield break;
                }

                foreach (string lore in progression.LoreEntries)
                    yield return $"- {lore}";
                yield break;

            case 4:
                yield return "Buildings";
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
