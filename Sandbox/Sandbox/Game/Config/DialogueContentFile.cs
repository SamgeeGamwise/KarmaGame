using System.Collections.Generic;

namespace Sandbox.Game.Config;

internal sealed class DialogueContentFile
{
    public List<DialogueConversationSettings> Conversations { get; set; } = [];

    public List<DialogueTriggerSettings> Triggers { get; set; } = [];
}
