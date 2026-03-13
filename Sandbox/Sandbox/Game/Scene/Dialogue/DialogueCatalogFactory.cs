using System;
using System.Collections.Generic;
using Engine.Dialogue;
using Sandbox.Game.Config;

namespace Sandbox.Game.Scene.Dialogue;

internal static class DialogueCatalogFactory
{
    public static DialogueCatalog Create(DialogueSettings settings)
    {
        var conversations = new List<DialogueConversation>();

        foreach (DialogueConversationSettings conversationSettings in settings.Conversations)
        {
            if (string.IsNullOrWhiteSpace(conversationSettings.ConversationId))
                continue;

            var variants = new List<DialogueVariant>();
            foreach (DialogueVariantSettings variantSettings in conversationSettings.Variants)
            {
                if (string.IsNullOrWhiteSpace(variantSettings.StartNodeId))
                    continue;

                var nodes = new Dictionary<string, DialogueNode>(StringComparer.Ordinal);
                foreach (DialogueNodeSettings nodeSettings in variantSettings.Nodes)
                {
                    if (string.IsNullOrWhiteSpace(nodeSettings.NodeId) ||
                        string.IsNullOrWhiteSpace(nodeSettings.Text))
                    {
                        continue;
                    }

                    var responses = new List<DialogueResponse>();
                    foreach (DialogueResponseSettings responseSettings in nodeSettings.Responses)
                    {
                        if (string.IsNullOrWhiteSpace(responseSettings.ResponseId) ||
                            string.IsNullOrWhiteSpace(responseSettings.Text))
                        {
                            continue;
                        }

                        var effects = new List<DialogueEffect>();
                        foreach (DialogueEffectSettings effectSettings in responseSettings.Effects)
                        {
                            if (!TryMapEffectType(effectSettings.EffectType, out DialogueActionType actionType) ||
                                string.IsNullOrWhiteSpace(effectSettings.Value))
                            {
                                continue;
                            }

                            effects.Add(new DialogueEffect(actionType, effectSettings.Value.Trim(), effectSettings.Extra.Trim()));
                        }

                        responses.Add(new DialogueResponse(
                            responseSettings.ResponseId.Trim(),
                            responseSettings.Text.Trim(),
                            responseSettings.NextNodeId.Trim(),
                            responseSettings.CloseDialogue,
                            effects,
                            CreateCondition(responseSettings.Conditions)));
                    }

                    nodes[nodeSettings.NodeId.Trim()] = new DialogueNode(
                        nodeSettings.NodeId.Trim(),
                        nodeSettings.Text.Trim(),
                        responses,
                        nodeSettings.NextNodeId.Trim(),
                        nodeSettings.CloseAfter,
                        CreateCondition(nodeSettings.Conditions),
                        nodeSettings.SpeakerName.Trim());
                }

                if (nodes.Count == 0)
                    continue;

                variants.Add(new DialogueVariant(
                    string.IsNullOrWhiteSpace(variantSettings.VariantId)
                        ? variantSettings.StartNodeId.Trim()
                        : variantSettings.VariantId.Trim(),
                    variantSettings.Priority,
                    Math.Max(1, variantSettings.Weight),
                    variantSettings.StartNodeId.Trim(),
                    nodes,
                    CreateCondition(variantSettings.Conditions),
                    variantSettings.SpeakerName.Trim()));
            }

            if (variants.Count == 0)
                continue;

            conversations.Add(new DialogueConversation(
                conversationSettings.ConversationId.Trim(),
                conversationSettings.SpeakerName.Trim(),
                variants));
        }

        return new DialogueCatalog(conversations);
    }

    private static DialogueCondition? CreateCondition(DialogueConditionSettings settings)
    {
        if (!HasAnyCondition(settings))
            return null;

        return new DialogueCondition(
            settings.EarliestMinutes,
            settings.LatestMinutes,
            settings.MinDayNumber,
            settings.MaxDayNumber,
            settings.AllowedWeekdays,
            settings.AllowedSeasons,
            settings.RequiredFlags,
            settings.ExcludedFlags,
            settings.RequiredQuestIds,
            settings.ExcludedQuestIds,
            settings.RandomChance);
    }

    private static bool HasAnyCondition(DialogueConditionSettings settings)
    {
        return settings.EarliestMinutes.HasValue ||
               settings.LatestMinutes.HasValue ||
               settings.MinDayNumber.HasValue ||
               settings.MaxDayNumber.HasValue ||
               settings.RandomChance < 1f ||
               settings.AllowedWeekdays.Count > 0 ||
               settings.AllowedSeasons.Count > 0 ||
               settings.RequiredFlags.Count > 0 ||
               settings.ExcludedFlags.Count > 0 ||
               settings.RequiredQuestIds.Count > 0 ||
               settings.ExcludedQuestIds.Count > 0;
    }

    private static bool TryMapEffectType(string rawEffectType, out DialogueActionType actionType)
    {
        switch (rawEffectType.Trim().ToLowerInvariant())
        {
            case "accept_quest":
                actionType = DialogueActionType.AcceptQuest;
                return true;
            case "set_flag":
                actionType = DialogueActionType.SetFlag;
                return true;
            case "add_lore":
                actionType = DialogueActionType.AddLoreEntry;
                return true;
            default:
                actionType = default;
                return false;
        }
    }
}
