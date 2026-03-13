using System;
using System.Collections.Generic;

namespace Sandbox.Game.Config;

internal static class DialogueSettingsValidator
{
    private static readonly HashSet<string> AllowedEffectTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "accept_quest",
        "set_flag",
        "add_lore"
    };

    public static void Validate(DialogueSettings settings, NpcSystemSettings npcs)
    {
        var errors = new List<string>();
        var conversationIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (DialogueConversationSettings conversation in settings.Conversations)
        {
            string conversationId = conversation.ConversationId.Trim();
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                errors.Add("Dialogue conversation is missing ConversationId.");
                continue;
            }

            if (!conversationIds.Add(conversationId))
                errors.Add($"Duplicate dialogue conversation id '{conversationId}'.");

            if (conversation.Variants.Count == 0)
            {
                errors.Add($"Dialogue conversation '{conversationId}' does not define any variants.");
                continue;
            }

            ValidateConversation(conversation, errors);
        }

        ValidateTriggers(settings.Triggers, conversationIds, errors);
        ValidateNpcConversationReferences(npcs, conversationIds, errors);

        if (errors.Count > 0)
            throw new InvalidOperationException("Dialogue content validation failed:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
    }

    private static void ValidateConversation(DialogueConversationSettings conversation, List<string> errors)
    {
        foreach (DialogueVariantSettings variant in conversation.Variants)
        {
            string variantId = string.IsNullOrWhiteSpace(variant.VariantId) ? "<missing>" : variant.VariantId.Trim();
            if (string.IsNullOrWhiteSpace(variant.StartNodeId))
                errors.Add($"Conversation '{conversation.ConversationId}' variant '{variantId}' is missing StartNodeId.");
            if (variant.Weight < 1)
                errors.Add($"Conversation '{conversation.ConversationId}' variant '{variantId}' must have Weight >= 1.");
            if (variant.Nodes.Count == 0)
            {
                errors.Add($"Conversation '{conversation.ConversationId}' variant '{variantId}' does not define any nodes.");
                continue;
            }

            var nodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DialogueNodeSettings node in variant.Nodes)
            {
                string nodeId = node.NodeId.Trim();
                if (string.IsNullOrWhiteSpace(nodeId))
                {
                    errors.Add($"Conversation '{conversation.ConversationId}' variant '{variantId}' contains a node without NodeId.");
                    continue;
                }

                if (!nodeIds.Add(nodeId))
                    errors.Add($"Conversation '{conversation.ConversationId}' variant '{variantId}' has duplicate node id '{nodeId}'.");

                if (string.IsNullOrWhiteSpace(node.Text))
                    errors.Add($"Conversation '{conversation.ConversationId}' variant '{variantId}' node '{nodeId}' has empty text.");

                ValidateResponses(conversation.ConversationId, variantId, node, errors);
            }

            if (!string.IsNullOrWhiteSpace(variant.StartNodeId) && !nodeIds.Contains(variant.StartNodeId.Trim()))
            {
                errors.Add($"Conversation '{conversation.ConversationId}' variant '{variantId}' starts at missing node '{variant.StartNodeId}'.");
            }

            foreach (DialogueNodeSettings node in variant.Nodes)
            {
                ValidateNodeLinks(conversation.ConversationId, variantId, node, nodeIds, errors);
            }
        }
    }

    private static void ValidateResponses(string conversationId, string variantId, DialogueNodeSettings node, List<string> errors)
    {
        var responseIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (DialogueResponseSettings response in node.Responses)
        {
            string responseId = response.ResponseId.Trim();
            if (string.IsNullOrWhiteSpace(responseId))
            {
                errors.Add($"Conversation '{conversationId}' variant '{variantId}' node '{node.NodeId}' contains a response without ResponseId.");
                continue;
            }

            if (!responseIds.Add(responseId))
                errors.Add($"Conversation '{conversationId}' variant '{variantId}' node '{node.NodeId}' has duplicate response id '{responseId}'.");

            if (string.IsNullOrWhiteSpace(response.Text))
                errors.Add($"Conversation '{conversationId}' variant '{variantId}' node '{node.NodeId}' response '{responseId}' has empty text.");

            foreach (DialogueEffectSettings effect in response.Effects)
            {
                if (!AllowedEffectTypes.Contains(effect.EffectType.Trim()))
                {
                    errors.Add($"Conversation '{conversationId}' variant '{variantId}' node '{node.NodeId}' response '{responseId}' uses unknown effect type '{effect.EffectType}'.");
                }

                if (string.IsNullOrWhiteSpace(effect.Value))
                    errors.Add($"Conversation '{conversationId}' variant '{variantId}' node '{node.NodeId}' response '{responseId}' has an effect with empty Value.");
            }
        }
    }

    private static void ValidateNodeLinks(
        string conversationId,
        string variantId,
        DialogueNodeSettings node,
        IReadOnlySet<string> nodeIds,
        List<string> errors)
    {
        if (!string.IsNullOrWhiteSpace(node.NextNodeId) && !nodeIds.Contains(node.NextNodeId.Trim()))
        {
            errors.Add($"Conversation '{conversationId}' variant '{variantId}' node '{node.NodeId}' points to missing next node '{node.NextNodeId}'.");
        }

        foreach (DialogueResponseSettings response in node.Responses)
        {
            if (!string.IsNullOrWhiteSpace(response.NextNodeId) && !nodeIds.Contains(response.NextNodeId.Trim()))
            {
                errors.Add($"Conversation '{conversationId}' variant '{variantId}' node '{node.NodeId}' response '{response.ResponseId}' points to missing node '{response.NextNodeId}'.");
            }
        }
    }

    private static void ValidateTriggers(
        IReadOnlyList<DialogueTriggerSettings> triggers,
        IReadOnlySet<string> conversationIds,
        List<string> errors)
    {
        var triggerIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (DialogueTriggerSettings trigger in triggers)
        {
            string triggerId = trigger.TriggerId.Trim();
            if (string.IsNullOrWhiteSpace(triggerId))
            {
                errors.Add("Dialogue trigger is missing TriggerId.");
                continue;
            }

            if (!triggerIds.Add(triggerId))
                errors.Add($"Duplicate dialogue trigger id '{triggerId}'.");

            if (string.IsNullOrWhiteSpace(trigger.MapAssetName))
                errors.Add($"Dialogue trigger '{triggerId}' is missing MapAssetName.");

            if (string.IsNullOrWhiteSpace(trigger.ConversationId))
            {
                errors.Add($"Dialogue trigger '{triggerId}' is missing ConversationId.");
            }
            else if (!conversationIds.Contains(trigger.ConversationId.Trim()))
            {
                errors.Add($"Dialogue trigger '{triggerId}' references missing conversation '{trigger.ConversationId}'.");
            }

            bool hasObjectTrigger = !string.IsNullOrWhiteSpace(trigger.TriggerObjectName);
            bool hasFallbackTrigger = trigger.InteractionRadius > 0f;
            if (!hasObjectTrigger && !hasFallbackTrigger)
            {
                errors.Add($"Dialogue trigger '{triggerId}' must define TriggerObjectName or InteractionRadius > 0.");
            }
        }
    }

    private static void ValidateNpcConversationReferences(
        NpcSystemSettings npcs,
        IReadOnlySet<string> conversationIds,
        List<string> errors)
    {
        foreach (NpcDefinitionSettings npc in npcs.Definitions)
        {
            if (string.IsNullOrWhiteSpace(npc.DialogueConversationId))
                continue;

            if (!conversationIds.Contains(npc.DialogueConversationId.Trim()))
            {
                errors.Add($"NPC '{npc.NpcId}' references missing dialogue conversation '{npc.DialogueConversationId}'.");
            }
        }
    }
}
