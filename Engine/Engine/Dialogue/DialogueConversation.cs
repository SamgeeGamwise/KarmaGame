using System.Collections.Generic;

namespace Engine.Dialogue;

public sealed record DialogueConversation(
    string ConversationId,
    string SpeakerName,
    IReadOnlyList<DialogueVariant> Variants);
