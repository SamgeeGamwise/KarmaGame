using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Sandbox.Game.Config;

internal static class DialogueSettingsLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static DialogueSettings LoadFromContent()
    {
        string dialogueDirectory = Path.Combine(AppContext.BaseDirectory, "Game", "Content", "Dialogue");
        if (!Directory.Exists(dialogueDirectory))
            throw new InvalidOperationException($"Dialogue content directory was not found: {dialogueDirectory}");

        string[] files = Directory.GetFiles(dialogueDirectory, "*.json", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (files.Length == 0)
            throw new InvalidOperationException($"No dialogue content files were found in: {dialogueDirectory}");

        var settings = new DialogueSettings();
        foreach (string filePath in files)
        {
            DialogueContentFile contentFile = LoadFile(filePath);
            settings.Conversations.AddRange(contentFile.Conversations);
            settings.Triggers.AddRange(contentFile.Triggers);
        }

        return settings;
    }

    private static DialogueContentFile LoadFile(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            DialogueContentFile? result = JsonSerializer.Deserialize<DialogueContentFile>(json, JsonOptions);
            if (result is null)
                throw new InvalidOperationException($"Dialogue file '{filePath}' deserialized to null.");

            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse dialogue file '{filePath}': {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to read dialogue file '{filePath}': {ex.Message}", ex);
        }
    }
}
