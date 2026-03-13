using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Dialogue;

public sealed class DialogueCatalog
{
    private readonly Dictionary<string, List<DialogueConversation>> _conversationsById = new(StringComparer.OrdinalIgnoreCase);

    public DialogueCatalog(IEnumerable<DialogueConversation> conversations)
    {
        foreach (DialogueConversation conversation in conversations)
        {
            if (string.IsNullOrWhiteSpace(conversation.ConversationId))
                continue;

            string conversationId = conversation.ConversationId.Trim();
            if (!_conversationsById.TryGetValue(conversationId, out List<DialogueConversation>? bucket))
            {
                bucket = [];
                _conversationsById.Add(conversationId, bucket);
            }

            bucket.Add(conversation);
        }
    }

    public bool TryCreateSession(string conversationId, DialogueContext context, string speakerOverride, out DialogueSession? session)
    {
        session = null;

        if (string.IsNullOrWhiteSpace(conversationId) ||
            !_conversationsById.TryGetValue(conversationId.Trim(), out List<DialogueConversation>? conversations))
        {
            return false;
        }

        var matches = new List<(DialogueConversation Conversation, DialogueVariant Variant)>();

        foreach (DialogueConversation conversation in conversations)
        {
            foreach (DialogueVariant variant in conversation.Variants.Where(variant => DialogueConditionEvaluator.Matches(variant.Condition, context)))
                matches.Add((conversation, variant));
        }

        if (matches.Count == 0)
            return false;

        int highestPriority = matches.Max(candidate => candidate.Variant.Priority);
        List<(DialogueConversation Conversation, DialogueVariant Variant)> topCandidates = matches
            .Where(candidate => candidate.Variant.Priority == highestPriority)
            .ToList();

        (DialogueConversation Conversation, DialogueVariant Variant) selected = SelectWeighted(topCandidates, context.RandomSource);

        string speakerName = !string.IsNullOrWhiteSpace(speakerOverride)
            ? speakerOverride.Trim()
            : !string.IsNullOrWhiteSpace(selected.Variant.SpeakerName)
                ? selected.Variant.SpeakerName
                : selected.Conversation.SpeakerName;

        session = new DialogueSession(speakerName, selected.Variant.StartNodeId, selected.Variant.Nodes);
        return session.TryGetCurrentPage(context, out _);
    }

    private static (DialogueConversation Conversation, DialogueVariant Variant) SelectWeighted(
        IReadOnlyList<(DialogueConversation Conversation, DialogueVariant Variant)> candidates,
        Random randomSource)
    {
        if (candidates.Count == 1)
            return candidates[0];

        int totalWeight = 0;
        foreach ((DialogueConversation _, DialogueVariant variant) in candidates)
            totalWeight += Math.Max(1, variant.Weight);

        int roll = randomSource.Next(totalWeight);
        int running = 0;
        foreach ((DialogueConversation conversation, DialogueVariant variant) in candidates)
        {
            running += Math.Max(1, variant.Weight);
            if (roll < running)
                return (conversation, variant);
        }

        return candidates[^1];
    }
}
